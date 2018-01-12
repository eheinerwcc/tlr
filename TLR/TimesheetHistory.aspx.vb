
Imports System.Data.SqlClient

Partial Public Class TimesheetHistory
	Inherits System.Web.UI.Page
	
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        uclFeedback.ResetDisplay()

        If Not IsPostBack Then
            'Without searching, display 5 most recent timesheets (SP will return only 5 results)
            rptTimesheets.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Timesheet_SEARCH", _
                                                   New SqlParameter("@SID", clsSession.userSID))
            rptTimesheets.DataBind()
            rptTimesheets.Visible = rptTimesheets.Items.Count > 0
            pMessage.InnerText = IIf(rptTimesheets.Items.Count > 0, "Your most recent timesheets", "You have no recent timesheets")
            pMessage.Visible = True
        End If
    End Sub


    Private Sub btnLookup_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnLookup.Click
        Dim strStartDate As String = txtStartDate.Text.Trim
        Dim strEndDate As String = txtEndDate.Text.Trim

        If IsValidEntry(strStartDate, strEndDate) Then
            rptTimesheets.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Timesheet_SEARCH", _
                                                               New SqlParameter("@SID", clsSession.userSID), _
                                                               New SqlParameter("@BeginDate", IIf(strStartDate = "", DBNull.Value, strStartDate)), _
                                                               New SqlParameter("@EndDate", IIf(strEndDate = "", DBNull.Value, strEndDate)), _
                                                               New SqlParameter("@StatusID", My.Settings.TimesheetStatus_ProcessedByPayroll))
            rptTimesheets.DataBind()

            rptTimesheets.Visible = rptTimesheets.Items.Count > 0
            pMessage.InnerText = "No timesheet were found"
            pMessage.Visible = rptTimesheets.Items.Count = 0
        End If
    End Sub

    Private Function IsValidEntry(ByVal strStartDate As String, ByVal strEndDate As String) As Boolean
        Dim blnIsValidEntry As Boolean = True

        If strStartDate = "" And strEndDate = "" Then
            blnIsValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TimesheetHistory_StartAndEndDatesEmpty)
        End If

        If blnIsValidEntry AndAlso strStartDate <> "" AndAlso Not clsGeneric.IsValidSQLDate(strStartDate) Then
            blnIsValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TimesheetHistory_InvalidStartDate)
        End If

        If blnIsValidEntry AndAlso strEndDate <> "" AndAlso Not clsGeneric.IsValidSQLDate(strEndDate) Then
            blnIsValidEntry = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_TimesheetHistory_InvalidEndDate)
        End If

        If blnIsValidEntry Then
            If strEndDate.Length = 0 Then
                strEndDate = "01/01/1000"
            ElseIf strStartDate.Length = 0 Then
                strStartDate = "01/01/1000"
            Else
                If (CDate(strStartDate) > CDate(strEndDate)) Then
                    blnIsValidEntry = False
                    uclFeedback.DisplayError(Resources.GlobalText.Error_TimeEntry_EndBeforeStart)
                End If
            End If
        End If

        Return blnIsValidEntry
    End Function
End Class