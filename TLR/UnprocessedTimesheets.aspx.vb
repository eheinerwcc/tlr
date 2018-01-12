Imports System.Data.SqlClient

Partial Public Class UnprocessedTimesheets
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        uclFeedback.ResetDisplay()

        If Not Me.IsPostBack Then
            clsGeneric.RedirectIfNotPayrollAdmin()

            If Request.QueryString("TimesheetTypeID") IsNot Nothing AndAlso (Request.QueryString("TimesheetTypeID") = My.Settings.TimesheetTypeID_Time Or Request.QueryString("TimesheetTypeID") = My.Settings.TimesheetTypeID_Leave) Then
                If Request.QueryString("TimesheetTypeID") = My.Settings.TimesheetTypeID_Time Then
                    'set header for Time
                    lblPageHeader.Text = "Process Timesheets - TIME"
                ElseIf Request.QueryString("TimesheetTypeID") = My.Settings.TimesheetTypeID_Leave Then
                    'set header for Leave
                    lblPageHeader.Text = "Process Timesheets - LEAVE"
                End If
                BindRepeater()
            Else
                uclFeedback.DisplayError("You need to access this page using the links in this site's menu.")
            End If
        End If
    End Sub


    Private Sub BindRepeater()
        rptTimesheets.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Timesheet_Unprocessed", _
                                                           New SqlParameter("@TimesheetTypeID", Request.QueryString("TimesheetTypeID")))
        rptTimesheets.DataBind()

        If rptTimesheets.Items.Count = 0 Then
            rptTimesheets.Visible = False
            pNoUnprocessedTimesheets.Visible = True
        Else
            rptTimesheets.Visible = True
            pNoUnprocessedTimesheets.Visible = False
        End If
    End Sub
End Class