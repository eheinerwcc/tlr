USE [TLR]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Nicole Swan
-- Create date: 1/2/2018
-- Description:	Generates time and leave export files for 
--	hourly timesheets and marks timesheets as processed.
-- Updates:
-- =============================================
CREATE PROCEDURE [dbo].[usp_FileExport_HOURLY] 
(
	@SID char(9) -- person generating the file
	,@BeginDate datetime
	,@FileNameTime varchar(50) output
	,@FileNameLeave varchar(50) output
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	/*** SET file names ***/
	declare @DateStamp datetime
	set @DateStamp = getdate()

	set @FileNameTime = 'P' + dbo.uf_FormatDate(@DateStamp, 'yyymdhhhnn') + Cast(DATEPART(second, @DateStamp) as varchar(3)) + '.run'
	set @FileNameLeave = 'HL' + dbo.uf_FormatDate(@DateStamp, 'yyymdhhhnn') + Cast(DATEPART(second, @DateStamp) as varchar(3)) + '.run'

	/*** Generate table of overtime timesheets to exclude ***/
	
	-- set the endDate (which is needed by usp_SELECT_Timesheet_Overtime)
	declare @EndPeriodDate datetime
	SELECT distinct @EndPeriodDate = EndDate FROM vw_TimeSheet WHERE BeginDate = @BeginDate

	declare @OvertimeTimesheets table (
		TimesheetID int
		,[SID] char(9)
		,EmployeeName varchar(80)
		,DepartmentName varchar(50)
		,JobTitle varchar(16)
		,OvertimeAmount decimal(6,2)
	)
	insert into @OvertimeTimesheets
	exec usp_SELECT_Timesheet_Overtime @EndPeriodDate

	--/********** FOR DEBUGGING ************/
	--select * from @OvertimeTimesheets
	--/********** END DEBUGGING ************/

	-----------------------------TIME---------------------------------------------------
	/***** Add TIME export data entries *******/
	insert ExportTimeData (
	TimesheetID
	,BeginDate
	,SSN
	,SID
	,EmployeeName
	,PayrollSchedule
	,JobNumber
	,BudgetNumber
	,BudgetWorkHours
	,TotalWorkHours
	,EarningTypeID
	,PayRate
	,CreatedBy
	,CreatedOn
	,ExportFileName
	)
	select 
		b.TimesheetID
		,ts.BeginDate
		,EncryptByCert(Cert_ID('TLR'), SSN)
		,e.SID
		,RTrim(Left(e.LastName + ', ' + e.FirstName + ' ' + Left(e.MiddleName, 1), 30)) as EmpName
		,p.PayrollSchedule
		,ts.JobNumber
		,b.BudgetNumber
		,Cast(b.Hours * 100 as int) as TotalHours
		,Cast(tt.TotalTimesheetHours * 100 as int) as TotalTimesheetHours
		,b.EarningTypeID
		,Cast(ts.PayRate * 1000 as int) as PayRate
		,@SID as CreatedBy
		,@DateStamp as CreatedOn
		,@FileNameTime
	from TimesheetBudget b
	join vw_Timesheet ts on b.TimesheetID = ts.TimesheetID
	join vw_PayCycle p on ts.PayCycleCode = p.PayCycleCode and ts.BeginDate = p.BeginDate
	join vw_Employee e on ts.SID = e.SID
	join (
		select
			b.TimesheetID
			,Sum(b.Hours) as TotalTimesheetHours
		from TimesheetBudget b
		join vw_Timesheet ts on b.TimesheetID = ts.TimesheetID
		where ts.TimesheetTypeID=1
		and ts.TimesheetStatusID=4
		group by b.TimesheetID
	) tt on b.TimesheetID = tt.TimesheetID
	where 
	ts.BeginDate = @BeginDate
	and ts.TimesheetTypeID=1
	and ts.TimesheetStatusID=4
	and ts.TimesheetID not in (select TimesheetID from @OvertimeTimesheets)
	and Cast(b.Hours * 100 as int) is not NULL
	and Cast(b.Hours * 100 as int) <> 0
	and Cast(tt.TotalTimesheetHours * 100 as int) is not NULL

	---------------------------------------------------------------------------------
	-- Added by Nathan 11/23/09 to remove people from export w/ negative sick balance
	-- Updated by Nicole 12/20/17 to include HSL
	---------------------------------------------------------------------------------
	declare @NegSickTime table (TimesheetID int)

	insert @NegSickTime
	SELECT SID
	FROM vw_LeaveBalance
	where Balance < 0
	and LeaveTypeID in ('CSL', 'HSL', 'SSL')	--Updated to also include negative HSL,SSL balance

	-- delete everyone with negative sick leave that may be in the ExportTimeData table 
	delete from ExportTimeData
	WHERE SID in (select SID from @NegSickTime)
	and ExportFileName=@FileNameTime
	----------------------------------------------------------------------------------

	------------------------ END TIME -----------------------------------------------


	------------------------- LEAVE -------------------------------------------------
	/***** Add hourly LEAVE export data entries *****/
	insert ExportLeaveData (TimesheetID,SID,BeginDate,EndDate,LeaveTaken,LeaveTypeID,CreatedBy,CreatedOn,ExportFileName)
	select 
		ts.TimesheetID
		,ts.SID
		,dbo.uf_FormatDate(Cast(ts.BeginDate as datetime), 'yymmdd') as BeginDate
		,dbo.uf_FormatDate(Cast(ts.EndDate as datetime), 'yymmdd') as EndDate
		,Cast(Sum(e.Duration)*100 as int) as TimesheetTotal
		,e.HPLeaveTypeID
		,@SID as CreatedBy
		,@DateStamp as CreatedOn
		,@FileNameLeave as ExportFileName
	from vw_TimesheetEntry e
	join vw_Timesheet ts on e.TimesheetID = ts.TimesheetID
	where 
	e.TimesheetID not in (select TimesheetID from @OvertimeTimesheets) -- human needs to verify timesheets w/ OT
	and Cast(ts.BeginDate as datetime) = @BeginDate
	and ts.TimesheetTypeID=1
	and ts.TimesheetStatusID = 4
	and e.HPLeaveTypeID in ('HSL','SSL') -- hourly leave types to include in export
	group by ts.TimesheetID, e.HPLeaveTypeID, ts.BeginDate, ts.SID, ts.EndDate
	order by SID

	/*** Now deal with all the various conditions, special circumstance ****/
	---------------------------------------------------------------------------------
	-- Remove people from export w/ negative sick balance
	-- Updated to include hourly leave type and group by leave 
	--		type to allow multiple rows for an SID depending on leave type
	---------------------------------------------------------------------------------
	declare  @userSickTime TABLE (SID int ,sickTimeEntered dec(6,2))

	insert into @userSickTime (SID, sickTimeEntered) 
	select 
		e.SID
		,SUM(e.Duration) as sickTimeEntered
		from vw_TimesheetEntry e
		where DATEPART(YEAR, @BeginDate) = Datepart(YEAR, e.EntryDate)
		and DATEPART(month, @BeginDate) = Datepart(month, e.EntryDate)
		and e.TimesheetStatusID <> 5
		and e.HPLeaveTypeID in ('CSL','HSL','SSL')
		group by e.HPLeaveTypeID, e.SID	

	declare @usersWithNegSickTime TABLE (SID int)	
	insert into @usersWithNegSickTime
	select ust.SID
		from @userSickTime ust
		left join vw_LeaveBalance lb on lb.SID = ust.SID 
		where lb.LeaveTypeID in ('CSL','HSL','SSL')	-- Put sick leave types here
		AND (lb.Balance - ust.sickTimeEntered) < 0

	-- delete everyone with negative sick leave that may be in the ExportTimeData table 
	delete from ExportLeaveData
	WHERE SID in (select SID from @usersWithNegSickTime)
	and ExportFileName=@FileNameLeave

	---------------------------- END LEAVE ---------------------------------
	
	-------------------------- Final processing ----------------------------
	--create temp table of processed timesheets from leave and time tables
	declare @ProcessedTimesheets table (TimesheetID int)

	insert @ProcessedTimesheets
	select distinct TimesheetID from ExportLeaveData where ExportFileName=@FileNameLeave
	union 
	select distinct TimesheetID from ExportTimeData where ExportFileName=@FileNameTime

	/** debug **/
	--SELECT * from @ProcessedTimesheets
	/** end debug **/

	--This will set the status of all timesheets exported to "Processed"
	update Timesheet set
	TimesheetStatusID = 5 
	where TimesheetID in (select * from @ProcessedTimesheets)

	--Create an action for all processed timesheets
	insert TimesheetAction (TimesheetID,ActionTypeID,ActionBy,ActionDate)
	select 
	distinct TimesheetID
	,8 --Processed by payroll
	,@SID
	,@DateStamp
	from @ProcessedTimesheets
	---------------------- End processing ------------------------------

	------------------------ Output data -------------------------------

	--time
	select 
		Cast(DecryptByCert(Cert_ID('TLR'), SSN) as char(9))
		+[SID]
		+EmployeeName + Left('                              ', 30-Len(EmployeeName))
		+PayrollSchedule
		+JobNumber
		+BudgetNumber
		+Left('        ', 6-Len(BudgetWorkHours)) + BudgetWorkHours
		+Left('        ', 6-Len(TotalWorkHours)) + TotalWorkHours
		+EarningTypeID
		+Left('        ', 9-Len(PayRate)) + PayRate as FileLine
		,ExportFileName
	from ExportTimeData
	where ExportFileName=@FileNameTime

	--leave
	select 
		SID+BeginDate+EndDate+Right('000000', 6-Len(LeaveTaken))+LeaveTaken+LeaveTypeID as FileLine
		,ExportFileName
	from ExportLeaveData 
	where ExportFileName=@FileNameLeave
END
GO
GRANT EXECUTE ON [dbo].[usp_FileExport_HOURLY] TO [WebApplicationUser] AS [dbo]
GO
