Imports System.Data.SqlClient


Partial Public Class Timesheet
    Inherits System.Web.UI.Page

    Private dtTimesheet As DataTable 'IQueryable(Of TLR.vw_Timesheet)
    Dim intTSID As Integer

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()

        If clsGeneric.IsValidIntegerQuerystring(Request.QueryString("TimesheetID")) Then
            intTSID = Request.QueryString("TimesheetID")

            uclTSFullTime.Visible = False
            uclTSPartTime.Visible = False

            Dim dsTimesheet As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Timesheet", _
                                                   New SqlParameter("@TimesheetID", intTSID))

            If dsTimesheet.Tables(0).Rows.Count <> 0 Then
                dtTimesheet = dsTimesheet.Tables(0)
                If IsUserAllowedViewer(intTSID, clsSession.userSID) Then
                    LoadTimesheet(intTSID)
                Else
                    uclFeedback.DisplayError(Resources.GlobalText.Error_TIMESHEET_NoPermission)
                End If
            Else
                uclFeedback.DisplayError(Resources.GlobalText.Error_TIMESHEET_TSIDNotFound)
            End If
        Else
            uclFeedback.DisplayError(Resources.GlobalText.Error_TIMESHEET_InvalidTSID)
        End If
    End Sub

    Private Sub LoadTimesheet(ByVal intTSID As Integer)
        Dim intTSTypeID As Integer = dtTimesheet.Rows(0).Item("TimesheetTypeID")
        Dim intTSStatusID As Integer = dtTimesheet.Rows(0).Item("TimesheetStatusID")

        uclTSFullTime.TimesheetStatusID = intTSStatusID
        uclTSPartTime.TimesheetStatusID = intTSStatusID
        uclTSFullTime.TSID = intTSID
        uclTSPartTime.TSID = intTSID
        uclTSFullTime.Visible = (intTSTypeID = My.Settings.TimesheetTypeID_Leave)
        uclTSPartTime.Visible = (intTSTypeID = My.Settings.TimesheetTypeID_Time)
    End Sub

    Private Function IsUserAllowedViewer(ByVal intTSID As Integer, ByVal strSID As String) As Boolean
        Dim blnIsUserAllowedViewer As Boolean = False

        If strSID = dtTimesheet.Rows(0).Item("SID") Then
            uclTSFullTime.IsOwner = True
            uclTSPartTime.IsOwner = True
            blnIsUserAllowedViewer = True
        End If

        'is the user someone with delegated rights to create/edit this timesheet?
        If SqlHelper.ExecuteScalar(SqlHelper.GetConnString, CommandType.StoredProcedure, "usp_IS_DelegatedUser", _
                                   New SqlParameter("@SID", strSID), _
                                   New SqlParameter("@TimesheetID", intTSID)) = 1 Then
            uclTSFullTime.IsDelegated = True
            uclTSPartTime.IsDelegated = True
        End If

        If clsSession.userIsPayrollAdmin Then
            blnIsUserAllowedViewer = True
        End If

        If clsSession.userIsHRAdmin Then
            blnIsUserAllowedViewer = True
        End If

        If SqlHelper.ExecuteScalar(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_IS_TimesheetSupervisor", _
                                New SqlParameter("@TimesheetID", intTSID), _
                                New SqlParameter("@SID", strSID)) = 1 Then
            uclTSFullTime.IsSupervisor = True
            uclTSPartTime.IsSupervisor = True
            blnIsUserAllowedViewer = True
        End If


        'Allow Finance personnel to view grant timesheets
        Dim blnContainsGrantBudget As Boolean = False
        Dim blnContainsFinancialAidBudget As Boolean = False

        Dim dsBudgets As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_TimesheetTotals_TIME", _
                                                                          New SqlParameter("@TimesheetID", intTSID))

        If dsBudgets.Tables(0).Rows.Count <> 0 Then
            For Each dr As DataRow In dsBudgets.Tables(0).Rows

                For Each strGrantBudgetAPPR As String In My.Settings.GrantBudgetAPPR.Trim.Replace(" ", "").ToString.Split(",")
                    If Left(dr.Item("BudgetNumber").ToString, 3) = strGrantBudgetAPPR Then
                        blnContainsGrantBudget = True
                        Exit For
                    End If
                Next

                For Each strFinancialAidEarningType As String In My.Settings.FinancialAidEarningTypes.Trim.Replace(" ", "").ToString.Split(",")
                    If dr.Item("EarningTypeID").ToString = strFinancialAidEarningType Then
                        blnContainsFinancialAidBudget = True
                        Exit For
                    End If
                Next

            Next
        End If

        If blnContainsGrantBudget And clsSession.userIsFinanceAdmin Then
            blnIsUserAllowedViewer = True
        End If

        'Allow Financial Aid personnel to view Financial Aid associated timesheets
        If blnContainsFinancialAidBudget And clsSession.userIsFinAidAdmin Then
            blnIsUserAllowedViewer = True
        End If

        Return blnIsUserAllowedViewer
    End Function

End Class