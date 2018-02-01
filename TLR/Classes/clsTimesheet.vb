Imports System.Data
Imports System.Data.SqlClient

Public Class clsTimesheet

    Shared Sub DeleteTimesheet(ByVal intTimesheetID As Integer)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_TimesheetAction_DELETE", _
                          New SqlParameter("@TimesheetID", intTimesheetID))
    End Sub

  Shared Sub RouteTimesheet(ByVal intTimesheetID As Integer, Optional ByVal blnIsDelegated As Boolean = False)

    SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_TimesheetAction_ROUTE", _
                              New SqlParameter("@TimesheetID", intTimesheetID), _
                              New SqlParameter("@FinAidEarningTypes", My.Settings.FinancialAidEarningTypes), _
                              New SqlParameter("@blnIsDelegated", blnIsDelegated))
  End Sub


    Public Shared Sub LogTimesheetAction(ByVal intTimesheetID As Integer, ByVal intActionTypeID As Integer, ByVal strActionBy As String, ByVal strComment As String)
        SqlHelper.ExecuteScalar(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_INSERT_TimesheetAction", _
                                New SqlParameter("@TimesheetID", intTimesheetID), _
                                New SqlParameter("@ActionTypeID", intActionTypeID), _
                                New SqlParameter("@ActionBy", strActionBy), _
                                New SqlParameter("@Comment", strComment))
    End Sub

    Shared Sub RejectTimesheet(ByVal intTimesheetID As Integer)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_TimesheetAction_REJECT", _
                          New SqlParameter("@TimesheetID", intTimesheetID))
    End Sub

    Shared Function GetTotalAccruedOT(ByVal intTimesheetID As Integer) As Double
        Dim dblTotalOT As Double = SqlHelper.ExecuteScalar(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Timesheet_OTHours", _
                                                           New SqlParameter("@TimesheetID", intTimesheetID))
        Return dblTotalOT
    End Function

    Shared Sub RedirectOnOutdatedStatus(ByVal intTimesheetID As Integer, ByVal intTimesheetStatusID As Integer)
        If intTimesheetStatusID <> Nothing Then
            Dim dsTimesheet As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Timesheet", _
                                                   New SqlParameter("@TimesheetID", intTimesheetID))

            If dsTimesheet.Tables(0).Rows.Count = 0 Then
                HttpContext.Current.Response.Redirect("~/home.aspx")
            Else
                If dsTimesheet.Tables(0).Rows(0).Item("TimesheetStatusID") <> intTimesheetStatusID Then HttpContext.Current.Response.Redirect("~/Timesheet.aspx?TimesheetID=" + intTimesheetID.ToString)
            End If
        End If
    End Sub

    Shared Sub UploadOT(ByVal intTimesheetID As Integer, ByVal dblOT_Pay As Double, ByVal dblOT_Convert As Double)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_Timesheet_OTHours", _
                                  New SqlParameter("@TimesheetID", intTimesheetID), _
                                  New SqlParameter("@OTPay", dblOT_Pay), _
                                  New SqlParameter("@OTComp", dblOT_Convert))
    End Sub

    Shared Function GetAlternateSignersDisplayString(ByVal intTimesheetID As Integer) As String
        Return SqlHelper.ExecuteScalar(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Timesheet_AlternateSignersString", _
                                       New SqlParameter("@TimesheetID", intTimesheetID))
    End Function

    Shared Function GetBudgetsDisplayString(ByVal intTimesheetID As Integer) As String
        Return SqlHelper.ExecuteScalar(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Timesheet_BudgetsString", _
                                       New SqlParameter("@TimesheetID", intTimesheetID))
    End Function

    Shared Sub UploadBudgetDataPerTimesheet(ByVal intTimesheetID As Integer, ByVal strBudgetData As String)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_TimesheetBudget_HoursDistribution", _
                                  New SqlParameter("@TimesheetID", intTimesheetID), _
                                  New SqlParameter("@BudgetInfo", strBudgetData))
    End Sub

    Shared Sub UploadBudgetDataPerEntry(ByVal intTimesheetID As Integer, ByVal strBudgetData As String)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_INSERT_TimesheetEntryBudgetDistribution", _
                                  New SqlParameter("@TimesheetID", intTimesheetID), _
                                  New SqlParameter("@BudgetInfo", strBudgetData))
    End Sub

    Shared Function IsTimesheetEmpty(ByVal intTimesheetID As Integer) As Boolean
        Return SqlHelper.ExecuteScalar(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_IS_TimesheetEmpty", _
                                New SqlParameter("@TimesheetID", intTimesheetID)) = 1
    End Function

    Shared Sub DeleteBudgetAllocations(ByVal intTimesheetID As Integer)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_DELETE_TimesheetEntryBudgetDistribution", _
                                  New SqlParameter("@TimesheetID", intTimesheetID))
    End Sub

    Shared Function RemarkRequired(ByVal intTimesheetID As Integer) As Boolean
        Return SqlHelper.ExecuteScalar(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_IS_TimesheetRequiresRemark", _
                                New SqlParameter("@TimesheetID", intTimesheetID), _
                                New SqlParameter("@LeaveTypes", My.Settings.EntryTypeRequiresRemark)) = 1
    End Function

    Shared Function IsHourlyLeaveEligible(ByVal strJobEmployeeTypeID) As Boolean
        'Returns whether timesheet job is hourly leave eligible
        Dim eligibleTypeList = My.Settings.JobEmployeeType_AllowsHourlyLeave.Split(",")
        If Not eligibleTypeList Is Nothing AndAlso eligibleTypeList.Contains(strJobEmployeeTypeID) Then
            Return True
        Else
            Return False
        End If
    End Function

    Shared Function IsValidLeaveEarningType(ByVal strEarningType) As Boolean
        'Returns whether timesheet job is hourly leave eligible
        Dim invalidTypeList = My.Settings.BudgetEarningType_LeaveNotAllowed.Trim.Replace(" ", "").Split(",")
        If Not invalidTypeList Is Nothing AndAlso invalidTypeList.Contains(strEarningType) Then
            Return False
        Else
            Return True
        End If
    End Function

    Shared Function CalcuateTimesheetEntryDuration(ByVal dateStartTime As Date, ByVal dateEndTime As Date)
        'Calculates duration in hours of entry given start and end times
        'Assumes validity of timesheet entry has otherwise been checked before arriving here
        Return Math.Abs(DateDiff(DateInterval.Minute, dateStartTime, dateEndTime) / 60)
    End Function
End Class
