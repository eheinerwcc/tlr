Imports System.Data.SqlClient

Partial Public Class WorkHours
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        uclFeedback.ResetDisplay()

        If Not Me.IsPostBack Then
            clsGeneric.RedirectIfNotWorkScheduleEligible()
            BindWorkSchedule()

            UpdateStatus()
            LoadEntryData()
        End If
    End Sub

    Private Sub BindWorkSchedule()
        ViewState("TotalWorkScheduleMinutes") = 0
        rptWorkSchedule.DataSource = clsWorkSchedule.GetEmployeeWorkSchedule(clsSession.userSID)
        rptWorkSchedule.DataBind()

        If rptWorkSchedule.Items.Count > 0 Then
            lblTotalWorkHours.Text = "Total: " + Math.Floor(ViewState("TotalWorkScheduleMinutes") / 60).ToString + " <abbr title='Hours'>hrs.</abbr> " + IIf(ViewState("TotalWorkScheduleMinutes") Mod 60 = 0, "", CInt(ViewState("TotalWorkScheduleMinutes") Mod 60).ToString + " <abbr title='Minutes'>mins.</abbr>")
        End If

        rptWorkSchedule.Visible = (rptWorkSchedule.Items.Count <> 0)
        btnSubmitWorkSchedule.Visible = (rptWorkSchedule.Items.Count <> 0)
        lblTotalWorkHours.Visible = (rptWorkSchedule.Items.Count <> 0)
    End Sub

    Private Sub UpdateStatus()
        Dim dtWorkSchedule As DataTable = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkSchedule", _
                                                     New SqlParameter("@SID", clsSession.userSID)).Tables(0) '.Rows(0).Item("StatusName")
        If dtWorkSchedule.Rows.Count = 0 Then
            SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_INSERT_WorkSchedule", _
                                      New SqlParameter("@SID", clsSession.userSID))
            dtWorkSchedule = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkSchedule", _
                                                     New SqlParameter("@SID", clsSession.userSID)).Tables(0)
        End If

        btnAddEntry.CommandArgument = dtWorkSchedule.Rows(0).Item("WorkScheduleID")
        lblWorkScheduleStatus.Text = dtWorkSchedule.Rows(0).Item("StatusName")
        btnSubmitWorkSchedule.Visible = (dtWorkSchedule.Rows(0).Item("WorkScheduleStatusID") = My.Settings.WorkScheduleStatus_Draft) And rptWorkSchedule.Items.Count > 0
    End Sub

    Private Sub LoadEntryData()
        For i As Integer = 1 To 12
            ddlStartHour.Items.Add(New ListItem(IIf(i.ToString.Length = 1, "0" + i.ToString(), i), i))
            ddlEndHour.Items.Add(New ListItem(IIf(i.ToString.Length = 1, "0" + i.ToString(), i), i))
        Next
        ddlStartHour.SelectedValue = 8
        ddlEndHour.SelectedValue = 5

        For i As Integer = 0 To 55 Step 5
            ddlStartMinute.Items.Add(New ListItem(IIf(i.ToString.Length = 1, "0" + i.ToString(), i)))
            ddlEndMinute.Items.Add(New ListItem(IIf(i.ToString.Length = 1, "0" + i.ToString(), i)))
        Next

        ddlEndAMPM.Items(1).Selected = True

        For i As Integer = 0 To 120 Step 5
            ddlMealTime.Items.Add(New ListItem(i.ToString() + " minutes", i))
        Next
        ddlMealTime.SelectedValue = "60"
    End Sub

    Private Sub btnAddEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddEntry.Click
        Dim dateDefault As DateTime = "1/1/1900"
        Dim strEndTime As String = ddlEndHour.SelectedItem.Value + ":" + ddlEndMinute.SelectedItem.Value + " " + ddlEndAMPM.SelectedItem.Value
        Dim endTime As Date
        Dim startTime As Date = dateDefault + " " + ddlStartHour.SelectedItem.Value + ":" + ddlStartMinute.SelectedItem.Value + " " + ddlStartAMPM.SelectedItem.Value
        Dim intMealTime As Integer = ddlMealTime.SelectedItem.Value

        If strEndTime = "12:00 AM" Then
            endTime = dateDefault.AddDays(1) + " " + strEndTime
        Else
            endTime = dateDefault + " " + strEndTime
        End If


        If IsValidEntry(startTime, endTime, intMealTime) Then
            SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_INSERT_WorkScheduleEntry", _
                                      New SqlParameter("@SID", clsSession.userSID), _
                                      New SqlParameter("@WorkScheduleID", btnAddEntry.CommandArgument), _
                                      New SqlParameter("@DayOfWeek", ddlDay.SelectedItem.Value), _
                                      New SqlParameter("@StartTime", startTime), _
                                      New SqlParameter("@EndTime", endTime), _
                                      New SqlParameter("@MealTime", intMealTime))

            SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_WorkScheduleStatus", _
                                      New SqlParameter("@SID", clsSession.userSID), _
                                      New SqlParameter("@WorkScheduleStatusID", My.Settings.WorkScheduleStatus_Draft))

            BindWorkSchedule()
            ddlDay.SelectedValue = IIf(ddlDay.SelectedValue + 1 = 8, 1, ddlDay.SelectedValue + 1)
        End If
    End Sub

    Private Function IsValidEntry(ByVal startTime As Date, ByVal endTime As Date, ByVal intMealTime As Integer, Optional ByVal intWorkScheduleEntryID As Integer = -1) As Boolean
        Dim blnValidEntry As Boolean = True

        If startTime > endTime Then
            blnValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TimeEntry_EndBeforeStart)
        ElseIf startTime = endTime Then
            blnValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TimeEntry_StartEqualsEnd)
        End If

        If blnValidEntry And ddlMealTime.SelectedItem.Value <> 0 Then
            If endTime.Subtract(startTime).TotalMinutes <= intMealTime Then
                blnValidEntry = False
                uclFeedback.DisplayError(Resources.GlobalText.Error_TimeEntry_MealExceedsShift)
            End If
        End If

        Dim blnOverlappingEntry As Integer = (SqlHelper.ExecuteScalar(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_IS_OverlappingWorkScheduleEntry", _
                                   New SqlParameter("@WorkScheduleID", btnAddEntry.CommandArgument), _
                                   New SqlParameter("@WorkScheduleEntryID", IIf(intWorkScheduleEntryID = -1, DBNull.Value, intWorkScheduleEntryID)), _
                                   New SqlParameter("@DayOfWeek", ddlDay.SelectedItem.Value), _
                                   New SqlParameter("@StartTime", startTime), _
                                   New SqlParameter("@EndTime", endTime)) = 1)

        If blnValidEntry AndAlso blnOverlappingEntry Then
            blnValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_WorkSchedule_OverlappingEntry)
        End If

        Dim intExistingEntryMinutes As Integer = 0

        If intWorkScheduleEntryID <> -1 Then
            Dim dsWorkScheduleEntry As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkScheduleEntry", _
                                                                   New SqlParameter("@SID", DBNull.Value), _
                                                                   New SqlParameter("@WorkScheduleEntryID", intWorkScheduleEntryID)) '.Tables(0).Rows(0).Item("TotalMinutes")

            If dsWorkScheduleEntry.Tables(0).Rows.Count > 0 Then
                intExistingEntryMinutes = dsWorkScheduleEntry.Tables(0).Rows(0).Item("TotalMinutes")
            End If
        End If

        If blnValidEntry AndAlso ((ViewState("TotalWorkScheduleMinutes") + DateDiff(DateInterval.Minute, startTime, endTime) - intMealTime) - intExistingEntryMinutes > 2400) Then
            blnValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_WorkSchedule_40WHoursExceeded)
        End If

        Return blnValidEntry
    End Function

    Protected Sub DeleteEntry(ByVal sender As Object, ByVal e As System.EventArgs)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_DELETE_WorkScheduleEntry", _
                                  New SqlParameter("@WorkScheduleEntryID", DirectCast(sender, Button).CommandArgument))

        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_WorkScheduleStatus", _
                                  New SqlParameter("@SID", clsSession.userSID), _
                                  New SqlParameter("@WorkScheduleStatusID", My.Settings.WorkScheduleStatus_Draft))

        BindWorkSchedule()
        UpdateStatus()
    End Sub

    Protected Sub EditEntry(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim intWorkScheduleEntryID As Integer = DirectCast(sender, Button).CommandArgument
        Dim dtTimesheetEntry As DataTable = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkScheduleEntry", _
                                                        New SqlParameter("@WorkScheduleEntryID", intWorkScheduleEntryID)).Tables(0)

        If dtTimesheetEntry.Rows.Count > 0 Then
            PrepareEditableEntry(True)

            With dtTimesheetEntry.Rows(0)
                Dim dateStartTime As Date = DirectCast(.Item("StartTime"), Date)
                Dim dateEndTime As Date = DirectCast(.Item("EndTime"), Date)

                Dim intStartHour As Integer = IIf(dateStartTime.Hour = 0, 12, dateStartTime.Hour)
                Dim intEndHour As Integer = IIf(dateEndTime.Hour = 0, 12, dateEndTime.Hour)

                ddlDay.SelectedValue = .Item("DayofWeek")
                ddlMealTime.SelectedValue = .Item("MealTime")

                ddlStartHour.SelectedValue = IIf(intStartHour <= 12, intStartHour, intStartHour - 12)
                ddlStartMinute.SelectedValue = IIf(dateStartTime.Minute.ToString.Length = 1, "0" + dateStartTime.Minute.ToString, dateStartTime.Minute.ToString)
                ddlStartAMPM.SelectedValue = IIf(CDate(.Item("StartTime")).TimeOfDay.Hours < 12, "AM", "PM") 'IIf(intStartHour < 12, "AM", "PM")

                ddlEndHour.SelectedValue = IIf(intEndHour <= 12, intEndHour, intEndHour - 12)
                ddlEndMinute.SelectedValue = IIf(dateEndTime.Minute.ToString.Length = 1, "0" + dateEndTime.Minute.ToString, dateEndTime.Minute.ToString)
                ddlEndAMPM.SelectedValue = IIf(CDate(.Item("EndTime")).TimeOfDay.Hours < 12, "AM", "PM") 'IIf(intEndHour < 12, "AM", "PM")

                btnUpdateEntry.CommandArgument = intWorkScheduleEntryID
                btnDeleteEntry.CommandArgument = intWorkScheduleEntryID
            End With
        End If
    End Sub

    Private Sub PrepareEditableEntry(ByVal blnEdit As Boolean)
        btnAddEntry.Visible = Not blnEdit
        btnCancelUpdate.Visible = blnEdit
        btnDeleteEntry.Visible = blnEdit
        btnUpdateEntry.Visible = blnEdit

        btnUpdateEntry.CommandArgument = ""
        btnDeleteEntry.CommandArgument = ""

        ddlDay.ClearSelection()
        ddlStartHour.ClearSelection()
        ddlStartMinute.ClearSelection()
        ddlStartAMPM.ClearSelection()
        ddlEndHour.ClearSelection()
        ddlEndMinute.ClearSelection()
        ddlEndAMPM.ClearSelection()
        ddlMealTime.ClearSelection()

        ddlStartHour.SelectedValue = 8
        ddlEndHour.SelectedValue = 5
        ddlMealTime.SelectedValue = 60
        ddlStartAMPM.Items(0).Selected = True
        ddlEndAMPM.Items(1).Selected = True

        lblWorkScheduleEntryTitle.Text = IIf(blnEdit, "Edit Time Entry", "Add Time Entry")
    End Sub

    Private Sub btnUpdateEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpdateEntry.Click
        Dim dateDefault As DateTime = "1/1/1900"
        Dim strEndTime As String = ddlEndHour.SelectedItem.Value + ":" + ddlEndMinute.SelectedItem.Value + " " + ddlEndAMPM.SelectedItem.Value
        Dim endTime As Date
        Dim startTime As Date = dateDefault + " " + ddlStartHour.SelectedItem.Value + ":" + ddlStartMinute.SelectedItem.Value + " " + ddlStartAMPM.SelectedItem.Value
        Dim intMealTime As Integer = ddlMealTime.SelectedItem.Value

        If strEndTime = "12:00 AM" Then
            endTime = dateDefault.AddDays(1) + " " + strEndTime
        Else
            endTime = dateDefault + " " + strEndTime
        End If

        If IsValidEntry(startTime, endTime, intMealTime, btnUpdateEntry.CommandArgument) Then
            SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_WorkScheduleEntry", _
                                      New SqlParameter("@WorkScheduleEntryID", btnUpdateEntry.CommandArgument), _
                                      New SqlParameter("@DayOfWeek", ddlDay.SelectedItem.Value), _
                                      New SqlParameter("@StartTime", startTime), _
                                      New SqlParameter("@EndTime", endTime), _
                                      New SqlParameter("@MealTime", intMealTime), _
                                      New SqlParameter("@ModifiedBy", clsSession.userSID))

            SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_WorkScheduleStatus", _
                                      New SqlParameter("@SID", clsSession.userSID), _
                                      New SqlParameter("@WorkScheduleStatusID", My.Settings.WorkScheduleStatus_Draft))

            PrepareEditableEntry(False)
            BindWorkSchedule()
            UpdateStatus()
        End If
    End Sub

    Private Sub btnCancelUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancelUpdate.Click
        PrepareEditableEntry(False)
    End Sub

    Private Sub btnDeleteEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeleteEntry.Click
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_DELETE_WorkScheduleEntry", _
                          New SqlParameter("@WorkScheduleEntryID", DirectCast(sender, Button).CommandArgument))

        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_WorkScheduleStatus", _
                                  New SqlParameter("@SID", clsSession.userSID), _
                                  New SqlParameter("@WorkScheduleStatusID", My.Settings.WorkScheduleStatus_Draft))

        BindWorkSchedule()
        UpdateStatus()
        PrepareEditableEntry(False)
    End Sub

    Private Sub rptWorkSchedule_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles rptWorkSchedule.ItemDataBound
        If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
            ViewState("TotalWorkScheduleMinutes") += e.Item.DataItem("TotalMinutes")
        End If
    End Sub

    Private Sub btnSubmitWorkSchedule_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSubmitWorkSchedule.Click
        'submit to DB
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_WorkScheduleStatus", _
                                  New SqlParameter("@SID", clsSession.userSID), _
                                  New SqlParameter("@WorkScheduleStatusID", My.Settings.WorkScheduleStatus_Finalized))

        'display thanks/success
        uclFeedback.DisplaySuccess(Resources.GlobalText.Success_WORKHOURS_Approved)

        'send email to supervisor:
        If ConfigurationManager.AppSettings.Get("Email_UponWorkScheduleSubmissionToSupervisor") Then clsNotify.SendWorkScheduleUpdateNotice()

        UpdateStatus()
    End Sub
End Class