Imports System.Data
Imports System.Data.SqlClient

Partial Public Class ApproveTimesheets
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        LoadPrimaryApproval()
        LoadAlternateApproval()
    End Sub

    Private Sub LoadPrimaryApproval()
        rptPrimaryApproval.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Timesheet_PrimaryApproval", _
                                                                New SqlParameter("@SID", clsSession.userSID))
        rptPrimaryApproval.DataBind()

        If rptPrimaryApproval.Items.Count = 0 Then
            pNoTimesheets_Primary.Visible = True
            rptPrimaryApproval.Visible = False
        Else
            pNoTimesheets_Primary.Visible = False
            rptPrimaryApproval.Visible = True
        End If
    End Sub

    Private Sub LoadAlternateApproval()
        rptAlternateApproval.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Timesheet_AlternateApproval", _
                                                                New SqlParameter("@SID", clsSession.userSID))
        rptAlternateApproval.DataBind()

        If rptAlternateApproval.Items.Count = 0 Then
            pNoTimesheets_Alternate.Visible = True
            rptAlternateApproval.Visible = False
        Else
            pNoTimesheets_Alternate.Visible = False
            rptAlternateApproval.Visible = True
        End If
    End Sub
End Class