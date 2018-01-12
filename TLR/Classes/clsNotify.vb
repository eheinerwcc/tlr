Imports System.Data.SqlClient
Imports System.Web.Configuration

Public Class clsNotify
  Shared Sub SendEmail(ByVal strTo As String, ByVal strSubject As String, ByVal strBody As String, Optional ByVal strFrom As String = "", Optional ByVal blnHTML As Boolean = True, Optional ByVal blnHighPriority As Boolean = False)
    If strFrom = "" Then strFrom = My.Settings.Email_FromAddress

    Dim mailmsg As New System.Net.Mail.MailMessage
    Dim mailfrom As New System.Net.Mail.MailAddress(strFrom)
    Dim strRecipients() As String = strTo.Trim.Replace(" ", "").ToString.Split(";")
        Dim blnValidEmails As Boolean = True

    'Find the release mode from appSettings in web.config
    Dim releaseMode As String
    releaseMode = ConfigurationManager.AppSettings.Get("Release_Mode")

    'and then set email notification addresses 
    If (releaseMode = "dev") Then
      mailmsg.To.Add("dev@college.edu")
    ElseIf (releaseMode = "test") Then
      mailmsg.To.Add("test@college.edu")

    Else
      For Each strRecip As String In strRecipients
        If clsGeneric.IsValidEmailAddress(strRecip) Then
          mailmsg.To.Add(strRecip)
        Else
          blnValidEmails = False
        End If
      Next
    End If




    If blnValidEmails Then
      mailmsg.IsBodyHtml = blnHTML

      mailmsg.Priority = IIf(blnHighPriority, Net.Mail.MailPriority.High, Net.Mail.MailPriority.Normal)

      mailmsg.From = mailfrom
      mailmsg.Subject = strSubject
      mailmsg.Body = strBody
      Dim smtp As System.Net.Mail.SmtpClient = New System.Net.Mail.SmtpClient
      smtp.Host = IIf(My.Settings.SMTPServer = "", System.Environment.MachineName, My.Settings.SMTPServer)

      Try
        smtp.Send(mailmsg)
      Catch ex As Exception
        'if there was a problem sending email, there is no need to try sending an error email, but the error should be recorded
        clsErrorHandler.RecordErrorToDB(clsSession.userSID, "Error Sending Email", ex.ToString)
      End Try
    End If
  End Sub

  Shared Sub SendErrorEmail(ByVal strSubject As String, ByVal strBody As String, Optional ByVal strFrom As String = "")
    If strFrom = "" Then strFrom = My.Settings.Email_FromAddress

    SendEmail(My.Settings.Email_ErrorNotificationDistributionList, strSubject, strBody, strFrom)
  End Sub

  Shared Sub SendTemplateEmail(ByVal intTimeSheetID As Integer, ByVal strBody As String, Optional ByVal blnEmailSupervisor As Boolean = False)
    Dim strSubject As String = ""
    Dim strTo As String = ""

    Dim drTimeSheet As SqlDataReader
    drTimeSheet = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), Data.CommandType.StoredProcedure, "usp_SELECT_Timesheet", _
                                          New SqlParameter("@TimeSheetID", intTimeSheetID))

    If drTimeSheet.HasRows Then
      drTimeSheet.Read()

      'replace tags
      strBody = Replace(strBody, "#SUPERVISORFIRSTNAME#", drTimeSheet.Item("JobSupervisorFirstName"))
      strBody = Replace(strBody, "#SUPERVISORNAME#", drTimeSheet.Item("JobSupervisorDisplayName"))
      strBody = Replace(strBody, "#EMPLOYEEFIRSTNAME#", drTimeSheet.Item("FirstName"))
      strBody = Replace(strBody, "#EMPLOYEELASTNAME#", drTimeSheet.Item("LastName"))
      strBody = Replace(strBody, "#BEGINDATE#", drTimeSheet.Item("BeginDate"))
      strBody = Replace(strBody, "#ENDDATE#", drTimeSheet.Item("EndDate"))
      strBody = Replace(strBody, "#APP_URL#", My.Settings.RootURL)
      strBody = Replace(strBody, "#TIMESHEETID#", intTimeSheetID)

      'select recipient
      strTo = clsGeneric.DeNull(IIf(blnEmailSupervisor, drTimeSheet.Item("JobSupervisorEmail"), drTimeSheet.Item("WorkEmail")))

      drTimeSheet.Close()

      'obtain email subject from the tag and then delete it from the email body
      If strBody.Contains("#Subject#") And strBody.Contains("#/Subject#") Then
        strSubject = Mid(strBody, InStr(strBody, "#Subject#") + 9, InStr(strBody, "#/Subject#") - InStr(strBody, "#Subject#") - 9)
        strBody = Replace(strBody, "#Subject#" + strSubject + "#/Subject#", "")
      End If

      If strTo IsNot Nothing AndAlso clsGeneric.IsValidEmailAddress(strTo) Then
        SendEmail(strTo, strSubject, strBody)
      End If
    End If
  End Sub

  Shared Sub SendWorkScheduleUpdateNotice()
    'get supervisor email string (could contain more than one email if user has more than one OT eligible job)
    Dim strTo As String = clsGeneric.DeNull(SqlHelper.ExecuteScalar(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_SELECT_Employee_SupervisorEmailsString", _
                                                  New SqlParameter("@SID", clsSession.userSID), _
                                                  New SqlParameter("@WorkScheduleEligibleEmployeeTypes", My.Settings.WorkHoursEligibleEmployeeTypes)))

    Dim blnValidEmails As Boolean = True

    For Each strEmail As String In strTo.Trim.Replace(" ", "").Split(";")
      If Not clsGeneric.IsValidEmailAddress(strEmail) Then blnValidEmails = False
    Next

    If strTo IsNot Nothing AndAlso blnValidEmails Then
      Dim drWorkSchedule As SqlDataReader = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkScheduleEntry", _
                                                       New SqlParameter("@SID", clsSession.userSID))
      Dim strWorkSchedule As String = "<table style='font: 0.8em verdana; border: solid 1px black; border-collapse: collapse; background-color: white;'><tr><th style='text-align: left; padding-left: 1em;'>Day</th><th style='text-align: left; padding-left: 1em;'>Start</th><th style='text-align: left; padding-left: 1em;'>End</th><th style='text-align: left; padding-left: 1em;'>Meal</th><th style='text-align: left; padding-left: 1em;'>Total</th></tr>"
      While drWorkSchedule.Read
        strWorkSchedule += "<tr>"
        strWorkSchedule += "<td style='padding: .2em 1em .2em 1em; text-align: left;'>" + System.Enum.GetValues(GetType(DayOfWeek))(drWorkSchedule.Item("DayOfWeek") - 1).ToString + "</td>"
        strWorkSchedule += "<td style='padding: .2em 1em .2em 1em; text-align: left;'>" + Format(drWorkSchedule.Item("StartTime"), "t") + "</td>"
        strWorkSchedule += "<td style='padding: .2em 1em .2em 1em; text-align: left;'>" + Format(drWorkSchedule.Item("EndTime"), "t") + "</td>"
        strWorkSchedule += "<td style='padding: .2em 1em .2em 1em; text-align: left;'>" + drWorkSchedule.Item("MealTime").ToString + "<abbr title='Minutes'>min.</abbr></td>"
        strWorkSchedule += "<td style='padding: .2em 1em .2em 1em; text-align: left;'>" + Math.Floor(drWorkSchedule.Item("TotalMinutes") / 60).ToString + "<abbr title='Hours'> hrs.</abbr> " + IIf(drWorkSchedule.Item("TotalMinutes") Mod 60 = 0, "", CInt(drWorkSchedule.Item("TotalMinutes") Mod 60).ToString + " <abbr title='Minutes'>mins.</abbr>") + "</td>"
        strWorkSchedule += "</tr>"
      End While
      strWorkSchedule += "</table>"

      Dim strSubject As String = ""
      Dim strBody As String = Resources.GlobalText.Email_WorkScheduleUpdated_NotifySupervisor
      strBody = Replace(strBody, "#EMPLOYEENAME#", clsSession.userDisplayName)
      strBody = Replace(strBody, "#EMPLOYEEFIRSTNAME#", clsSession.userFirstName)
      strBody = Replace(strBody, "#EMPLOYEELASTNAME#", clsSession.userLastName)
      If strBody.Contains("#Subject#") And strBody.Contains("#/Subject#") Then
        strSubject = Mid(strBody, InStr(strBody, "#Subject#") + 9, InStr(strBody, "#/Subject#") - InStr(strBody, "#Subject#") - 9)
        strBody = Replace(strBody, "#Subject#" + strSubject + "#/Subject#", "")
      End If
      strBody = Replace(strBody, "#WORKSCHEDULE#", strWorkSchedule)
      strBody = Replace(strBody, "#APP_URL#", My.Settings.RootURL)

      SendEmail(strTo, strSubject, strBody)
    End If
  End Sub
End Class
