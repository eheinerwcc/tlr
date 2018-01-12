Imports System.Data
Imports System.Data.SqlClient

Partial Public Class LogIn
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub btnLogin_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnLogin.Click
        'Error-handle SID/PIN input
        Dim strSID As String

        Dim parValidLogin As New SqlParameter
        parValidLogin.ParameterName = "@Result"
        parValidLogin.Direction = Data.ParameterDirection.Output
        parValidLogin.DbType = Data.DbType.Boolean

        If Len(txtSID.Text) = 0 Then
            uclFeedback.DisplayError(Resources.GlobalText.Error_LOGIN_SIDblank)
            Exit Sub
        End If
        If Len(txtSID.Text) <> 9 Then
            uclFeedback.DisplayError(Resources.GlobalText.Error_LOGIN_SIDNot9Char)
            Exit Sub
        End If
        If Len(txtPIN.Text) = 0 Then
            uclFeedback.DisplayError(Resources.GlobalText.Error_LOGIN_PINBlank)
            Exit Sub
        End If
        If Not clsGeneric.IsExpectedPIN(txtPIN.Text) Then  'IsNumeric(txtPIN.Text) Then
            uclFeedback.DisplayError(Resources.GlobalText.Error_LOGIN_PINNotNumeric)
            Exit Sub
        End If
        'user input validated
        strSID = txtSID.Text.ToUpper

        'Vaidate SID/PIN against db
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, Data.CommandType.StoredProcedure, "usp_ValidateEmployeePIN", _
        New SqlParameter("@SID", strSID), _
        New SqlParameter("@PIN", txtPIN.Text), _
        parValidLogin)

        If parValidLogin.Value Then
            clsSession.userSID = strSID
            Dim dtUserProfile As DataTable
            dtUserProfile = SqlHelper.ExecuteDataset(SqlHelper.GetConnString, Data.CommandType.StoredProcedure, "usp_SELECT_UserProfile", _
            New SqlParameter("@SID", strSID)).Tables(0)

            If dtUserProfile.Rows.Count > 0 Then
                If dtUserProfile.Rows(0).Item("EmploymentStatus") = "S" Then
                    uclFeedback.DisplayError(Resources.GlobalText.Error_LOGIN_InactiveEmployee)
                Else
                    'set all session variables
                    clsSession.userDisplayName = dtUserProfile.Rows(0).Item("DisplayName")
                    clsSession.userFirstName = dtUserProfile.Rows(0).Item("FirstName")
                    clsSession.userFirstName = dtUserProfile.Rows(0).Item("LastName")
                    clsSession.userEmployeeType = dtUserProfile.Rows(0).Item("EmployeeTypeID")
                    clsSession.userEmploymentStatus = dtUserProfile.Rows(0).Item("EmploymentStatus")
                    clsSession.userFullPartIndicator = dtUserProfile.Rows(0).Item("FullPartInd")
                    clsSession.userLeaveExpirationMonth = dtUserProfile.Rows(0).Item("LeaveExpirMonth")
                    clsSession.userLeaveEligible = IIf(dtUserProfile.Rows(0).Item("JobLeaveIndicator") = "Y", 1, 0)
                    clsSession.userWorkEmail = dtUserProfile.Rows(0).Item("WorkEmail")
                    clsSession.userIsSupervisor = dtUserProfile.Rows(0).Item("IsSupervisor")
                    clsSession.userIsDelegatedTimesheetManager = dtUserProfile.Rows(0).Item("IsDelegatedTimesheetManager")
                    clsSession.userIsPayrollAdmin = My.Settings.Role_PayrollAdmin.Contains(strSID) 'dtUserProfile.Rows(0).Item("IsPayrollAdmin")
                    clsSession.userIsFinAidAdmin = My.Settings.Role_FinAidAdmin.Contains(strSID)
                    clsSession.userIsFinanceAdmin = My.Settings.Role_FinanceAdmin.Contains(strSID)
                    clsSession.userIsHRAdmin = My.Settings.Role_HRAdmin.Contains(strSID)

                    Dim strWorkScheduleEligibleEmployeeTypes() As String = My.Settings.WorkHoursEligibleEmployeeTypes.Split(",")

                    clsSession.userIsWorkScheduleEligible = False
                    For Each strEmployeeType As String In strWorkScheduleEligibleEmployeeTypes
                        If dtUserProfile.Rows(0).Item("EmployeeTypeID") = strEmployeeType Then
                            clsSession.userIsWorkScheduleEligible = True
                        End If
                    Next

                    If clsGeneric.IsValidIntegerQuerystring(Request.QueryString("TimesheetID")) Then
                        Response.Redirect("Timesheet.aspx?TimesheetID=" + Request.QueryString("TimesheetID").ToString)
                    Else
                        Response.Redirect("home.aspx")
                    End If
                End If
            End If
        Else
            uclFeedback.DisplayError(Resources.GlobalText.Error_LOGIN_InvalidLogin)
        End If
    End Sub

    Private Sub LogIn_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        Page.SetFocus(txtSID)
    End Sub
End Class