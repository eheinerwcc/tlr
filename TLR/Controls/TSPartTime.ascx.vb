Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization

Partial Public Class TS_PartTime
  Inherits System.Web.UI.UserControl

  Public WriteOnly Property TSID() As Integer
    Set(ByVal value As Integer)
      intTSID = value
    End Set
  End Property

  Public WriteOnly Property IsSupervisor() As Boolean
    Set(ByVal value As Boolean)
      blnIsSupervisor = value
    End Set
  End Property

  Public WriteOnly Property IsOwner() As Boolean
    Set(ByVal value As Boolean)
      blnIsOwner = value
    End Set
  End Property

  Public WriteOnly Property TimesheetStatusID() As Integer
    Set(ByVal value As Integer)
      intTimesheetStatusID = value
    End Set
  End Property

  Public WriteOnly Property IsDelegated() As Boolean
    Set(ByVal value As Boolean)
      blnIsDelegated = value
    End Set
  End Property

  Private intTimesheetStatusID As Integer = 0
  Private blnIsOwner As Boolean = False
  Private blnIsSupervisor As Boolean = False
  Private intTSID As Integer = 0
    Private dtTimesheet As DataTable
    Private strJobEmployeeTypeID As String
    Private blnIsDelegated As Boolean = False
    Private blnContainsLeaveIneligibleBudget As Boolean = False

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Me.Visible Then
            uclFeedback.ResetDisplay()

            BindDataTable()
            strJobEmployeeTypeID = dtTimesheet.Rows(0).Item("JobEmployeeTypeID")

            If Not IsPostBack Then

                'BindDataTable()
                ViewState("TimesheetStatusID") = intTimesheetStatusID
                ViewState("blnEditableRemarkMode") = False
                ViewState("blnClickableGridMode") = False
                uclTSActionLog.TimesheetID = intTSID

                Select Case intTimesheetStatusID
                    Case My.Settings.TimesheetStatus_InProcess
                        If blnIsOwner Or blnIsDelegated Then
                            'Owner Edit Mode
                            ViewState("blnClickableGridMode") = True
                            ViewState("blnEditableRemarkMode") = True
                            pnlEntryDetails.Visible = True
                            BindEditControls()
                            pnlSubmissionInterface.Visible = True
                            btnAddRemark.Visible = True
                            h2Remarks.Visible = True
                            ViewState("ActionsColumnText") = "Actions"
                        End If
                    Case My.Settings.TimesheetStatus_AwaitingSupervisorApproval
                        If blnIsSupervisor Then
                            'Supervisor Edit Mode
                            ViewState("blnClickableGridMode") = True
                            ViewState("blnEditableRemarkMode") = True
                            pnlApprovalInterface.Visible = True
                            btnAddRemark.Visible = True
                            h2Remarks.Visible = True
                        Else
                            pnlBudgetAllocation.Visible = False
                        End If
                    Case My.Settings.TimesheetStatus_SentToPayroll
                        If clsSession.userIsPayrollAdmin Then
                            ViewState("blnClickableGridMode") = False 'payroll doesn't need clickable links
                            ViewState("blnEditableRemarkMode") = True
                            pnlApprovalInterface.Visible = True
                            btnAddRemark.Visible = True
                            h2Remarks.Visible = True
                            btnApproveTimesheet.Text = "Mark as Processed"
                            btnApproveTimesheet.OnClientClick = "return confirm('Are you sure you want to mark this timesheet as processed?')"
                        End If
                    Case My.Settings.TimesheetStatus_ProcessedByPayroll
                        'View Mode
                End Select

                BindTimesheetHeader()
                BindBudgetAllocationControls(intTSID)
                BindTimesheetGrid()
                BindTimesheetTotals()
                BindTimesheetRemarks()
                DisplayBudgetWarning()

            End If
            BindOnLoadJavascript()
        End If
        clsTimesheet.RedirectOnOutdatedStatus(intTSID, ViewState("TimesheetStatusID"))
    End Sub

    Private Sub BindOnLoadJavascript()
        Dim onloadScript As New System.Text.StringBuilder()
        onloadScript.Append("<script type='text/javascript'>")
        onloadScript.Append(vbCrLf)
        onloadScript.Append("ToggleMealVisibility();")
        onloadScript.Append("</script>")
        Page.ClientScript.RegisterStartupScript(Me.GetType(), "onLoadCall", onloadScript.ToString)
    End Sub

    Private Sub BindBudgetAllocationControls(ByVal intTimesheetID As Integer)
        Dim dsBudgets As DataSet = DBHelper.ExecuteDataset(CommandType.StoredProcedure, "usp_SELECT_TimesheetTotals_TIME", _
                                                                              New SqlParameter("@TimesheetID", intTimesheetID))

        Dim blnContainsGrantBudget As Boolean = False
        Dim strGrantBudgetAPPRs() As String = My.Settings.GrantBudgetAPPR.Trim.Replace(" ", "").ToString.Split(",")
        'Dim blnContainsLeaveIneligibleBudget As Boolean = False

        ViewState("BudgetCount") = dsBudgets.Tables(0).Rows.Count

        If dsBudgets.Tables(0).Rows.Count <> 0 Then
            For Each dr As DataRow In dsBudgets.Tables(0).Rows
                For Each strGrantBudgetAPPR As String In strGrantBudgetAPPRs
                    If Left(dr.Item("BudgetNumber").ToString, 3) = strGrantBudgetAPPR Then
                        blnContainsGrantBudget = True
                        'Exit For
                    End If
                    If Not clsTimesheet.IsValidLeaveEarningType(dr.Item("EarningTypeID")) Then
                        blnContainsLeaveIneligibleBudget = True
                    End If
                Next
            Next
        End If

        If blnContainsGrantBudget And intTimesheetStatusID <> My.Settings.TimesheetStatus_InProcess And dsBudgets.Tables(0).Rows.Count > 1 Then ViewState("ActionsColumnText") = "Budgets"

        If intTimesheetStatusID = My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then
            If blnIsSupervisor Then
                If dsBudgets.Tables(0).Rows.Count > 1 Then
                    If blnContainsGrantBudget Then
                        ViewState("dsBudgets") = dsBudgets
                    Else
                        pnlBudgetAllocation.Visible = True
                        fsSplitBudgets.Visible = True
                        rptBudgetAllocationSelection.DataSource = dsBudgets
                        rptBudgetAllocationSelection.DataBind()
                    End If
                End If
            End If
        ElseIf intTimesheetStatusID <> My.Settings.TimesheetStatus_InProcess And dsBudgets.Tables(0).Rows.Count > 1 Then
            pnlBudgetAllocation.Visible = True
            rptBudgetAllocationSelection.DataSource = dsBudgets
            rptBudgetAllocationSelection.DataBind()
        End If
    End Sub

    Private Sub BindTimesheetHeader()

        Dim datePeriodBegin As Date = dtTimesheet.Rows(0).Item("BeginDate").ToString
        Dim datePeriodEnd As Date = dtTimesheet.Rows(0).Item("EndDate").ToString

        lblTSPeriod.Text = datePeriodBegin & " - " & datePeriodEnd

        liUpdateTimesheet.Visible = (clsSession.userIsPayrollAdmin And intTimesheetStatusID <> My.Settings.TimesheetStatus_ProcessedByPayroll)
        lblStatus.Text = dtTimesheet.Rows(0).Item("StatusName")
        lblName.Text = dtTimesheet.Rows(0).Item("DisplayName") + " (" + dtTimesheet.Rows(0).Item("SID") + ")"
        If dtTimesheet.Rows(0).Item("EmploymentStatus") = "S" Then lblName.Text += " - Separated"
        lblJobTitle.Text = dtTimesheet.Rows(0).Item("JobClassNameLong")
        lblSupervisor.Text = dtTimesheet.Rows(0).Item("JobSupervisorDisplayName") + " (" + clsTimesheet.GetAlternateSignersDisplayString(intTSID) + ")"
        lblPayRate.Text = String.Format("{0:C}", dtTimesheet.Rows(0).Item("PayRate"))


        lblBudget.Text = clsTimesheet.GetBudgetsDisplayString(intTSID)
        lblDueDate.Text = DateAdd(DateInterval.Day, -2, dtTimesheet.Rows(0).Item("EndDate"))

        If blnIsDelegated Then
            lblDelegationInfo.Text = " on behalf of " + dtTimesheet.Rows(0).Item("DisplayName")
        End If

    End Sub

    Private Sub BindDataTable()
        dtTimesheet = DBHelper.ExecuteDataset(CommandType.StoredProcedure, "usp_SELECT_Timesheet", _
                                               New SqlParameter("@TimesheetID", intTSID)).Tables(0)
    End Sub

    Private Sub BindEditControls()
        Dim datePeriodBegin As Date = dtTimesheet.Rows(0).Item("BeginDate").ToString
        Dim datePeriodEnd As Date = dtTimesheet.Rows(0).Item("EndDate").ToString

        For i As Integer = 0 To DatePart(DateInterval.Day, datePeriodEnd) - DatePart(DateInterval.Day, datePeriodBegin)
            ddlDate.Items.Add(New ListItem(Left(datePeriodBegin.AddDays(i).DayOfWeek.ToString, 3) & " - " & datePeriodBegin.AddDays(i), datePeriodBegin.AddDays(i)))
        Next

        For i As Integer = 1 To 12
            ddlStartHour.Items.Add(New ListItem(IIf(i.ToString.Length = 1, "0" + i.ToString(), i), i))
            ddlEndHour.Items.Add(New ListItem(IIf(i.ToString.Length = 1, "0" + i.ToString(), i), i))
        Next
        ddlStartHour.SelectedValue = 7
        ddlEndHour.SelectedValue = 8

        For i As Integer = 0 To 45 Step 15
            ddlStartMinute.Items.Add(New ListItem(IIf(i.ToString.Length = 1, "0" + i.ToString(), i)))
            ddlEndMinute.Items.Add(New ListItem(IIf(i.ToString.Length = 1, "0" + i.ToString(), i)))
        Next

        For i As Integer = 0 To 120 Step 5
            ddlMealTime.Items.Add(New ListItem(i.ToString() + " minutes", i))
        Next

        lblTSPeriod.Text = datePeriodBegin & " - " & datePeriodEnd

        BindEditEntryTypes()
    End Sub

    Private Sub BindEditEntryTypes()
        'Bind leave type drop down list
        ddlEntryType.DataSource = DBHelper.ExecuteReader(CommandType.StoredProcedure, "usp_SELECT_EntryTypeForTimesheet",
                                                      New SqlParameter("@TimesheetID", intTSID))
        ddlEntryType.DataBind()
        liEntryType.Visible = (ddlEntryType.Items.Count > 0)
    End Sub

    Private Sub BindTimesheetTotals()
        rptTimesheetTotals.DataSource = DBHelper.ExecuteReader(CommandType.StoredProcedure, "usp_SELECT_TimesheetTotals_LEAVE",
                                                                    New SqlParameter("@TimesheetID", intTSID))
        rptTimesheetTotals.DataBind()
        pnlTimesheetTotals.Visible = (rptTimesheetTotals.Items.Count > 0)
    End Sub

    Private Sub BindTimesheetGrid()
        Dim dsTimesheetGridData As DataSet = DBHelper.ExecuteDataset(CommandType.StoredProcedure, "usp_SELECT_TimesheetGridData_TIME", _
                                                        New SqlParameter("@TimesheetID", intTSID))

        dsTimesheetGridData.Tables(0).TableName = "Weeks"
        dsTimesheetGridData.Tables(1).TableName = "Days"

        'Specify relationship for weeks and days
        Dim colParentWkNum As DataColumn = dsTimesheetGridData.Tables("Weeks").Columns("WeekNumber")
        Dim colChildWkNum As DataColumn = dsTimesheetGridData.Tables("Days").Columns("WeekNumber")
        Dim relWeekNumber As New DataRelation("relWeekNumber", colParentWkNum, colChildWkNum)
        dsTimesheetGridData.Relations.Add(relWeekNumber)

        rptWeeks.DataSource = dsTimesheetGridData.Tables("Weeks")
        rptWeeks.DataBind()

        Dim intTimesheetTotalMinutes As Integer = 0
        Dim intTimesheetLeaveMinutes As Integer = 0
        For Each dr As DataRow In dsTimesheetGridData.Tables(0).Rows
            intTimesheetTotalMinutes += CInt(clsGeneric.DeNull(dr.Item("TotalMinutes")))
            intTimesheetLeaveMinutes += CInt(clsGeneric.DeNull(dr.Item("TotalLeaveMinutes")))
        Next

        ViewState("TotalTimesheetMinutes") = intTimesheetTotalMinutes.ToString
        ViewState("TotalTimesheetLeaveMinutes") = intTimesheetLeaveMinutes.ToString
        lblGrandTotalHours.Text = IIf(ViewState("TotalTimesheetMinutes") > 0, Math.Floor(ViewState("TotalTimesheetMinutes") / 60) & " <abbr title='hours'>hrs</abbr> ", "") + IIf((ViewState("TotalTimesheetMinutes") Mod 60) > 0, ViewState("TotalTimesheetMinutes") Mod 60 & " <abbr title='minutes'>mins</abbr>", "") 'ViewState("TotalTimesheetMinutes")
        lblBudgetAllocationTotalHours.Text = ViewState("TotalTimesheetMinutes") / 60

    End Sub

    Private Sub DisplayBudgetWarning()
        'Display leave warning as applicable based on budget, for supervisor view
        If blnIsSupervisor AndAlso intTimesheetStatusID = My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then
            If blnContainsLeaveIneligibleBudget AndAlso ViewState("TotalTimesheetLeaveMinutes") IsNot Nothing AndAlso ViewState("TotalTimesheetLeaveMinutes") > 0 Then
                Dim leaveHours = ViewState("TotalTimesheetLeaveMinutes") / 60

                lblLeaveWarning.Text = String.Format(Resources.GlobalText.Warning_LeaveTotalWithInvalidBudgetEarningType, leaveHours, My.Settings.BudgetEarningType_LeaveNotAllowed)
                lblLeaveWarning.Visible = True
            End If
        End If
    End Sub

    Protected Sub rptWeeks_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles rptWeeks.ItemDataBound
        If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then

            Dim rptDays As Repeater = e.Item.FindControl("rptDays")
            If Not IsNothing(rptDays) Then
                rptDays.DataSource = CType(e.Item.DataItem, DataRowView).CreateChildView("relWeekNumber")
                rptDays.DataBind()
            End If
        End If

        'Only display leave row if employee job is hourly leave eligible
        If clsTimesheet.IsHourlyLeaveEligible(strJobEmployeeTypeID) Then
            Dim tr As HtmlTableRow = CType(e.Item.FindControl("rowTotalLeaveHours"), HtmlTableRow)
            If tr IsNot Nothing Then
                tr.Visible = True
            End If
        End If

    End Sub

    Private Sub BindTimesheetRemarks()

        Using remarkdt As DataTable = DBHelper.ExecuteReader(CommandType.StoredProcedure, "usp_SELECT_TimesheetRemark", _
                                            New SqlParameter("@TimesheetID", intTSID))
            rptRemarks.DataSource = remarkdt
            rptRemarks.DataBind()
        End Using

        rptRemarks.Visible = (rptRemarks.Items.Count <> 0)
        h2Remarks.Visible = ((rptRemarks.Items.Count <> 0) Or btnAddRemark.Visible)
        txtRemark.Text = ""
        pnlEditRemark.Visible = False
    End Sub

    Protected Sub EditRemark(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim ds As DataSet = DBHelper.ExecuteDataset(CommandType.StoredProcedure, "usp_SELECT_TimesheetRemark", _
                                                     New SqlParameter("@TimesheetRemarkID", DirectCast(sender, Button).CommandArgument))

        If ds.Tables(0).Rows.Count = 1 Then
            txtRemark.Text = Server.HtmlDecode(ds.Tables(0).Rows(0).Item("RemarkText").ToString)
            btnSaveRemark.CommandArgument = DirectCast(sender, Button).CommandArgument
            pnlEditRemark.Visible = True
            btnAddRemark.Visible = False
        End If
    End Sub

    Protected Sub DeleteRemark(ByVal sender As Object, ByVal e As System.EventArgs)
        DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_DELETE_TimesheetRemark", _
                              New SqlParameter("@TimesheetRemarkID", DirectCast(sender, Button).CommandArgument))
        BindTimesheetRemarks()
    End Sub

    Private Sub btnAddRemark_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddRemark.Click
        pnlEditRemark.Visible = True
        btnSaveRemark.CommandArgument = ""
        btnAddRemark.Visible = False
    End Sub

    Private Sub btnCancelRemarkUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancelRemarkUpdate.Click
        txtRemark.Text = ""
        btnSaveRemark.CommandArgument = ""
        pnlEditRemark.Visible = False
        btnAddRemark.Visible = True
    End Sub

    Private Sub btnSaveRemark_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSaveRemark.Click

        If txtRemark.Text.Trim.Length > 0 Then
            If btnSaveRemark.CommandArgument = "" Then
                'new remark
                DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_INSERT_TimesheetRemark", _
                  New SqlParameter("@TimesheetID", intTSID), _
                  New SqlParameter("@SID", clsSession.userSID), _
                  New SqlParameter("@RemarkText", Server.HtmlEncode(txtRemark.Text)))
            Else
                'update remark
                DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_UPDATE_TimesheetRemark", _
                  New SqlParameter("@TimesheetRemarkID", btnSaveRemark.CommandArgument), _
                  New SqlParameter("@SID", clsSession.userSID), _
                  New SqlParameter("@RemarkText", Server.HtmlEncode(txtRemark.Text)))
            End If
        Else
            uclFeedback.DisplayError(Resources.GlobalText.Error_TSFullTime_InvalidRemarkLength)
        End If

        BindTimesheetRemarks()
        txtRemark.Text = ""
        pnlEditRemark.Visible = False
        btnAddRemark.Visible = True
        btnSaveRemark.CommandArgument = ""
    End Sub

    Private Function IsValidEntry(ByVal startTime As Date, ByVal endTime As Date, ByRef intMealTime As Integer, Optional ByVal intTimesheetEntryID As Integer = -1) As Boolean
        Dim blnValidEntry As Boolean = True

        If startTime > endTime Then
            blnValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TimeEntry_EndBeforeStart)
        ElseIf startTime = endTime Then
            blnValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TimeEntry_StartEqualsEnd)
        End If

        If blnValidEntry AndAlso ddlMealTime.SelectedItem.Value <> 0 Then
            If endTime.Subtract(startTime).TotalMinutes <= ddlMealTime.SelectedItem.Value Then
                blnValidEntry = False
                uclFeedback.DisplayError(Resources.GlobalText.Error_TimeEntry_MealExceedsShift)
            End If
        End If

        If blnValidEntry AndAlso ddlMealTime.SelectedValue <> 0 AndAlso DateDiff(DateInterval.Hour, startTime, endTime) < 5 Then
            'blnValidEntry = False
            'uclFeedback.DisplayError(Resources.GlobalText.Error_TSParttime_MealBreakNotAllowed)

            blnValidEntry = True

            'remove the meal break if the difference is less than 5 hours.
            ddlMealTime.ClearSelection()
            intMealTime = 0

        End If

        If blnValidEntry AndAlso DBHelper.ExecuteScalar(CommandType.StoredProcedure, "usp_IS_OverlappingTIMEentry",
                                   New SqlParameter("@TimesheetID", intTSID),
                                   New SqlParameter("@TimesheetEntryID", IIf(intTimesheetEntryID = -1, DBNull.Value, intTimesheetEntryID)),
                                   New SqlParameter("@EntryStartTime", startTime),
                                   New SqlParameter("@EntryEndTime", endTime)) Then
            blnValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TimeEntry_OverlappingEntry)
        End If

        Return blnValidEntry
    End Function

    Private Sub btnAddEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddEntry.Click
        Dim strStartTime As String = ddlStartHour.SelectedItem.Value + ":" + ddlStartMinute.SelectedItem.Value + " " + ddlStartAMPM.SelectedItem.Value
        Dim strEndTime As String = ddlEndHour.SelectedItem.Value + ":" + ddlEndMinute.SelectedItem.Value + " " + ddlEndAMPM.SelectedItem.Value
        Dim endTime As Date
        Dim startTime As Date = ddlDate.SelectedItem.Value + " " + strStartTime
        Dim intMealTime As Integer = ddlMealTime.SelectedItem.Value

        If strEndTime = "12:00 AM" Then
            endTime = CDate(ddlDate.SelectedItem.Value).AddDays(1) + " " + strEndTime
        Else
            endTime = ddlDate.SelectedItem.Value + " " + strEndTime
        End If


        If IsValidEntry(startTime, endTime, intMealTime) Then
            'Calculate duration
            Dim duration = clsTimesheet.CalcuateTimesheetEntryDuration(startTime, endTime)

            'Set up variable to hold possible returned error code
            Dim parErrorCode As New SqlParameter
            parErrorCode.ParameterName = "@ErrorCode"
            parErrorCode.Direction = ParameterDirection.Output
            parErrorCode.DbType = DbType.String
            parErrorCode.Size = 100

            Dim entryType = Nothing
            If chkbxEnterType.Checked Then
                entryType = ddlEntryType.SelectedItem.Value
            End If

            'Try to insert entry
            DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_UPSERT_TimesheetEntry_HOURLY",
                                            New SqlParameter("@TimesheetID", intTSID),
                                            New SqlParameter("@EntryTypeID", entryType),
                                            New SqlParameter("@EntryDate", ddlDate.SelectedItem.Value),
                                            New SqlParameter("@EntryStartTime", startTime),
                                            New SqlParameter("@EntryEndTime", endTime),
                                            New SqlParameter("@Duration", duration),
                                            New SqlParameter("@MealBreak", intMealTime),
                                            New SqlParameter("@ModifiedBy", clsSession.userSID),
                                            parErrorCode)

            If Not IsDBNull(parErrorCode.Value) AndAlso parErrorCode.Value <> "" Then
                uclFeedback.DisplayError(Resources.GlobalText.ResourceManager.GetString(parErrorCode.Value.ToString))
            Else
                BindOnLoadJavascript()
                BindTimesheetGrid()
                BindTimesheetTotals()

                'Grab the date submitted and select the next date in the dropdown list on page refresh
                Dim dateAdded As DateTime
                Dim dateAddedNext As DateTime
                Dim dateAddedNextString As String

                'grab the date that the person just added to their timesheet
                dateAdded = Convert.ToDateTime(ddlDate.SelectedItem.Value())

                'select the next day after that day
                dateAddedNext = dateAdded.AddDays(1)

                'BindTSDataTable()
                BindDataTable()

                'grab the enddate of that time period
                Dim datePeriodEnd As Date = dtTimesheet.Rows(0).Item("EndDate").ToString

                'if the day the person added to their timesheet is the last day of the time period
                'do not increment the dropdown list to the next day.
                If (Not (Convert.ToDateTime(datePeriodEnd) = dateAdded)) Then
                    dateAddedNextString = dateAddedNext.ToString("d")
                    ddlDate.Items.FindByValue(dateAdded).Selected = False
                    ddlDate.Items.FindByValue(dateAddedNextString).Selected = True
                End If

                'Clear "add as leave" checkbox
                chkbxEnterType.Checked = False

                'Grab the date submitted and select the next date in the dropdown list on page refresh
                'Dim dateAdded As DateTime
                'Dim dateAddedNext As DateTime
                'Dim dateAddedNextString As String
                'dateAdded = Convert.ToDateTime(ddlDate.SelectedItem.Value())
                'dateAddedNext = dateAdded.AddDays(1)
                'dateAddedNextString = dateAddedNext.ToString("d")
                'ddlDate.Items.FindByValue(dateAdded).Selected = False
                'ddlDate.Items.FindByValue(dateAddedNextString).Selected = True
            End If

        End If
    End Sub

    Private Sub PreapreEditableEntry(ByVal blnEdit As Boolean)
        btnAddEntry.Visible = Not blnEdit
        btnCancelUpdate.Visible = blnEdit
        btnDeleteEntry.Visible = blnEdit
        btnUpdateEntry.Visible = blnEdit

        btnUpdateEntry.CommandArgument = ""
        btnDeleteEntry.CommandArgument = ""

        ddlDate.ClearSelection()
        ddlStartHour.ClearSelection()
        ddlStartMinute.ClearSelection()
        ddlStartAMPM.ClearSelection()
        ddlEndHour.ClearSelection()
        ddlEndMinute.ClearSelection()
        ddlEndAMPM.ClearSelection()

        ddlStartHour.SelectedValue = 7
        ddlEndHour.SelectedValue = 8

        litHeader.Text = IIf(blnEdit, "Edit Time Entry", "Add Time Entry")

        chkbxEnterType.Checked = False
    End Sub

    Protected Sub EditEntry(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim intTimesheetEntryID As Integer = DirectCast(sender, Button).CommandArgument
        Dim dtTimesheetEntry As DataTable = DBHelper.ExecuteDataset(CommandType.StoredProcedure, "usp_SELECT_TimesheetEntry", _
                                                                     New SqlParameter("@TimesheetEntryID", intTimesheetEntryID)).Tables(0) '(From t In TLRData.vw_TimesheetEntries Where t.TimesheetEntryID = intTimesheetEntryID)

        If dtTimesheetEntry.Rows.Count > 0 Then
            PreapreEditableEntry(True)

            With dtTimesheetEntry.Rows(0)
                Dim intStartHour As Integer = IIf(CDate(.Item("EntryStartTime")).Hour = 0, 12, CDate(.Item("EntryStartTime")).Hour)
                Dim intEndHour As Integer = IIf(CDate(.Item("EntryEndTime")).Hour = 0, 12, CDate(.Item("EntryEndTime")).Hour)

                ddlDate.SelectedValue = CDate(.Item("EntryDate")).ToShortDateString
                ddlMealTime.SelectedValue = .Item("MealBreak")

                ddlStartHour.SelectedValue = IIf(intStartHour <= 12, intStartHour, intStartHour - 12)
                ddlStartMinute.SelectedValue = IIf(CDate(.Item("EntryStartTime")).Minute.ToString.Length = 1, "0" + CDate(.Item("EntryStartTime")).Minute.ToString, CDate(.Item("EntryStartTime")).Minute.ToString)
                ddlStartAMPM.SelectedValue = IIf(CDate(.Item("EntryStartTime")).TimeOfDay.Hours < 12, "AM", "PM") 'IIf(intStartHour < 12, "AM", "PM")

                ddlEndHour.SelectedValue = IIf(intEndHour <= 12, intEndHour, intEndHour - 12)
                ddlEndMinute.SelectedValue = IIf(CDate(.Item("EntryEndTime")).Minute.ToString.Length = 1, "0" + CDate(.Item("EntryEndTime")).Minute.ToString, CDate(.Item("EntryEndTime")).Minute.ToString)
                ddlEndAMPM.SelectedValue = IIf(CDate(.Item("EntryEndTime")).TimeOfDay.Hours < 12, "AM", "PM") 'IIf(intEndHour < 12, "AM", "PM") 'CDate(.Item("EntryEndTime")).

                If Not IsDBNull(.Item("EntryTypeID")) Then
                    'It is not time so show as leave
                    chkbxEnterType.Checked = True
                    ddlEntryType.SelectedValue = .Item("EntryTypeID")
                End If
            End With

            btnUpdateEntry.CommandArgument = intTimesheetEntryID
            btnDeleteEntry.CommandArgument = intTimesheetEntryID
            BindOnLoadJavascript()
        End If
    End Sub

    Protected Sub DeleteEntry(ByVal sender As Object, ByVal e As System.EventArgs)
        DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_DELETE_TimesheetEntry", _
                              New SqlParameter("@TimesheetEntryID", DirectCast(sender, Button).CommandArgument))
        BindTimesheetGrid()
        BindTimesheetTotals()
    End Sub

    Private Sub btnCancelUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancelUpdate.Click
        PreapreEditableEntry(False)
        BindOnLoadJavascript()
    End Sub

    Private Sub btnUpdateEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpdateEntry.Click
        Dim strStartTime As String = ddlStartHour.SelectedItem.Value + ":" + ddlStartMinute.SelectedItem.Value + " " + ddlStartAMPM.SelectedItem.Value
        Dim strEndTime As String = ddlEndHour.SelectedItem.Value + ":" + ddlEndMinute.SelectedItem.Value + " " + ddlEndAMPM.SelectedItem.Value
        Dim endTime As Date
        Dim startTime As Date = ddlDate.SelectedItem.Value + " " + strStartTime
        Dim intMealTime As Integer = ddlMealTime.SelectedItem.Value

        If strEndTime = "12:00 AM" Then
            endTime = CDate(ddlDate.SelectedItem.Value).AddDays(1) + " " + strEndTime
        Else
            endTime = ddlDate.SelectedItem.Value + " " + strEndTime
        End If

        If IsValidEntry(startTime, endTime, intMealTime, btnUpdateEntry.CommandArgument) Then
            'Calculate duration
            Dim duration = clsTimesheet.CalcuateTimesheetEntryDuration(startTime, endTime)

            'Set up variable to hold possible returned error code
            Dim parErrorCode As New SqlParameter
            parErrorCode.ParameterName = "@ErrorCode"
            parErrorCode.Direction = ParameterDirection.Output
            parErrorCode.DbType = DbType.String
            parErrorCode.Size = 100

            Dim entryType = Nothing
            If chkbxEnterType.Checked Then
                entryType = ddlEntryType.SelectedItem.Value
            End If

            DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_UPSERT_TimesheetEntry_HOURLY",
                                New SqlParameter("@TimesheetID", intTSID),
                                New SqlParameter("@TimesheetEntryID", btnUpdateEntry.CommandArgument),
                                New SqlParameter("@EntryTypeID", entryType),
                                New SqlParameter("@EntryDate", ddlDate.SelectedItem.Value),
                                New SqlParameter("@EntryStartTime", startTime),
                                New SqlParameter("@EntryEndTime", endTime),
                                New SqlParameter("@Duration", duration),
                                New SqlParameter("@MealBreak", intMealTime),
                                New SqlParameter("@ModifiedBy", clsSession.userSID),
                                parErrorCode)

            'DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_UPDATE_TimesheetEntry_TIME",
            '                                New SqlParameter("@TimesheetEntryID", btnUpdateEntry.CommandArgument),
            '                                New SqlParameter("@EntryDate", ddlDate.SelectedItem.Value),
            '                                New SqlParameter("@EntryStartTime", startTime),
            '                                New SqlParameter("@EntryEndTime", endTime),
            '                                New SqlParameter("@MealBreak", intMealTime),
            '                                New SqlParameter("@ModifiedBy", clsSession.userSID))

            If Not IsDBNull(parErrorCode.Value) AndAlso parErrorCode.Value <> "" Then
                uclFeedback.DisplayError(Resources.GlobalText.ResourceManager.GetString(parErrorCode.Value.ToString))
            Else
                'BindDataTable()
                BindTimesheetGrid()
                BindTimesheetTotals()
                PreapreEditableEntry(False)
                BindOnLoadJavascript()
            End If
        End If
    End Sub

    Private Sub btnDeleteEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteEntry.Click
        DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_DELETE_TimesheetEntry", _
                                      New SqlParameter("@TimesheetEntryID", btnDeleteEntry.CommandArgument.ToString))

        PreapreEditableEntry(False)
        BindOnLoadJavascript()

        BindTimesheetGrid()
        BindTimesheetTotals()
    End Sub

    Private Sub btnDeleteTimesheet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteTimesheet.Click
        clsTimesheet.DeleteTimesheet(intTSID)
        Response.Redirect("Home.aspx")
    End Sub

    Private Sub btnSubmitTimesheet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSubmitTimesheet.Click
        If Not clsTimesheet.IsTimesheetEmpty(intTSID) Then

            Dim sendEmailSuper As String
            sendEmailSuper = ConfigurationManager.AppSettings.Get("EmailSupervisor_UponParttimeTimesheetSubmission")

            If (sendEmailSuper = "True") Then
                'send email to super
                clsNotify.SendTemplateEmail(intTSID, Resources.GlobalText.Email_TimesheetSubmitted_SupervisorNotification, True)
            End If

            clsTimesheet.RouteTimesheet(intTSID)
            clsTimesheet.LogTimesheetAction(intTSID, My.Settings.TimesheetAction_SubmitTimesheet, clsSession.userSID, "")
            Response.Redirect("Home.aspx")
        Else
            uclFeedback.DisplayError(Resources.GlobalText.Error_TIMESHEET_EmptyTimesheet)
        End If
    End Sub

    Private Function SubmitBudgetEntries() As Boolean
        Dim blnAreBudgetsValid As Boolean = True
        Dim blnSplitByPercent As Boolean = IIf(pnlBudgetAllocation.Visible, (ddlBudgetAllocationSplitType.SelectedValue = "Percent"), False)
        Dim dblTotalTimesheetHours As Double = Format(CInt(ViewState("TotalTimesheetMinutes")) / 60, "#.##")
        Dim dblTotalAllocation As Double = 0
        Dim strBudgetAllocationDBUpload As String = ""
        Dim strTempBudgetAllocation As String = ""

        If ViewState("BudgetCount") = 0 Then
            blnAreBudgetsValid = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TSParttime_NoBudget)
        ElseIf pnlBudgetAllocation.Visible Then 'multiple budgets, none grant related
            blnSplitByPercent = (ddlBudgetAllocationSplitType.SelectedValue = "Percent")

            'format:  "BudgetNumber|Hours;BudgetNumber:Hours"
            For Each rpi As RepeaterItem In rptBudgetAllocationSelection.Items
                strTempBudgetAllocation = DirectCast(rpi.FindControl("txtBudgetAllocationAmount"), TextBox).Text.Trim
                strTempBudgetAllocation = IIf(strTempBudgetAllocation = "", "0.0", strTempBudgetAllocation)

                If clsGeneric.IsExpectedNumber(strTempBudgetAllocation, 2) AndAlso strTempBudgetAllocation >= 0 Then
                    If blnSplitByPercent And strTempBudgetAllocation.Contains(".") Then

                        If (strTempBudgetAllocation.Contains("0.00")) Then
                            blnAreBudgetsValid = True
                        Else
                            blnAreBudgetsValid = False
                            uclFeedback.DisplayError(Resources.GlobalText.Error_TSParttime_PercentAllocationMustBeWholeNumber)
                        End If

                        Exit For
                    Else
                        strTempBudgetAllocation = IIf(blnSplitByPercent, strTempBudgetAllocation / 100 * dblTotalTimesheetHours, strTempBudgetAllocation)
                        strBudgetAllocationDBUpload += DirectCast(rpi.FindControl("lblBudgetAllocationNumber"), Label).Text + "|"
                        strBudgetAllocationDBUpload += strTempBudgetAllocation + ";"
                        dblTotalAllocation += CDbl(strTempBudgetAllocation)
                    End If
                Else
                    blnAreBudgetsValid = False
                    uclFeedback.DisplayError("'" + Server.HtmlEncode(strTempBudgetAllocation) + Resources.GlobalText.Error_TSParttime_InvalidAllocationText)
                    Exit For
                End If
            Next

            If blnAreBudgetsValid Then
                If dblTotalAllocation <> dblTotalTimesheetHours Then
                    blnAreBudgetsValid = False
                    uclFeedback.DisplayError(Resources.GlobalText.Error_TSParttime_InvalidTimesheetAllocation)
                Else
                    clsTimesheet.UploadBudgetDataPerTimesheet(intTSID, strBudgetAllocationDBUpload)
                End If
            End If
        ElseIf ViewState("dsBudgets") IsNot Nothing Then 'multiple budgets, at least one grant related - per entry issue
            blnSplitByPercent = (ddlBudgetAllocationSplitTypePerEntry.SelectedValue = "Percent")
            Dim strBudgetAllocationDBUploadFinal As String = ""

            For Each rpi As RepeaterItem In rptWeeks.Items
                For Each rpi2 As RepeaterItem In DirectCast(rpi.FindControl("rptDays"), Repeater).Items
                    dblTotalAllocation = 0
                    strBudgetAllocationDBUpload = ""
                    If DirectCast(rpi2.FindControl("rptBudgetAllocationPerEntry"), Repeater).Visible And blnAreBudgetsValid Then
                        For Each rpi3 As RepeaterItem In DirectCast(rpi2.FindControl("rptBudgetAllocationPerEntry"), Repeater).Items
                            strTempBudgetAllocation = DirectCast(rpi3.FindControl("txtBudgetAllocationAmount"), TextBox).Text.Trim
                            strTempBudgetAllocation = IIf(strTempBudgetAllocation = "", "0.0", strTempBudgetAllocation)

                            If clsGeneric.IsExpectedNumber(strTempBudgetAllocation, 2) AndAlso strTempBudgetAllocation >= 0 Then
                                If blnSplitByPercent And strTempBudgetAllocation.Contains(".") Then
                                    blnAreBudgetsValid = False
                                    uclFeedback.DisplayError(Resources.GlobalText.Error_TSParttime_PercentAllocationMustBeWholeNumber)
                                    Return False
                                Else
                                    strTempBudgetAllocation = IIf(blnSplitByPercent, strTempBudgetAllocation / 100 * CDbl(DirectCast(rpi3.FindControl("lblTotalEntryMinutes"), Label).Text) / 60, strTempBudgetAllocation)
                                    strBudgetAllocationDBUpload += DirectCast(rpi3.FindControl("lblEntryID"), Label).Text + "|" + DirectCast(rpi3.FindControl("lblBudgetAllocationNumber"), Label).Text + "|"
                                    strBudgetAllocationDBUpload += strTempBudgetAllocation + ";"
                                    dblTotalAllocation += CDbl(strTempBudgetAllocation)
                                End If
                            Else
                                blnAreBudgetsValid = False
                                uclFeedback.DisplayError("For " + Format(CDate(DirectCast(rpi2.FindControl("lblEntryDate"), Label).Text), "d") + ", '" + Server.HtmlEncode(strTempBudgetAllocation) + Resources.GlobalText.Error_TSParttime_InvalidAllocationText)
                                Exit For
                            End If
                        Next
                        If blnAreBudgetsValid AndAlso dblTotalAllocation <> Format(CDbl(DirectCast(rpi2.FindControl("lblTotalEntryMinutes"), Label).Text) / 60, "#.##") Then
                            blnAreBudgetsValid = False
                            uclFeedback.DisplayError("For " + Format(CDate(DirectCast(rpi2.FindControl("lblEntryDate"), Label).Text), "d") + ", " + Resources.GlobalText.Error_TSParttime_InvalidEntryAllocation)
                        Else
                            strBudgetAllocationDBUploadFinal += strBudgetAllocationDBUpload
                        End If
                    End If
                Next
            Next

            If blnAreBudgetsValid Then clsTimesheet.UploadBudgetDataPerEntry(intTSID, strBudgetAllocationDBUploadFinal)
        Else
            Dim strBudgetNumber As String = DBHelper.ExecuteDataset(CommandType.StoredProcedure, "usp_SELECT_TimesheetTotals_TIME", _
                                                                              New SqlParameter("@TimesheetID", intTSID)).Tables(0).Rows(0).Item("BudgetNumber").ToString

            clsTimesheet.UploadBudgetDataPerTimesheet(intTSID, strBudgetNumber + "|" + Format(CDbl(ViewState("TotalTimesheetMinutes")) / 60, "#.##"))
        End If

        Return blnAreBudgetsValid
    End Function

    Private Sub btnApproveTimesheet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApproveTimesheet.Click
        Dim blnValidTimesheet As Boolean = True

        If intTimesheetStatusID = My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then
            If SubmitBudgetEntries() Then
                clsTimesheet.LogTimesheetAction(intTSID, My.Settings.TimesheetAction_ApproveTimesheet, clsSession.userSID, Server.HtmlEncode(txtComment.Text))
            Else
                blnValidTimesheet = False
            End If
        ElseIf intTimesheetStatusID = My.Settings.TimesheetStatus_SentToPayroll Then
            'if payperiod is in future, don't allow submission: 
            BindDataTable()
            If Now.Date <= CDate(dtTimesheet.Rows(0).Item("BeginDate")) Then
                blnValidTimesheet = False
                uclFeedback.DisplayError(Resources.GlobalText.Error_Timesheet_CannotProcessFuture)
            Else
                clsTimesheet.LogTimesheetAction(intTSID, My.Settings.TimesheetAction_ProcessTimesheet, clsSession.userSID, Server.HtmlEncode(txtComment.Text))
            End If
        End If

        If blnValidTimesheet Then
            clsTimesheet.RouteTimesheet(intTSID)
            Response.Redirect(Request.RawUrl)
        End If
    End Sub

    Private Sub btnRejectTimesheet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRejectTimesheet.Click
        If txtComment.Text.ToString.Trim <> "" Then
            clsNotify.SendTemplateEmail(intTSID, Resources.GlobalText.Email_TimesheetRejected_EmployeeNotification)

            If intTimesheetStatusID > My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then 'If intTimesheetStatusID = My.Settings.TimesheetStatus_SentToPayroll Or intTimesheetStatusID = My.Settings.TimesheetStatus_AwaitingFinAidApproval Then
                clsTimesheet.DeleteBudgetAllocations(intTSID)
                clsNotify.SendTemplateEmail(intTSID, Resources.GlobalText.Email_TimesheetRejected_SupervisorNotification, True)
            End If

            clsTimesheet.RejectTimesheet(intTSID)
            clsTimesheet.LogTimesheetAction(intTSID, My.Settings.TimesheetAction_RejectTimesheet, clsSession.userSID, Server.HtmlEncode(txtComment.Text))
            Response.Redirect(Request.RawUrl)
        Else
            uclFeedback.DisplayError(Resources.GlobalText.Error_TIMESHEET_CommentBlank)
        End If
    End Sub

    Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        Page.SetFocus(txtRemark)
    End Sub

    Private Sub rptBudgetAllocationSelection_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles rptBudgetAllocationSelection.ItemDataBound
        If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim txtBudgetAllocationAmount As TextBox = e.Item.FindControl("txtBudgetAllocationAmount")
            If blnIsSupervisor And intTimesheetStatusID = My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then txtBudgetAllocationAmount.Enabled = True
        End If
    End Sub


End Class