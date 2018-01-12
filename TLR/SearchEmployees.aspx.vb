Imports System.Data.SqlClient
Imports System.Web.Services

Partial Public Class SearchEmployees
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        uclFeedback.ResetDisplay()
        hPageHeader.InnerText = "Search Employees"

        If Not Me.IsPostBack Then
            If clsSession.userIsPayrollAdmin Or clsSession.userIsHRAdmin Then
                pnlEmployeeLookup.Visible = True
            ElseIf clsSession.userIsSupervisor Then
                BindEmployeeListControls()
            Else
                Response.Redirect("Home.aspx")
            End If
        End If
    End Sub

    Private Sub BindEmployeeLookupControls()
        pnlEmployeeLookup.Visible = True
    End Sub

    Private Sub BindEmployeeListControls()
        rptUserSearchResults.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_EmployeeList_FT", _
                                                          New SqlParameter("@SID", clsSession.userSID))
        rptUserSearchResults.DataBind()

        If rptUserSearchResults.Items.Count = 0 Then
            rptUserSearchResults.Visible = False
            pNoFulltimeEmployees.Visible = True
        Else
            pnlUserList.Visible = True
            rptUserSearchResults.Visible = True
        End If
    End Sub

    <WebMethod()> _
    <Script.Services.ScriptMethod()> _
    Public Shared Function SearchUsers(ByVal prefixText As String, ByVal count As Integer) As String()
        clsGeneric.RedirectOnSessionTimeout()
        Return clsGeneric.SearchUsers_StringArray(prefixText, count, False)
    End Function

    Private Sub btnSearchUsers_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSearchUsers.Click
        SearchUsers(txtUserSearch.Text)
    End Sub

    Private Sub DisplayUserDetails(ByVal strSID As String)
        pnlUserList.Visible = False
        pnlEmployeeLookup.Visible = False
        pnlEmployeeDetails.Visible = True
        btnSelectNewEmployee.Visible = True
        hPageHeader.InnerText = "Employee Details"

        'employee name
        Dim dsEmployeeDetails As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Employee_Details", _
                                                       New SqlParameter("@SID", strSID))
        lblEmployeeName.Text = dsEmployeeDetails.Tables(0).Rows(0).Item("DisplayName").ToString
        If dsEmployeeDetails.Tables(0).Rows(0).Item("EmploymentStatus") = "S" Then lblEmployeeName.Text += " (Separated)"

        'work schedule
        rptWorkSchedule.DataSource = clsWorkSchedule.GetEmployeeWorkSchedule(strSID)
        rptWorkSchedule.DataBind()

        pnlWorkSchedule.Visible = (rptWorkSchedule.Items.Count > 0)
        If rptWorkSchedule.Items.Count > 0 Then
            lblWorkScheduleStatus.Text = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkSchedule", _
                                                     New SqlParameter("@SID", strSID)).Tables(0).Rows(0).Item("StatusName").ToString
        End If

        'balances
        rptLeaveBalance.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_LeaveBalance", _
                                                New SqlParameter("@SID", strSID))
        rptLeaveBalance.DataBind()

        pnlLeaveBalances.Visible = (rptLeaveBalance.Items.Count > 0)

        'timesheets
        rptTimesheets.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Timesheet_SEARCH", _
                                                           New SqlParameter("@SID", strSID), _
                                                           New SqlParameter("@BeginDate", IIf(ddlViewPeriod.SelectedValue.ToString = "-1", "1/1/1900", Now.AddMonths(ddlViewPeriod.SelectedValue * -1))), _
                                                           New SqlParameter("@EndDate", DBNull.Value), _
                                                           New SqlParameter("@StatusID", DBNull.Value))
        rptTimesheets.DataBind()
        rptTimesheets.Visible = rptTimesheets.Items.Count > 0

        btnViewPeriod.CommandArgument = strSID
    End Sub

    Protected Sub SelectUser(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayUserDetails(DirectCast(sender, Button).CommandArgument.ToString)
    End Sub

    Private Sub SearchUsers(ByVal strSearchText As String)
        pnlUserList.Visible = False
        rptUserSearchResults.Visible = False
        pnlEmployeeDetails.Visible = False
        pNoSearchResults.Visible = False

        Dim dsUserSearchResults As DataSet = clsGeneric.SearchUsers_Dataset(strSearchText, False)

        If dsUserSearchResults.Tables(0).Rows.Count = 0 Then
            'show no user result msg
            pNoSearchResults.Visible = True
        ElseIf dsUserSearchResults.Tables(0).Rows.Count = 1 Then
            'call DisplayUserDetails passing SID
            pnlUserList.Visible = True
            rptUserSearchResults.Visible = False
            DisplayUserDetails(dsUserSearchResults.Tables(0).Rows(0).Item("SID"))
        Else
            'show list of results
            pnlUserList.Visible = True
            rptUserSearchResults.Visible = True
            rptUserSearchResults.DataSource = dsUserSearchResults.Tables(0)
            rptUserSearchResults.DataBind()
        End If
    End Sub

    Private Sub btnViewPeriod_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnViewPeriod.Click
        DisplayUserDetails(DirectCast(sender, Button).CommandArgument)
    End Sub

    Private Sub btnSelectNewEmployee_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSelectNewEmployee.Click
        btnSelectNewEmployee.Visible = False
        Response.Redirect(Request.RawUrl)
    End Sub
End Class