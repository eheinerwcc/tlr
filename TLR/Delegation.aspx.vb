Imports System.Data.SqlClient
Imports System.Web.Services

Partial Public Class Delegation
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        uclFeedback.ResetDisplay()

        If Not Me.IsPostBack Then
            clsGeneric.RedirectIfNotPayrollAdmin()
            clsGeneric.RedirectIfNotHRAdmin()
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
        ViewState("EmployeeSID") = strSID
        pnlUserSearch.Visible = False

        pnlUserList.Visible = False
        pnlEmployeeDetails.Visible = True
        btnSelectNewEmployee.Visible = True

        'employee name
        Dim dsEmployeeDetails As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Employee_Details", _
                                                       New SqlParameter("@SID", strSID))
        lblEmployeeName.Text = dsEmployeeDetails.Tables(0).Rows(0).Item("DisplayName").ToString
        If dsEmployeeDetails.Tables(0).Rows(0).Item("EmploymentStatus") = "S" Then lblEmployeeName.Text += " (Separated)"

        'signature delegation data
        ddlSupervisors.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Supervisor", _
                                                            New SqlParameter("@SID", strSID))
        ddlSupervisors.DataBind()

        BindExistingDelegations()
    End Sub

    Private Sub BindExistingDelegations()
        rptExistingDelegations.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Delegation", _
                                                                    New SqlParameter("@SID", ViewState("EmployeeSID")))
        rptExistingDelegations.DataBind()

        rptExistingDelegations.Visible = rptExistingDelegations.Items.Count > 0
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

    Private Sub btnSelectNewEmployee_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSelectNewEmployee.Click
        btnSelectNewEmployee.Visible = False
        Response.Redirect(Request.RawUrl)
    End Sub

    Protected Sub DeleteDelegation(ByVal sender As Object, ByVal e As System.EventArgs)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_DELETE_Delegation", _
                                  New SqlParameter("@DelegationID", DirectCast(sender, Button).CommandArgument))
        BindExistingDelegations()
    End Sub

    Protected Sub btnAddDelegation_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddDelegation.Click
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_INSERT_Delegation", _
                                  New SqlParameter("@SID", ViewState("EmployeeSID")), _
                                  New SqlParameter("@SuperID", ddlSupervisors.SelectedValue), _
                                  New SqlParameter("@AddedBy", clsSession.userSID))
        BindExistingDelegations()
    End Sub
End Class