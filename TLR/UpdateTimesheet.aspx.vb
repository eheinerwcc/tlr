Imports System.Data.SqlClient
Imports System.Web.Services

Partial Public Class UpdateTimesheet
  Inherits System.Web.UI.Page

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    clsGeneric.RedirectOnSessionTimeout()
    uclFeedback.ResetDisplay()

    If Not Me.IsPostBack Then
      clsGeneric.RedirectIfNotPayrollAdmin()

      If clsGeneric.IsValidIntegerQuerystring(Request.QueryString("TimesheetID")) Then
        BindTimesheetDetails()
      Else
        uclFeedback.DisplayError(Resources.GlobalText.Error_UpdateTimesheet_InvalidTimesheetID)
      End If
    End If
  End Sub

  Private Sub BindTimesheetDetails()
    pnlUpdateTimesheet.Visible = True
    Dim intTimesheetID As Integer = Request.QueryString("TimesheetID")

    Dim dtTimesheet As DataTable = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Timesheet", _
                                   New SqlParameter("@TimesheetID", intTimesheetID)).Tables(0)

    If dtTimesheet.Rows.Count > 0 Then
      With dtTimesheet.Rows(0)
        lblTSPeriod.Text = .Item("BeginDate").ToString & " - " & .Item("EndDate").ToString
        lblStatus.Text = .Item("StatusName")
        lblName.Text = .Item("DisplayName") + " (" + .Item("SID") + ")"
        lblJobTitle.Text = .Item("JobClassNameLong")

        'if there is no supervisor, convert the null value from the database to a string.
        If (.Item("JobSupervisorDisplayName") Is Nothing) Then
          lblSupervisor.Text = ""
        Else
          lblSupervisor.Text = .Item("JobSupervisorDisplayName").ToString()
        End If


        If .Item("TimesheetTypeID") = My.Settings.TimesheetTypeID_Time Then
          liBudgets.Visible = True
          lblBudget.Text = clsTimesheet.GetBudgetsDisplayString(intTimesheetID)

          liPayRate.Visible = True
          lblPayRate.Text = .Item("PayRate")
        End If
      End With
    Else
      pnlUpdateTimesheet.Visible = False
      uclFeedback.DisplayError(Resources.GlobalText.Error_UpdateTimesheet_TimesheetIDNotFound)
    End If
  End Sub

  <WebMethod()> _
  <Script.Services.ScriptMethod()> _
  Public Shared Function SearchUsers(ByVal prefixText As String, ByVal count As Integer) As String()
    clsGeneric.RedirectOnSessionTimeout()
    Dim dsSearchResults As DataSet = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_Search_Supervisor", _
                                     New SqlParameter("@SearchString", prefixText))

    Dim strResults() As String
    Dim intResultDisplayCount As Integer = IIf(dsSearchResults.Tables(0).Rows.Count > count, count, dsSearchResults.Tables(0).Rows.Count)

    ReDim strResults(intResultDisplayCount - 1)

    For intCounter As Integer = 0 To intResultDisplayCount - 1
      strResults(intCounter) = dsSearchResults.Tables(0).Rows(intCounter).Item("SearchString").ToString
    Next

    Return strResults
  End Function

  Private Sub HideUpdatePanels()
    pnlUpdateBudgets.Visible = False
    pnlUpdatePayRate.Visible = False
    pnlUpdateSupervisor.Visible = False
  End Sub

  Private Sub btnEditSupervisor_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditSupervisor.Click
    HideUpdatePanels()
    pnlUpdateSupervisor.Visible = True
    rptUserSearchResults.Visible = False
    txtNewSupervisor.Text = ""
  End Sub

  Private Sub btnSearchSupervisors_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSearchSupervisors.Click
    h2NoSearchResults.Visible = False
    rptUserSearchResults.Visible = False

    Dim dsSearchResults As DataSet = clsGeneric.SearchUsers_Dataset(txtNewSupervisor.Text)

    If dsSearchResults.Tables(0).Rows.Count = 1 Then
      UpdateSupervisor(dsSearchResults.Tables(0).Rows(0).Item("SID"))
      HideUpdatePanels()
    ElseIf dsSearchResults.Tables(0).Rows.Count = 0 Then
      h2NoSearchResults.Visible = True
    Else
      rptUserSearchResults.Visible = True
      rptUserSearchResults.DataSource = dsSearchResults
      rptUserSearchResults.DataBind()
    End If
  End Sub

  Protected Sub SupervisorSelected(ByVal sender As Object, ByVal e As System.EventArgs)
    UpdateSupervisor(DirectCast(sender, Button).CommandArgument)
    HideUpdatePanels()
  End Sub

  Private Sub UpdateSupervisor(ByVal strSID As String)
    SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_Timesheet_JobSupervisor", _
                              New SqlParameter("@TimesheetID", Request.QueryString("TimesheetID")), _
                              New SqlParameter("@SID", strSID))
    uclFeedback.DisplaySuccess(Resources.GlobalText.Success_UpdateTimesheet_SupervisorUpdated)
    HideUpdatePanels()
    BindTimesheetDetails()
  End Sub

  Private Sub btnEditBudgets_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditBudgets.Click
    Dim intTimesheetID As Integer = Request.QueryString("TimesheetID")

    Dim dtTimesheet As DataTable = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_Timesheet", _
                                   New SqlParameter("@TimesheetID", intTimesheetID)).Tables(0)

    If dtTimesheet.Rows(0).Item("TimesheetStatusID") <= My.Settings.TimesheetStatus_AwaitingSupervisorApproval Then
      HideUpdatePanels()
      pnlUpdateBudgets.Visible = True
      txtNewBudget.Text = ""
      ddlEarningTypes.ClearSelection()
      ddlEarningTypes.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_EarningType")
      ddlEarningTypes.DataBind()
      BindBudgets()
    Else
      uclFeedback.DisplayError(Resources.GlobalText.Error_UpdateTimesheet_BudgetNotEditable)
    End If
  End Sub

  Private Sub BindBudgets()
    rptBudgets.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_TimesheetBudget", _
                                                    New SqlParameter("@TimesheetID", Request.QueryString("TimesheetID")))
    rptBudgets.DataBind()
  End Sub

  Protected Sub DeleteBudget(ByVal sender As Object, ByVal e As System.EventArgs)
    If rptBudgets.Items.Count > 1 Then
      SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_DELETE_TimesheetBudget", _
                                New SqlParameter("@TimesheetBudgetID", DirectCast(sender, Button).CommandArgument))
      uclFeedback.DisplaySuccess("Budget number successfully deleted from this timesheet.")
      BindTimesheetDetails()
      BindBudgets()
    Else
      uclFeedback.DisplayError(Resources.GlobalText.Error_UpdateTimesheet_OneBudgetNecessary)
    End If
  End Sub

  Private Sub btnAddBudget_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddBudget.Click
    Dim strNewBudget As String = txtNewBudget.Text

    If strNewBudget.Length = 14 And clsGeneric.IsAlphaNumeric(strNewBudget) Then
      SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_INSERT_TimesheetBudget", _
                                New SqlParameter("@TimesheetID", Request.QueryString("TimesheetID")), _
                                New SqlParameter("@BudgetNumber", strNewBudget), _
                                New SqlParameter("@EarningTypeID", ddlEarningTypes.SelectedValue))
      uclFeedback.DisplaySuccess(Resources.GlobalText.Success_UpdateTimesheet_BudgetNumberAdded)
      BindBudgets()
      BindTimesheetDetails()
      txtNewBudget.Text = ""
    Else
      uclFeedback.DisplayError(Resources.GlobalText.Error_UpdateTimesheet_InvalidBudgetNumber)
    End If
  End Sub

  Private Sub btnEditPayRate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditPayRate.Click
    HideUpdatePanels()
    pnlUpdatePayRate.Visible = True
    txtNewPayRate.Text = lblPayRate.Text.Replace("$", "")
  End Sub

  Private Sub btnUpdatePayRate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpdatePayRate.Click
    Dim blnValidPayRate As Boolean = True
    Dim strNewPayRate As String = txtNewPayRate.Text.Trim.Replace("$", "")

    If clsGeneric.IsExpectedNumber(strNewPayRate, 3) AndAlso CDbl(strNewPayRate) > 0 Then
      If blnValidPayRate Then
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_Timesheet_PayRate", _
                                  New SqlParameter("@TimesheetID", Request.QueryString("TimesheetID")), _
                                  New SqlParameter("@PayRate", txtNewPayRate.Text))
        uclFeedback.DisplaySuccess("Pay Rate for this timesheet has been successfully updated.")
        txtNewPayRate.Text = ""
        HideUpdatePanels()
        BindTimesheetDetails()
      End If
    Else
      uclFeedback.DisplayError(Resources.GlobalText.Error_UpdateTimesheet_InvalidPayRate)
    End If
  End Sub
End Class