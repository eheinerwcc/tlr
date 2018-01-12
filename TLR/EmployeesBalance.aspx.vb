Imports System.Data
Imports System.Data.SqlClient


Partial Public Class EmployeesBalance
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        clsGeneric.RedirectIfNotSupervisor()

        rptLeaveBalance.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_LeaveBalance_SupervisorEmployees", _
                                                             New SqlParameter("@SID", clsSession.userSID))
        rptLeaveBalance.DataBind()
    End Sub

End Class