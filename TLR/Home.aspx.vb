Imports System.Data
Imports System.Data.SqlClient


Partial Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()

    If Not IsPostBack Then

      DisplayActiveTimesheets()
      PopulateJobList()
      DisplayWorkScheduleMessage()
    End If
    End Sub

    Private Sub DisplayActiveTimesheets()
        rptActiveTimesheets.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Timesheet_ActiveTSList", _
                                                        New SqlParameter("@SID", clsSession.userSID))
        rptActiveTimesheets.DataBind()
        If rptActiveTimesheets.Items.Count = 0 Then
            lblNoActiveTimesheets.Visible = True
            rptActiveTimesheets.Visible = False
        Else
            lblNoActiveTimesheets.Visible = False
            rptActiveTimesheets.Visible = True
        End If
    End Sub


    Private Sub PopulateJobList()

        rptJobList.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_ActiveJobListForEmployee", _
                                                        New SqlParameter("@SID", clsSession.userSID))
        rptJobList.DataBind()

        If rptJobList.Items.Count = 0 Then
            lblNoActiveJobs.Visible = True
            rptJobList.Visible = False
            btnCreateTimeSheet.Visible = False
        Else
            lblNoActiveJobs.Visible = False
            rptJobList.Visible = True
            btnCreateTimeSheet.Visible = True
            'If only one job, select it by default
            If rptJobList.Items.Count = 1 Then
                Dim litJobChecked As Literal = CType(rptJobList.Items(0).FindControl("litJobChecked"), Literal)
                litJobChecked.Text = "checked=""checked"""
            End If
        End If
    End Sub

    Private Sub DisplayWorkScheduleMessage()
        'if user has rights to use the work hours page
        If clsSession.userIsWorkScheduleEligible Then
            pnlWorkSchedule.Visible = True
            'determine if user has work hours entered & approved, left edited or never entered
            If Not clsWorkSchedule.Exists(clsSession.userSID) Then
                pnlWorkScheduleOffer.Visible = True
            ElseIf clsWorkSchedule.GetStatus(clsSession.userSID) = My.Settings.WorkScheduleStatus_Finalized Then
                pnlWorkScheduleCurrent.Visible = True
                rptWorkSchedule.DataSource = clsWorkSchedule.GetEmployeeWorkSchedule(clsSession.userSID)
                rptWorkSchedule.DataBind()
            Else
                pnlWorkScheduleNotApproved.Visible = True
            End If
        End If
    End Sub

    Private Sub PopulateDropDown(ByVal result As IEnumerable, ByVal dropDown As ListControl)

        dropDown.DataSource = result
        dropDown.DataTextField = "DisplayDates"
        dropDown.DataValueField = "EndDate"
        dropDown.DataBind()
    End Sub

    Private Sub rptJobList_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles rptJobList.ItemDataBound
        If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then

            'Dim lblJobNumber As Label = CType(e.Item.FindControl("lblJobNumber"), Label)
            'lblJobNumber.Text = e.Item.DataItem("JobNumber")

      Dim ddlPayPeriod As DropDownList = CType(e.Item.FindControl("ddlPayPeriod"), DropDownList)
      Dim periodEndDate As Date
      Dim todayDate As Date

            ddlPayPeriod.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_PayPeriodListForJob", _
                                                              New SqlParameter("@SID", clsSession.userSID), _
                                                              New SqlParameter("@PayCycleCode", e.Item.DataItem("PayCycleCode")), _
                                                              New SqlParameter("@JobNumber", e.Item.DataItem("JobNumber")))
            ddlPayPeriod.DataBind()

            Dim lstDay As ListItem
            Dim blnPayPeriodSelected As Boolean = False
            For Each lstDay In ddlPayPeriod.Items

        periodEndDate = CDate(Right(lstDay.Text, Len(lstDay.Text) - InStr(lstDay.Text, "-") - 1))
        todayDate = CDate(Today())
        If todayDate <= periodEndDate Then



          ddlPayPeriod.Items.FindByText(lstDay.Text).Selected = True
          blnPayPeriodSelected = True
          Exit For
        End If
      Next

            If Not blnPayPeriodSelected And ddlPayPeriod.Items.Count <> 0 Then
        ddlPayPeriod.Items(ddlPayPeriod.Items.Count - 1).Selected = True

            End If
        End If
    End Sub

    Private Sub btnCreateTimeSheet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCreateTimeSheet.Click
        If Request.Form("rdoJobNumber") = "" Then
            uclFeedback.DisplayError(Resources.GlobalText.Error_HOME_JobNotChosen)
            Exit Sub
    End If



        Dim intSelectedRow As Integer = Request.Form("rdoJobNumber")
        Dim ddlPayPeriod As DropDownList = CType(rptJobList.Items(intSelectedRow).FindControl("ddlPayPeriod"), DropDownList)
        Dim lblJobNumber As Label = CType(rptJobList.Items(intSelectedRow).FindControl("lblJobNumber"), Label)

        'No more pay periods to report on.
        If ddlPayPeriod.SelectedValue = "" Then
            uclFeedback.DisplayError(Resources.GlobalText.Error_HOME_NoMorePayPeriods)
            Exit Sub
        End If

        Dim parTimesheetID As New SqlParameter
        parTimesheetID.ParameterName = "@TimesheetID"
        parTimesheetID.Direction = ParameterDirection.Output
        parTimesheetID.DbType = DbType.Int32


        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_INSERT_Timesheet", _
                                New SqlParameter("@SID", clsSession.userSID), _
                                New SqlParameter("@PayCycleID", ddlPayPeriod.SelectedValue), _
                                New SqlParameter("@JobNumber", lblJobNumber.Text), _
                                parTimesheetID)

        clsTimesheet.LogTimesheetAction(parTimesheetID.Value, My.Settings.TimesheetAction_CreateTimesheet, clsSession.userSID, "")

        Response.Redirect("Timesheet.aspx?TimesheetID=" & parTimesheetID.Value.ToString)
    End Sub

    Private Sub GoToWorkHoursPage(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditWorkSchedule.Click, btnEnterWorkSchedule.Click, btnWorkScheduleContinueEdit.Click
        Response.Redirect("WorkHours.aspx")
    End Sub
End Class