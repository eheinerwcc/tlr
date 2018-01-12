USE [TLR]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_UPSERT_TimesheetEntry_HOURLY]
(
	@TimesheetID int
	,@TimeSheetEntryID int = 0
	,@EntryTypeID varchar(3) = NULL
	,@EntryDate datetime
	,@EntryStartTime datetime
	,@EntryEndTime datetime
	,@Duration decimal(4,2)
	,@MealBreak int = 0
	,@MealBreakWaived bit = 0
    ,@ModifiedBy char(9)
	,@ErrorCode varchar(100) OUTPUT
)
AS

IF @EntryTypeID IS NULL
	/* Entry is time so don't do extra validation */
	begin
		IF @TimesheetEntryID = 0
			--insert entry
			begin
				INSERT INTO TimesheetEntry
				   ([TimesheetID]
				   ,[EntryDate]
				   ,[EntryStartTime]
				   ,[EntryEndTime]
				   ,[Duration]
				   ,[MealBreak]
				   ,[MealBreakWaived]
				   ,[CreatedBy]
					)
				 VALUES
					   (@TimesheetID
					   ,@EntryDate
					   ,@EntryStartTime
					   ,@EntryEndTime
					   ,@Duration
					   ,@MealBreak
					   ,@MealBreakWaived
					   ,@ModifiedBy
					   )
			end
		ELSE
			--update entry
			begin
				UPDATE TimesheetEntry
					SET [EntryDate] = @EntryDate
						,[EntryTypeID] = @EntryTypeID
						,[EntryStartTime] = @EntryStartTime
						,[EntryEndTime] = @EntryEndTime
						,[Duration] = @Duration
						,[MealBreak] = @MealBreak
						,[MealBreakWaived] = @MealBreakWaived
						,[ModifiedBy] = @ModifiedBy
						,[ModifiedDate] = GETDATE()
					WHERE TimesheetEntryID=@TimeSheetEntryID
			end
	end
ELSE
	/* entry is leave (or non-time) type, so validate against balance */
	begin

		declare @Today datetime
			,@HPLeaveTypeID char(3)
			,@SID char(9) --SID on this timesheet
			,@BeginDate datetime
			,@EndDate datetime

			--get the HP leave code for the entry attempted
			select @HPLeaveTypeID = HPLeaveTypeID from vw_EntryType where EntryTypeID = @EntryTypeID
			select @SID = SID 
			,@BeginDate=BeginDate
			,@EndDate = @EndDate
			from vw_Timesheet where TimesheetID=@TimesheetID
	
			set @ErrorCode = ''
	
			/***************************************************************************************
			Validate entry info
			***************************************************************************************/

			--**************************
			--CREATE a table with balances to validate against
			--**************************
			declare @Balance table (
				LeaveTypeID varchar(3)
				,Balance decimal(10,2)
				)

			if (DATEPART(YEAR, GETDATE()) = DATEPART(year, @EntryDate) and DATEPART(month, GETDATE()) <= DATEPART(month, @EntryDate))
			OR (DATEPART(year, @EntryDate) > DATEPART(YEAR, GETDATE()))
				begin
					--Current month or later - get current balance
					insert @Balance
					select 
					LeaveTypeID
					,Balance
					from vw_LeaveBalance
					where SID=@SID
				end
			else
				begin
					--past timesheet - obtain data from historical balance
					insert @Balance
					select 
					LeaveTypeID
					,EndBalance
					from vw_LeaveHistory
					where SID=@SID
					and DatePart(year, DateAdd(month, -1, @EntryDate)) = YearTaken
					and DatePart(month, DateAdd(month, -1, @EntryDate)) = MonthTaken

					insert @Balance
					select 
					LeaveTypeID
					,Balance
					from vw_LeaveBalance 
					where SID=@SID
					and LeaveTypeID not in (select LeaveTypeID from @Balance)
				end

			/** If user has no leave balance entries of submitted leave type then reject entry **/
			if (select count(LeaveTypeID) from @Balance where LeaveTypeID = @HPLeaveTypeID) = 0
				begin
					set @ErrorCode =  'Error_TSPartTime_BalanceExceeded'
					print @ErrorCode
					return
				end

			/* If has a leave type and leave is one of the validated types validate against leave balance */
			if @HPLeaveTypeID in (
				select LeaveTypeID from @Balance
				where LeaveTypeID in ('HSL','SSL')	--list of validated types
				)
				begin
					declare @CurrentBalance decimal(10,2) --Current balance for that entry type
					select @CurrentBalance = Balance from @Balance where LeaveTypeID=@HPLeaveTypeID
		
					if (
						select @CurrentBalance - isNULL(SUM(Duration), 0.00) - @Duration
						from vw_TimesheetEntry 
						where SID=@SID
						and HPLeaveTypeID = @HPLeaveTypeID
						and DATEPART(month, EntryDate) = DATEPART(month, @EntryDate)
						and DATEPART(year, EntryDate) = DATEPART(year, @EntryDate)
						and TimesheetStatusID <> 5
						and TimesheetEntryID <> @TimesheetEntryID) < 0
					begin
						set @ErrorCode =  'Error_TSPartTime_BalanceExceeded'
						return
					end

				end

			/** Otherwise process entry info **/
			if @TimesheetEntryID = 0 
				--insert new entry
				begin
					INSERT INTO TimesheetEntry
						   ([TimesheetID]
						   ,[EntryTypeID]
						   ,[EntryDate]
						   ,[EntryStartTime]
						   ,[EntryEndTime]
						   ,[Duration]
						   ,[MealBreak]
						   ,[MealBreakWaived]
						   ,[CreatedBy]
							)
					 VALUES
						   (@TimesheetID
						   ,@EntryTypeID
						   ,@EntryDate
						   ,@EntryStartTime
						   ,@EntryEndTime
						   ,@Duration
						   ,@MealBreak
						   ,@MealBreakWaived
						   ,@ModifiedBy
						   )
				end
			else 
				--update entry
				begin
					 UPDATE TimesheetEntry
						SET [EntryDate] = @EntryDate
							,[EntryTypeID] = @EntryTypeID
							,[EntryStartTime] = @EntryStartTime
							,[EntryEndTime] = @EntryEndTime
							,[Duration] = @Duration
							,[MealBreak] = @MealBreak
							,[MealBreakWaived] = @MealBreakWaived
							,[ModifiedBy] = @ModifiedBy
							,[ModifiedDate] = GETDATE()
						WHERE TimesheetEntryID=@TimeSheetEntryID
				end
	end

GO
GRANT EXECUTE ON [dbo].[usp_UPSERT_TimesheetEntry_HOURLY] TO [WebApplicationUser] AS [dbo]
GO