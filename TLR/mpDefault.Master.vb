Public Partial Class mpDefault
    Inherits System.Web.UI.MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'set the text that shows dev/test/prod
        If (ConfigurationManager.AppSettings.Get("Release_Mode") = "prod") Then
            litAppVersion.Visible = False
        ElseIf (ConfigurationManager.AppSettings.Get("Release_Mode") = "dev") Then
            litAppVersion.Visible = True
      litAppVersion.Text = "<div id=""devAppVersion"">DEV</div>"
        ElseIf (ConfigurationManager.AppSettings.Get("Release_Mode") = "test") Then
            litAppVersion.Visible = True
      litAppVersion.Text = "<div id=""testAppVersion"">TEST</div>"
    End If

    litSupervisorReportLink.Text = "<a href=""" + My.Settings.ReportSupervisorURL + """>Supervisor Reports</a>"
    litSupervisorReportLinkPayroll.Text = "<a href=""" + My.Settings.ReportSupervisorURL + """>Supervisor Reports</a>"
    litPayrollReportLink.Text = "<a href=""" + My.Settings.ReportPayrollURL + """>Payroll Reports</a>"

        'redirect to the session timeout page if the userSID is not valid
        If clsSession.userSID Is Nothing Then Response.Redirect("Default.aspx")


        If Not IsPostBack Then
            lblUserFullName.Text = clsSession.userDisplayName
            If clsSession.userIsSupervisor Then
                liSupervisor.Visible = True
            Else
                liSupervisor.Visible = False
            End If

            If clsSession.userIsPayrollAdmin Then
                liPayroll.Visible = True
            Else
                liPayroll.Visible = False
            End If
            If clsSession.userIsFinAidAdmin Then
                liFinancialAid.Visible = True
            Else
                liFinancialAid.Visible = False
            End If
            If clsSession.userIsFinanceAdmin Then
                liFinance.Visible = True
            Else
                liFinance.Visible = False
            End If
            If clsSession.userIsHRAdmin Then
                liHR.Visible = True
            Else
                liHR.Visible = False
            End If
            If clsSession.userIsWorkScheduleEligible Then
                liWorkHours.Visible = True
            Else
                liWorkHours.Visible = False
            End If
            If clsSession.userEmployeeType = "H" Or clsSession.userEmployeeType = "S" Or clsSession.userEmployeeType = "E" Or clsSession.userEmployeeType = "C" Or clsSession.userEmployeeType = "F" Then
                liLeaveBalance.Visible = True
            Else
                liLeaveBalance.Visible = False
            End If
            If clsSession.userIsDelegatedTimesheetManager Then
                liManageTimesheets.Visible = True
            Else
                liManageTimesheets.Visible = False
            End If
        End If
    End Sub

    Protected Function GetLeaveTimesheetTypeID() As Integer
        Return My.Settings.TimesheetTypeID_Leave
    End Function

    Protected Function GetTimeTimesheetTypeID() As Integer
        Return My.Settings.TimesheetTypeID_Time
    End Function

    Private Sub btnLogOut_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnLogOut.Click
        If clsSession.userSID IsNot Nothing Then
            Session.Abandon()
        End If

        Response.Redirect("default.aspx")
    End Sub

End Class