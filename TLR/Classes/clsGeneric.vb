Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Configuration

Public Class clsGeneric
    Shared Function IsValidIntegerQuerystring(ByVal obj As System.Object) As Boolean
        If obj Is Nothing Then Return False
        If obj.ToString.Trim = "" Then Return False
        If Not IsNumeric(obj) Then Return False
        If obj.ToString.Contains(".") Then Return False

        Return True
    End Function

    Shared Function DeNull(ByVal ThisValue As System.Object) As System.Object
        Return IIf(ThisValue Is DBNull.Value, "", ThisValue)
    End Function

    Shared Sub RedirectOnSessionTimeout()
        If clsSession.userSID Is Nothing Then HttpContext.Current.Response.Redirect("~/Default.aspx")
    End Sub

    Shared Function IsValidSQLDate(ByVal strDate As String) As Boolean
        ' Returns True if the Date is Valid for SQL (Matches Pattern)
        strDate = Trim$(strDate)
        Return Regex.IsMatch(strDate, "^(?:(?:(?:0?[13578]|1[02])(\/|-|\.)31)\1|(?:(?:0?[13-9]|1[0-2])(\/|-|\.)(?:29|30)\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:0?2(\/|-|\.)29\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:(?:0?[1-9])|(?:1[0-2]))(\/|-|\.)(?:0?[1-9]|1\d|2[0-8])\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$")
    End Function

    Shared Function IsAlphaNumeric(ByVal strValue As String) As Boolean
        Return Regex.IsMatch(strValue, "[A-Za-z0-9]")
    End Function

    Shared Function IsExpectedNumber(ByVal strValue As String, ByVal intAllowedPrecision As Integer) As Boolean
        If strValue = "" Then
            Return False
        ElseIf intAllowedPrecision = 0 Then
            Return Regex.IsMatch(strValue, "^d{0,3}")
        Else
            Return Regex.IsMatch(strValue, "^\d{0,3}(\.\d{1," + intAllowedPrecision.ToString + "})?$")
        End If
    End Function

    Shared Function IsExpectedPIN(ByVal strValue As String) As Boolean
        Return Regex.IsMatch(strValue, "^\d{1,6}$")
    End Function

    Shared Function PrepareForDisplay(ByVal strInput As String) As String
        Return Replace(Replace(DeNull(strInput), vbCrLf, "<br />"), "  ", "&nbsp; ")
    End Function

    Shared Sub RedirectIfNotPayrollAdmin()
        If Not clsSession.userIsPayrollAdmin Then HttpContext.Current.Response.Redirect("~/Home.aspx")
    End Sub

    Shared Sub RedirectIfNotHRAdmin()
        If Not clsSession.userIsPayrollAdmin Then HttpContext.Current.Response.Redirect("~/Home.aspx")
    End Sub

    Shared Sub RedirectIfNotSupervisor()
        If Not clsSession.userIsSupervisor Then HttpContext.Current.Response.Redirect("~/Home.aspx")
    End Sub

    Shared Sub RedirectIfNotWorkScheduleEligible()
        If Not clsSession.userIsWorkScheduleEligible Then HttpContext.Current.Response.Redirect("~/Home.aspx")
    End Sub

    Shared Sub RedirectIfNotDelegatedTimesheetManager()
        If Not clsSession.userIsDelegatedTimesheetManager Then HttpContext.Current.Response.Redirect("~/Home.aspx")
    End Sub

    Public Shared Function SearchUsers_StringArray(ByVal prefixText As String, ByVal count As Integer, Optional ByVal blnActiveEmployeesOnly As Boolean = True) As String()
        clsGeneric.RedirectOnSessionTimeout()

        Dim dsSearchResults As DataSet = SearchUsers_Dataset(prefixText, blnActiveEmployeesOnly)
        Dim strResults() As String
        Dim intResultDisplayCount As Integer = IIf(dsSearchResults.Tables(0).Rows.Count > count, count, dsSearchResults.Tables(0).Rows.Count)

        ReDim strResults(intResultDisplayCount - 1) '(dsSearchResults.Tables(0).Rows.Count - 1)

        For intCounter As Integer = 0 To intResultDisplayCount - 1 'dsSearchResults.Tables(0).Rows.Count - 1
            strResults(intCounter) = dsSearchResults.Tables(0).Rows(intCounter).Item("SearchString").ToString
        Next

        Return strResults
    End Function

    Public Shared Function SearchUsers_Dataset(ByVal strSearchText As String, Optional ByVal blnActiveEmployeesOnly As Boolean = True) As DataSet
        Return SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_Search_Employee", _
                                         New SqlParameter("@SearchString", strSearchText), _
                                         New SqlParameter("@SearcherSID", clsSession.userSID), _
                                         New SqlParameter("@IsSupervisor", clsSession.userIsSupervisor), _
                                         New SqlParameter("@IsFinAidAdmin", clsSession.userIsFinAidAdmin), _
                                         New SqlParameter("@IsPayrollAdmin", clsSession.userIsPayrollAdmin), _
                                         New SqlParameter("@IsFinanceAdmin", clsSession.userIsFinanceAdmin), _
                                         New SqlParameter("@IsHRAdmin", clsSession.userIsHRAdmin), _
                                         New SqlParameter("@GrantBudgetAPPR", My.Settings.GrantBudgetAPPR), _
                                         New SqlParameter("@FinAidEarningTypes", My.Settings.FinancialAidEarningTypes), _
                                         New SqlParameter("@ActiveEmployeesOnly", blnActiveEmployeesOnly))
    End Function


    Shared Function IsValidEmailAddress(ByVal strEmailAddress As String) As Boolean
        ' Returns True if the E-Mail Address is Valid (Matches Pattern)
        strEmailAddress = Trim$(strEmailAddress)
        Return Regex.IsMatch(strEmailAddress, "^[\w\.\-]+@[a-zA-Z0-9\-]+(\.[a-zA-Z0-9\-]{1,})*(\.[a-zA-Z]{2,3}){1,2}$")
    End Function

	Shared Public Function GetEmployeeAnniversaryDate (ByVal sid As String) As String
    'Throw New NotImplementedException()

    Dim ds As New DataSet
    Dim dt As DataTable
    Dim anniversaryDate As String

    anniversaryDate = ""

    ds = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_GET_EmployeeAnniversary", _
                                         New SqlParameter("@SID", sid))

    dt = ds.Tables(0)
    For Each dr In dt.Rows
      anniversaryDate += dr("anniversaryDate")
    Next

    Return anniversaryDate
  End Function
End Class
