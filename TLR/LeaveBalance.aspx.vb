Imports System.Data
Imports System.Data.SqlClient

Partial Public Class LeaveBalance
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()

        If Not IsPostBack Then
            LoadData()
        End If
    End Sub

    Private Sub LoadData()
        rptLeaveBalance.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_LeaveBalance", _
                                                New SqlParameter("@SID", clsSession.userSID))
        rptLeaveBalance.DataBind()
    End Sub
End Class