Imports System.Data.Sql
Imports System.Data.SqlClient

Partial Public Class TSActionLog
    Inherits System.Web.UI.UserControl

    Private intTimesheetID As Integer = 0

    Public WriteOnly Property TimesheetID() As Integer
        Set(ByVal value As Integer)
            intTimesheetID = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            LoadBoundControls()
        End If
    End Sub

    Private Sub LoadBoundControls()
        rptTimesheetActionLog.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_TimesheetAction", _
                                                                   New SqlParameter("@TimesheetID", intTimesheetID))
        rptTimesheetActionLog.DataBind()
    End Sub
End Class