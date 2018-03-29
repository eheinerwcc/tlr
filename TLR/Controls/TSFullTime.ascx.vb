Imports System.Data
Imports System.Data.SqlClient

Partial Public Class TS_FullTime
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
  Private blnIsDelegated As Boolean = False

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    If Me.Visible Then
      uclFeedback.ResetDisplay()


      If Not IsPostBack Then

        BindTSDataTable()
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
                            DisplayWorkHourMessage()
                            pnlEntryDetails.Visible = True
                            BindEditControls()
                            pnlSubmissionInterface.Visible = True
                            btnAddRemark.Visible = True
                            h2Remarks.Visible = True
                            txtOT_Convert.ReadOnly = False
                            txtOT_Pay.ReadOnly = False
                            pOT.Visible = True
                        End If
                    Case My.Settings.TimesheetStatus_AwaitingSupervisorApproval
                        If blnIsSupervisor Then
                            'Supervisor Edit Mode
                            'Setting to false so approvers can't preapprove leave.
                            ViewState("blnClickableGridMode") = False
                            ViewState("blnEditableRemarkMode") = True
                            pnlApprovalInterface.Visible = True
                            btnAddRemark.Visible = True
                            h2Remarks.Visible = True
                        End If
                    Case My.Settings.TimesheetStatus_SentToPayroll
                        If clsSession.userIsPayrollAdmin Then
                            ViewState("blnClickableGridMode") = False 'payroll doesn't need clickable links
                            ViewState("blnEditableRemarkMode") = True
                            pnlApprovalInterface.Visible = True
                            btnAddRemark.Visible = True
                            h2Remarks.Visible = True
                            btnApproveTimesheet.Text = "Mark as Processed"
                            btnApproveTimesheet.OnClientClick = "return confirm('Are you sure you want to mark this timesheet as processed?');"
                        End If
                    Case My.Settings.TimesheetStatus_ProcessedByPayroll
                        'View Mode
                End Select

                BindTimesheetHeader()
        BindTimesheetRemarks()
        BindTimesheetGrid()
        BindTimesheetTotals()
        BindOTControls()
      End If
    End If






    Dim parComptimeLeft As New SqlParameter
    parComptimeLeft.ParameterName = "@comptimeLeft"
    parComptimeLeft.Direction = ParameterDirection.Output
    parComptimeLeft.DbType = DbType.Int16
    parComptimeLeft.Size = 1


    'Check if the employee has vacation on this timesheet when they still have comp time left to use
    SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_TimesheetComptimeLeft", _
                                                                New SqlParameter("@TimesheetID", intTSID), parComptimeLeft)



    Try
      lblCompTimeMsg.Text = ""
    Catch ex As Exception

    End Try

    If parComptimeLeft.Value = 1 Then
      Try
        lblCompTimeMsg.Text = "Compensatory (comp.) time remaining. Please schedule all comp time before scheduling vacation time."
      Catch ex As Exception

      End Try
    Else
      Try
        lblCompTimeMsg.Text = ""
      Catch ex As Exception

      End Try

    End If



    clsTimesheet.RedirectOnOutdatedStatus(intTSID, ViewState("TimesheetStatusID"))
  End Sub

  Private Sub BindOTControls()
    Dim dblTotalOT As Double = clsTimesheet.GetTotalAccruedOT(intTSID)

    If dblTotalOT <> 0 Then
      pnlOvertime.Visible = True
      h2Overtime.Visible = True
      lblOTHours.Text = dblTotalOT.ToString

      If dtTimesheet IsNot Nothing Then
        txtOT_Convert.Text = clsGeneric.DeNull(dtTimesheet.Rows(0).Item("OTComp"))
        txtOT_Pay.Text = clsGeneric.DeNull(dtTimesheet.Rows(0).Item("OTPay"))
      End If
    Else
      pnlOvertime.Visible = False
      h2Overtime.Visible = False
    End If
  End Sub

  Private Sub BindTimesheetHeader()
    Dim datePeriodBegin As Date = dtTimesheet.Rows(0).Item("BeginDate").ToString
    Dim datePeriodEnd As Date = dtTimesheet.Rows(0).Item("EndDate").ToString

    For i As Integer = 0 To DatePart(DateInterval.Day, datePeriodEnd) - DatePart(DateInterval.Day, datePeriodBegin)
      ddlDate.Items.Add(New ListItem(Left(datePeriodBegin.AddDays(i).DayOfWeek.ToString, 3) & " - " & datePeriodBegin.AddDays(i), datePeriodBegin.AddDays(i)))
    Next

    lblTSPeriod.Text = datePeriodBegin & " - " & datePeriodEnd

    liUpdateTimesheet.Visible = (clsSession.userIsPayrollAdmin And intTimesheetStatusID <> My.Settings.TimesheetStatus_ProcessedByPayroll)
    lblStatus.Text = dtTimesheet.Rows(0).Item("StatusName")
    lblName.Text = dtTimesheet.Rows(0).Item("DisplayName") + " (" + dtTimesheet.Rows(0).Item("SID") + ")"
    If dtTimesheet.Rows(0).Item("EmploymentStatus") = "S" Then lblName.Text += " - Separated"
    lblJobTitle.Text = dtTimesheet.Rows(0).Item("JobClassNameLong")
    lblSupervisor.Text = dtTimesheet.Rows(0).Item("JobSupervisorDisplayName") + " (" + clsTimesheet.GetAlternateSignersDisplayString(intTSID) + ")"
    lblDueDate.Text = DateAdd(DateInterval.Day, -2, dtTimesheet.Rows(0).Item("EndDate"))

    If blnIsDelegated Then
      lblDelegationInfo.Text = " on behalf of " + dtTimesheet.Rows(0).Item("DisplayName")
    End If

  End Sub


  Private Sub BindTSDataTable()
    dtTimesheet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Timesheet", _
                                           New SqlParameter("@TimesheetID", intTSID)).Tables(0)
  End Sub

  Private Sub DisplayWorkHourMessage()
    If clsSession.userIsWorkScheduleEligible Then
      pnlNoWorkSchedule.Visible = Not clsWorkSchedule.Exists(clsSession.userSID)
      pnlImportHours.Visible = (clsWorkSchedule.GetStatus(clsSession.userSID) = My.Settings.WorkScheduleStatus_Finalized) And (Not dtTimesheet.Rows(0).Item("WorkHoursImported")) And (Not dtTimesheet.Rows(0).Item("WorkHoursImportDenied"))
      pnlWorkScheduleNotApproved.Visible = (clsWorkSchedule.GetStatus(clsSession.userSID) = My.Settings.WorkScheduleStatus_Draft) And (Not dtTimesheet.Rows(0).Item("WorkHoursImported")) And (Not dtTimesheet.Rows(0).Item("WorkHoursImportDenied"))
    End If
  End Sub

  Private Sub BindEditControls()
    'Bind Leave Type Drop Down List:
    ddlEntryType.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_EntryTypeForTimesheet", _
                                                      New SqlParameter("@TimesheetID", intTSID))
    ddlEntryType.DataBind()
  End Sub

  Private Sub BindTimesheetTotals()
    rptTimesheetTotals.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_TimesheetTotals_LEAVE", _
                                                            New SqlParameter("@TimesheetID", intTSID))
    rptTimesheetTotals.DataBind()
    pnlTimesheetTotals.Visible = (rptTimesheetTotals.Items.Count > 0)
  End Sub

  Private Sub BindTimesheetGrid()
    Dim intTimesheetID As Integer = intTSID

    'Insert the Holiday pay if it is not already inserted
    SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, _
      CommandType.StoredProcedure, "usp_INSERT_TimesheetHolidayPay", _
      New SqlParameter("@TimesheetID", intTimesheetID))

    'Find names of holidays
    rptHolidays.DataSource = SqlHelper.ExecuteDataset(SqlHelper.GetConnString, _
                              CommandType.StoredProcedure, "usp_SELECT_TimesheetHolidayNames", _
                              New SqlParameter("@TimesheetID", intTimesheetID))
    rptHolidays.DataBind()

    If rptHolidays.Items.Count = 0 Then
      Try
        liGenericHoliday.Visible = True
      Catch ex As Exception

      End Try
    Else
      Try
        liGenericHoliday.Visible = False
      Catch ex As Exception

      End Try
    End If


    Dim dsTimesheetGridData As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString, _
                                          CommandType.StoredProcedure, "usp_SELECT_TimesheetGridData_LEAVE", _
                                          New SqlParameter("@TimesheetID", intTimesheetID))


    dsTimesheetGridData.Tables(0).TableName = "Weeks"
    dsTimesheetGridData.Tables(1).TableName = "Days"
    dsTimesheetGridData.Tables(2).TableName = "Entries"

    'Specify relationship for weeks and days
    Dim colParentWkNum As DataColumn = dsTimesheetGridData.Tables("Weeks").Columns("WeekNumber")
    Dim colChildWkNum As DataColumn = dsTimesheetGridData.Tables("Days").Columns("WeekNumber")
    Dim relWeekNumber As New DataRelation("relWeekNumber", colParentWkNum, colChildWkNum)
    dsTimesheetGridData.Relations.Add(relWeekNumber)

    'Specify relationship for days and entires
    Dim colParentDay As DataColumn = dsTimesheetGridData.Tables("Days").Columns("OneDay")
    Dim colChildDay As DataColumn = dsTimesheetGridData.Tables("Entries").Columns("EntryDate")
    Dim relDay As New DataRelation("relDay", colParentDay, colChildDay)
    dsTimesheetGridData.Relations.Add(relDay)

    rptWeeks.DataSource = dsTimesheetGridData.Tables("Weeks")
    rptWeeks.DataBind()
  End Sub

  Private Sub BindTimesheetRemarks()
    rptRemarks.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_TimesheetRemark", _
                                            New SqlParameter("@TimesheetID", intTSID))
    rptRemarks.DataBind()

    h2Remarks.Visible = ((rptRemarks.Items.Count <> 0) Or btnAddRemark.Visible)
    rptRemarks.Visible = (rptRemarks.Items.Count <> 0)
    txtRemark.Text = ""
    pnlEditRemark.Visible = False
  End Sub


  Protected Sub rptWeeks_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles rptWeeks.ItemDataBound
    If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
      Dim rptDays As Repeater = e.Item.FindControl("rptDays")
      If Not IsNothing(rptDays) Then
        rptDays.DataSource = CType(e.Item.DataItem, DataRowView).CreateChildView("relWeekNumber")
        rptDays.DataBind()
      End If
    End If
  End Sub

  Protected Sub rptDays_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs)
    If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
      Dim rptEntries As Repeater = e.Item.FindControl("rptEntries")
      If Not IsNothing(rptEntries) Then
        rptEntries.DataSource = CType(e.Item.DataItem, DataRowView).CreateChildView("relDay")
        rptEntries.DataBind()
      End If
    End If
  End Sub

  Private Sub btnAddEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddEntry.Click
    Dim blnAllowEntry As Boolean = True

    If Not IsValidEntry(txtHours.Text) Then blnAllowEntry = False

    If blnAllowEntry Then
      Dim parErrorCode As New SqlParameter
      parErrorCode.ParameterName = "@ErrorCode"
      parErrorCode.Direction = ParameterDirection.Output
      parErrorCode.DbType = DbType.String
      parErrorCode.Size = 100

      SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_UPSERT_TimesheetEntry_LEAVE", _
                                New SqlParameter("@TimesheetID", intTSID), _
                                New SqlParameter("@EntryDate", ddlDate.SelectedItem.Value), _
                                New SqlParameter("@EntryTypeID", ddlEntryType.SelectedItem.Value), _
                                New SqlParameter("@Duration", txtHours.Text), _
                                New SqlParameter("@ModifiedBy", clsSession.userSID), _
                                parErrorCode)

      If parErrorCode.Value <> "" Then
        uclFeedback.DisplayError(Resources.GlobalText.ResourceManager.GetString(parErrorCode.Value.ToString))
      Else
        If ddlEntryType.SelectedItem.Value = "O" Then BindOTControls()
        BindTimesheetGrid()
        BindTimesheetTotals()
        txtHours.Text = ""

        'Grab the date submitted and select the next date in the dropdown list on page refresh
        Dim dateAdded As DateTime
        Dim dateAddedNext As DateTime
        Dim dateAddedNextString As String

        'grab the date that the person just added to their timesheet
        dateAdded = Convert.ToDateTime(ddlDate.SelectedItem.Value())

        'select the next day after that day
        dateAddedNext = dateAdded.AddDays(1)

        BindTSDataTable()

        'grab the enddate of that time period
        Dim datePeriodEnd As Date = dtTimesheet.Rows(0).Item("EndDate").ToString

        'if the day the person added to their timesheet is the last day of the time period
        'do not increment the dropdown list to the next day.
        If (Not (Convert.ToDateTime(datePeriodEnd) = dateAdded)) Then
          dateAddedNextString = dateAddedNext.ToString("d")
          ddlDate.Items.FindByValue(dateAdded).Selected = False
          ddlDate.Items.FindByValue(dateAddedNextString).Selected = True
        End If
      End If
    End If
  End Sub

  Function IsValidEntry(ByVal strInput As String) As Boolean
    Dim blnReturnValue As Boolean = True

    If Not clsGeneric.IsExpectedNumber(strInput, 2) Then
      uclFeedback.DisplayError(Resources.GlobalText.Error_TSFullTime_InvalidHours_NonNumeric)
      blnReturnValue = False
    Else
      If CDbl(strInput) > 24 Or CDbl(strInput) <= 0 Then
        uclFeedback.DisplayError(Resources.GlobalText.Error_TSFullTime_InvalidHours_Greater24Less0)
        blnReturnValue = False
      End If
    End If

    Return blnReturnValue
  End Function

  Protected Sub EditEntry(ByVal sender As Object, ByVal e As System.EventArgs)
    Dim intTimesheetEntryID As Integer = DirectCast(sender, Button).CommandArgument

    If blnIsSupervisor And intTimesheetStatusID = My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then
      SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_TimesheetEntry_PreApproval", _
                                New SqlParameter("@TimesheetEntryID", intTimesheetEntryID))
      BindTimesheetGrid()
    Else
      Dim dtTimesheetEntry As DataTable = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_TimesheetEntry", _
                                                                   New SqlParameter("@TimesheetEntryID", intTimesheetEntryID)).Tables(0)

      If dtTimesheetEntry.Rows.Count > 0 Then
        PrepareEditableEntry(True)
        With dtTimesheetEntry.Rows(0)
          txtHours.Text = .Item("Duration").ToString
          ddlDate.Items.FindByValue(.Item("EntryDate").Date.ToShortDateString).Selected = True
          ddlEntryType.Items.FindByValue(.Item("EntryTypeID")).Selected = True
        End With

        btnUpdateEntry.CommandArgument = intTimesheetEntryID
        btnDeleteEntry.CommandArgument = intTimesheetEntryID
      End If
    End If
  End Sub

  Private Sub PrepareEditableEntry(ByVal blnEdit As Boolean)
    btnAddEntry.Visible = Not blnEdit
    btnCancelUpdate.Visible = blnEdit
    btnDeleteEntry.Visible = blnEdit
    btnUpdateEntry.Visible = blnEdit

    txtHours.Text = ""
    ddlDate.ClearSelection()
    ddlEntryType.ClearSelection()
    pnlEntryDetails.DefaultButton = IIf(blnEdit, "btnUpdateEntry", "btnAddEntry")
    litEntryHeader.Text = IIf(blnEdit, "Edit Time Entry", "Add Time Entry")
  End Sub

  Private Sub btnCancelUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancelUpdate.Click
    PrepareEditableEntry(False)
  End Sub

  Private Sub btnDeleteEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteEntry.Click
    Dim parErrorCode As New SqlParameter
    parErrorCode.ParameterName = "@ErrorCode"
    parErrorCode.Direction = ParameterDirection.Output
    parErrorCode.DbType = DbType.String
    parErrorCode.Size = 100

    SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_DELETE_TimesheetEntry", _
                              New SqlParameter("@TimesheetEntryID", btnDeleteEntry.CommandArgument), _
                              parErrorCode)

    If parErrorCode.Value <> "" Then
      uclFeedback.DisplayError(Resources.GlobalText.ResourceManager.GetString(parErrorCode.Value.ToString))
    Else
      BindTimesheetGrid()
      BindTimesheetTotals()
      BindOTControls()
      PrepareEditableEntry(False)
    End If
  End Sub

  Private Sub btnUpdateEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpdateEntry.Click
    Dim blnAllowEntry As Boolean = True

    If Not IsValidEntry(txtHours.Text) Then
      uclFeedback.DisplayError(Resources.GlobalText.Error_TSFullTime_InvalidHours)
      blnAllowEntry = False
    End If

    If blnAllowEntry Then
      Dim parErrorCode As New SqlParameter
      parErrorCode.ParameterName = "@ErrorCode"
      parErrorCode.Direction = ParameterDirection.Output
      parErrorCode.DbType = DbType.String
      parErrorCode.Size = 100

      SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_UPSERT_TimesheetEntry_LEAVE", _
                                New SqlParameter("@TimesheetID", intTSID), _
                                New SqlParameter("@TimesheetEntryID", btnUpdateEntry.CommandArgument), _
                                New SqlParameter("@EntryDate", ddlDate.SelectedItem.Value), _
                                New SqlParameter("@EntryTypeID", ddlEntryType.SelectedItem.Value), _
                                New SqlParameter("@Duration", txtHours.Text), _
                                New SqlParameter("@ModifiedBy", clsSession.userSID), _
                                parErrorCode)

      If parErrorCode.Value <> "" Then
        uclFeedback.DisplayError(Resources.GlobalText.ResourceManager.GetString(parErrorCode.Value.ToString))
      Else
        If ddlEntryType.SelectedItem.Value = "O" Then BindOTControls()
        BindTimesheetGrid()
        BindTimesheetTotals()
        PrepareEditableEntry(False)
      End If
    End If
  End Sub

  Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
    Page.SetFocus(txtHours)
    If txtRemark.Visible Then Page.SetFocus(txtRemark)
  End Sub

  Protected Sub DeleteRemark(ByVal sender As Object, ByVal e As System.EventArgs)
    SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_DELETE_TimesheetRemark", _
                              New SqlParameter("@TimesheetRemarkID", DirectCast(sender, Button).CommandArgument))
    BindTimesheetRemarks()
  End Sub

  Protected Sub EditRemark(ByVal sender As Object, ByVal e As System.EventArgs)
    Dim ds As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_TimesheetRemark", _
                                                 New SqlParameter("@TimesheetRemarkID", DirectCast(sender, Button).CommandArgument))

    If ds.Tables(0).Rows.Count = 1 Then
      txtRemark.Text = Server.HtmlDecode(ds.Tables(0).Rows(0).Item("RemarkText").ToString)
      btnSaveRemark.CommandArgument = DirectCast(sender, Button).CommandArgument
      pnlEditRemark.Visible = True
      btnAddRemark.Visible = False
    End If
  End Sub

  Private Sub btnAddRemark_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddRemark.Click
    pnlEditRemark.Visible = True
    btnSaveRemark.CommandArgument = ""
    btnAddRemark.Visible = False
  End Sub

  Private Sub btnCancelRemarkUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancelRemarkUpdate.Click
    txtRemark.Text = ""
    pnlEditRemark.Visible = False
    btnSaveRemark.CommandArgument = ""
    btnAddRemark.Visible = True
  End Sub

  Private Sub btnSaveRemark_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSaveRemark.Click

    If txtRemark.Text.Trim.Length > 0 Then
      If btnSaveRemark.CommandArgument = "" Then
        'new remark
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_INSERT_TimesheetRemark", _
                  New SqlParameter("@TimesheetID", intTSID), _
                  New SqlParameter("@SID", clsSession.userSID), _
                  New SqlParameter("@RemarkText", Server.HtmlEncode(txtRemark.Text)))
      Else
        'update remark
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_TimesheetRemark", _
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

  Private Sub btnDeleteTimesheet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteTimesheet.Click
    clsTimesheet.DeleteTimesheet(intTSID)
    Response.Redirect("Home.aspx")
  End Sub

  Private Sub btnSubmitTimesheet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSubmitTimesheet.Click


    Dim miscContainsRemarks As New Boolean


    'Validate the timesheet for a remark field if there is misc (I) time on timesheet
    Dim parMiscRows As New SqlParameter
    parMiscRows.ParameterName = "@miscRows"
    parMiscRows.Direction = ParameterDirection.Output
    parMiscRows.DbType = DbType.Int16

    SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_TimesheetContainsMisc", _
                              New SqlParameter("@TimesheetID", intTSID), _
                              parMiscRows)

    If (parMiscRows.Value > 0) Then
      miscContainsRemarks = False
    ElseIf (parMiscRows.Value = 0) Then
      miscContainsRemarks = True
    End If






    If (IsTimesheetValid(intTSID) And miscContainsRemarks) Then
      clsNotify.SendTemplateEmail(intTSID, Resources.GlobalText.Email_TimesheetSubmitted_SupervisorNotification, True)
      clsTimesheet.RouteTimesheet(intTSID, blnIsDelegated)
      clsTimesheet.LogTimesheetAction(intTSID, My.Settings.TimesheetAction_SubmitTimesheet, clsSession.userSID, Server.HtmlEncode(txtComment.Text))
      If pnlOvertime.Visible Then clsTimesheet.UploadOT(intTSID, IIf(clsGeneric.IsExpectedNumber(txtOT_Pay.Text, 2), txtOT_Pay.Text, 0.0), IIf(clsGeneric.IsExpectedNumber(txtOT_Convert.Text, 2), txtOT_Convert.Text, 0.0))
      Response.Redirect("Home.aspx")

    ElseIf (miscContainsRemarks <> True) Then
      uclFeedback.DisplayError(Resources.GlobalText.Error_TIMESHEET_NeedRemark)
    Else
    End If
  End Sub

  Private Function IsTimesheetValid(ByVal intTimesheetID As Integer) As Boolean
    Dim blnIsTimesheetValid As Boolean = True

    'test if there are any entries - no need to submit unless there is something to report
    If clsTimesheet.IsTimesheetEmpty(intTimesheetID) Then
      blnIsTimesheetValid = False
      uclFeedback.DisplayError(Resources.GlobalText.Error_TIMESHEET_EmptyTimesheet)
    End If

    If clsTimesheet.RemarkRequired(intTimesheetID) Then
      blnIsTimesheetValid = False
      uclFeedback.DisplayError(Resources.GlobalText.Error_TSFullTime_RemarkRequired)
    End If

    Dim dblOTHours As Double = clsTimesheet.GetTotalAccruedOT(intTimesheetID)

    If blnIsTimesheetValid AndAlso dblOTHours <> 0 Then
      'test that overtime conversion to pay/comp time has been entered appropriately
      Dim dblOT_Pay As Double, dblOT_Convert As Double

      If clsGeneric.IsExpectedNumber(txtOT_Pay.Text, 2) Then
        dblOT_Pay = txtOT_Pay.Text
      Else
        If txtOT_Pay.Text.Trim = "" Then
          dblOT_Pay = 0.0
        Else
          blnIsTimesheetValid = False
          uclFeedback.DisplayError(Resources.GlobalText.Error_TSFullTime_InvalidOTValue)
        End If
      End If

      If blnIsTimesheetValid AndAlso clsGeneric.IsExpectedNumber(txtOT_Convert.Text, 2) Then
        dblOT_Convert = txtOT_Convert.Text
      ElseIf blnIsTimesheetValid Then
        If txtOT_Convert.Text.Trim = "" Then
          dblOT_Convert = 0.0
        Else
          blnIsTimesheetValid = False
          uclFeedback.DisplayError(Resources.GlobalText.Error_TSFullTime_InvalidOTValue)
        End If
      End If

      If blnIsTimesheetValid AndAlso dblOT_Pay + dblOT_Convert <> dblOTHours Then
        uclFeedback.DisplayError(Resources.GlobalText.Error_TSFulltime_InvalidOTSplit)
        blnIsTimesheetValid = False
      End If
    End If

    Return blnIsTimesheetValid
  End Function


  Private Sub btnEnterWorkHours_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEnterWorkHours.Click
    Response.Redirect("WorkHours.aspx")
  End Sub

  Private Sub btnAskMeLater_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAskMeLater.Click
    pnlNoWorkSchedule.Visible = False
  End Sub

  Private Sub btnImportWorkHours_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnImportWorkHours.Click
    SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_INSERT_TimesheetEntry_LEAVE_ImportFromWorkSchedule", _
                              New SqlParameter("@TimesheetID", intTSID))
    BindTSDataTable() 'populates dtTimesheet
    BindTimesheetGrid()
    BindTimesheetTotals()
    DisplayWorkHourMessage()
    BindOTControls()
  End Sub

  Private Sub btnWorkScheduleContinueEdit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnWorkScheduleContinueEdit.Click
    Response.Redirect("WorkHours.aspx")
  End Sub

  Private Sub btnDoNotImportWorkHours_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDoNotImportWorkHours.Click
    SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UDPATE_Timesheet_DenyWorkHoursImport", _
                              New SqlParameter("@TimesheetID", intTSID))
    BindTSDataTable() 'populates dtTimesheet
    DisplayWorkHourMessage()
  End Sub

  Private Sub btnApproveTimesheet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApproveTimesheet.Click
    Dim blnValidTimesheet As Boolean = True

    If intTimesheetStatusID = My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then
      clsTimesheet.LogTimesheetAction(intTSID, My.Settings.TimesheetAction_ApproveTimesheet, clsSession.userSID, Server.HtmlEncode(txtComment.Text))
    ElseIf intTimesheetStatusID = My.Settings.TimesheetStatus_SentToPayroll Then
      BindTSDataTable()
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
      If intTimesheetStatusID > My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then
        clsNotify.SendTemplateEmail(intTSID, Resources.GlobalText.Email_TimesheetRejected_SupervisorNotification, True)
      End If

      clsTimesheet.RejectTimesheet(intTSID)
      clsTimesheet.LogTimesheetAction(intTSID, My.Settings.TimesheetAction_RejectTimesheet, clsSession.userSID, Server.HtmlEncode(txtComment.Text))
      Response.Redirect(Request.RawUrl)
    Else
      uclFeedback.DisplayError(Resources.GlobalText.Error_TIMESHEET_CommentBlank)
    End If
  End Sub

  Protected Function GetSupervisorApprovalStatusID() As Integer
    Return My.Settings.TimesheetStatus_AwaitingSupervisorApproval
  End Function


End Class
