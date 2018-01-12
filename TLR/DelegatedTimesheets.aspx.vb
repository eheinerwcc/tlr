Imports System.Data.SqlClient

Partial Public Class DelegatedTimesheets
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()

        If Not Me.IsPostBack Then
            clsGeneric.RedirectIfNotDelegatedTimesheetManager()
            BindControls()
        End If
    End Sub

    Private Sub BindControls()
        rptJobList.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_DelegationForSupervisor", _
                                                        New SqlParameter("@SID", clsSession.userSID))
        rptJobList.DataBind()

        rptActiveTimesheets.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Timesheet_ManagedBySupervisor", _
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

    Private Sub rptJobList_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles rptJobList.ItemDataBound
        If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then

            'Dim lblJobNumber As Label = CType(e.Item.FindControl("lblJobNumber"), Label)
            'lblJobNumber.Text = e.Item.DataItem("JobNumber")

            'Dim lblEmployeeSID As Label = CType(e.Item.FindControl("lblEmployeeSID"), Label)
            'lblJobNumber.Text = e.Item.DataItem("SID")


            Dim ddlPayPeriod As DropDownList = CType(e.Item.FindControl("ddlPayPeriod"), DropDownList)

            ddlPayPeriod.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_PayPeriodListForJob", _
                                                              New SqlParameter("@SID", e.Item.DataItem("SID")), _
                                                              New SqlParameter("@PayCycleCode", e.Item.DataItem("PayCycleCode")), _
                                                              New SqlParameter("@JobNumber", e.Item.DataItem("JobNumber")))
            ddlPayPeriod.DataBind()

            Dim lstDay As ListItem
            Dim blnPayPeriodSelected As Boolean = False
            For Each lstDay In ddlPayPeriod.Items
                If Now() <= CDate(Right(lstDay.Text, Len(lstDay.Text) - InStr(lstDay.Text, "-") - 1)) Then
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
        Dim strEmployeeSID As String = DirectCast(rptJobList.Items(intSelectedRow).FindControl("lblEmployeeSID"), Label).Text

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
                                New SqlParameter("@CreatedBy", clsSession.userSID), _
                                New SqlParameter("@SID", strEmployeeSID), _
                                New SqlParameter("@PayCycleID", ddlPayPeriod.SelectedValue), _
                                New SqlParameter("@JobNumber", lblJobNumber.Text), _
                                parTimesheetID)

        clsTimesheet.LogTimesheetAction(parTimesheetID.Value, My.Settings.TimesheetAction_CreateTimesheet, clsSession.userSID, "")

        Response.Redirect("Timesheet.aspx?TimesheetID=" & parTimesheetID.Value.ToString)
    End Sub
End Class