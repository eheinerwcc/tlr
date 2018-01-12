Imports Microsoft.VisualBasic

Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web
Imports System.Web.Mail
Imports System.Collections.Specialized

Public Class clsErrorHandler
    Shared Sub RecordError(ByVal EX As Exception)
        Dim strHeading As String = "<TABLE BORDER='0' WIDTH='100%' CELLPADDING='1' CELLSPACING='0'><TR><TD bgcolor='black' COLSPAN='2'><FONT face='Arial' color='white'><B> <!--HEADER--></B></FONT></TD></TR></TABLE>"
        Dim strHTML As String = "<FONT face='Arial' size='5' color='red'>Error - " + EX.Message + "</FONT><BR><BR>"
        Dim error_info As New NameValueCollection
        error_info.Add("Date", Now.ToString())
        error_info.Add("Message", CleanHTML(EX.Message))
        error_info.Add("Source", CleanHTML(EX.Source))
        error_info.Add("TargetSite", CleanHTML(EX.TargetSite.ToString()))
        error_info.Add("StackTrace", CleanHTML(EX.StackTrace))

        ' Error Information
        strHTML += strHeading.Replace("<!--HEADER-->", "Error Information")
        strHTML += collectionToHtmlTable(error_info)

        'html error message (if available)
        'Try
        '    strHTML += strHeading.Replace("<!--HEADER-->", "HTML Error Message")
        '    Dim ex2 As HttpException = DirectCast(EX.GetBaseException(), HttpException)
        '    strHTML += ex2.GetHtmlErrorMessage().ToString()
        'Catch ex2 As Exception
        '    'do nothing - .message is not available, just continue
        'End Try

        ' QueryString Collection
        strHTML += "<BR><BR>" + strHeading.Replace("<!--HEADER-->", "QueryString Collection")
        strHTML += collectionToHtmlTable(HttpContext.Current.Request.QueryString)

        ' Form Collection
        strHTML += "<BR><BR>" + strHeading.Replace("<!--HEADER-->", "Form Collection")
        strHTML += collectionToHtmlTable(HttpContext.Current.Request.Form)

        ' Cookies Collection
        strHTML += "<BR><BR>" + strHeading.Replace("<!--HEADER-->", "Cookies Collection")
        strHTML += collectionToHtmlTable(HttpContext.Current.Request.Cookies)

        ' Session Variables
        strHTML += "<BR><BR>" + strHeading.Replace("<!--HEADER-->", "Session Variables")
        strHTML += collectionToHtmlTable(HttpContext.Current.Session)

        ' Server Variables
        strHTML += "<BR><BR>" + strHeading.Replace("<!--HEADER-->", "Server Variables")
        strHTML += collectionToHtmlTable(HttpContext.Current.Request.ServerVariables)

        'Environment(Variables)
        strHTML += "<BR><BR>" + strHeading.Replace("<!--HEADER-->", "Environment Variables")
        strHTML += collectionToHtmlTable(System.Environment.GetEnvironmentVariables)

        'browser, time, rawurl
        strHTML += "<BR><BR>" + strHeading.Replace("<!--HEADER-->", "User Browser Information")
        strHTML += collectionToHtmlTable(HttpContext.Current.Request.Browser)


        'record error in DB
        RecordErrorToDB(IIf(clsSession.userSID Is Nothing, "No User", clsSession.userSID), EX.Message.ToString, strHTML)

        'send email with error to responsible address(es)
        clsNotify.SendErrorEmail(My.Settings.Email_ErrorSubject, strHTML)
    End Sub

    Shared Function CleanHTML(ByVal strHTML As String) As String
        Return (IIf(strHTML.Length = 0, "", strHTML.Replace("<", "<").Replace(vbCrLf, "<BR>").Replace("&", "&amp").Replace(" ", " ")))
    End Function

    Shared Function collectionToHtmlTable(ByVal collection As HttpBrowserCapabilities) As String
        Dim NVC As New NameValueCollection

        If Not collection Is Nothing Then
            For Each pi As System.Reflection.PropertyInfo In collection.GetType.GetProperties
                NVC.Add(pi.Name, collection.Item(pi.Name))
            Next
        End If

        Return collectionToHtmlTable(NVC)
    End Function

    Shared Function CollectionToHtmlTable(ByVal collection As HttpCookieCollection) As String
        Dim NVC As New NameValueCollection

        If Not collection Is Nothing Then
            For Each strItem As String In collection
                NVC.Add(strItem, collection(strItem).Value)
            Next
        End If

        Return CollectionToHtmlTable(NVC)
    End Function

    Shared Function CollectionToHtmlTable(ByVal collection As System.Web.SessionState.HttpSessionState) As String
        Dim NVC As New NameValueCollection

        If Not collection Is Nothing Then
            For Each strItem As String In collection
                NVC.Add(strItem, collection(strItem).ToString())
            Next
        End If
        Return CollectionToHtmlTable(NVC)
    End Function

    'for system environment variables
    Shared Function collectionToHtmlTable(ByVal collection As IDictionary) As String
        Dim NVC As New NameValueCollection
        Dim dictEnum As IDictionaryEnumerator = collection.GetEnumerator

        While dictEnum.MoveNext
            NVC.Add(dictEnum.Entry.Key, dictEnum.Entry.Value)
        End While
        'If Not collection Is Nothing Then
        '    For Each pi As System.Reflection.PropertyInfo In collection..GetType.GetProperties
        '        NVC.Add(pi.Name, collection.Item(pi.Name))
        '    Next
        'End If

        Return collectionToHtmlTable(NVC)
    End Function

    Shared Function CollectionToHtmlTable(ByVal collection As NameValueCollection) As String
        Dim strTD As String = "<TD><FONT face='Arial' size='2'><!--VALUE--></FONT></TD>"

        Dim strHTML As String = vbCrLf + "<TABLE width='100%'>" _
            + "  <TR bgcolor='#C0C0C0'>" + strTD.Replace("<!--VALUE-->", " <B>Name</B>") _
            + "  " + strTD.Replace("<!--VALUE-->", " <B>Value</B>") + "</TR>" + vbCrLf

        ' No Body? -> N/A
        If (collection.Count = 0) Then
            collection = New NameValueCollection
            collection.Add("N/A", "")
        End If

        ' Table Body
        For i As Integer = 0 To collection.Count - 1
            strHTML += "<TR valign='top' bgcolor='" + (IIf(i Mod 2 = 0, "white", "EEEEEE")) + "'>" _
                + strTD.Replace("<!--VALUE-->", clsGeneric.DeNull(collection.Keys(i))) + vbCrLf _
                + strTD.Replace("<!--VALUE-->", clsGeneric.DeNull(collection(i))) + "</TR>" + vbCrLf
        Next

        ' Table Footer
        Return strHTML + "</TABLE>"
    End Function

    Shared Sub RecordErrorToDB(ByVal strUser As String, ByVal strErrorDescription As String, ByVal strErrorReport As String)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_INSERT_ApplicationError", _
        New SqlParameter("@LoggedOnUser", strUser), _
        New SqlParameter("@ErrorDescription", strErrorDescription), _
        New SqlParameter("@ErrorReport", strErrorReport))
    End Sub
End Class
