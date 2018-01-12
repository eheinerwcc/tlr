Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Services

Partial Public Class SearchTimesheets
    Inherits System.Web.UI.Page


  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    clsGeneric.RedirectOnSessionTimeout()
    uclFeedback.ResetDisplay()

    If Not Me.IsPostBack Then
      BindControls()
      If clsSession.userIsPayrollAdmin Or clsSession.userIsHRAdmin Then liType.Visible = True


    End If
  End Sub

    Private Sub BindControls()
        cblStatuses.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_TimesheetStatus")
        cblStatuses.DataBind()

        For Each li As ListItem In cblStatuses.Items
            li.Selected = True
        Next

        cblTypes.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_TimesheetType")
        cblTypes.DataBind()

        For Each li As ListItem In cblTypes.Items
            li.Selected = True
    Next

    rptEmployeeTypes.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_TimesheetEmployeeType")
    rptEmployeeTypes.DataBind()


    End Sub

    <WebMethod()> _
    <Script.Services.ScriptMethod()> _
    Public Shared Function SearchUsers(ByVal prefixText As String, ByVal count As Integer) As String()
        clsGeneric.RedirectOnSessionTimeout()

        Return clsGeneric.SearchUsers_StringArray(prefixText, count)
    End Function

    Protected Sub SearchTimesheets(ByVal strDisplayString As String)

    pnlTimesheetSearchResults.Visible = False
    pNoSearchResults.Visible = False
    rptUserSearchResults.Visible = False

    Dim strSelectedStatuses As String = ""
    For Each li As ListItem In cblStatuses.Items
      If li.Selected Then strSelectedStatuses += li.Value + ","
    Next

    Dim strSelectedTypes As String = ""
    For Each li As ListItem In cblTypes.Items
      If li.Selected Then strSelectedTypes += li.Value + ","
    Next


    Dim strSelectedEmployeeType As String = ""

    Try
      Dim employeeTypeCheckBox As CheckBox


      For Each item As RepeaterItem In rptEmployeeTypes.Items
        employeeTypeCheckBox = item.FindControl("cbEmployeeTypes")
        If (employeeTypeCheckBox.Checked = True) Then
          'find out which employee types the user has checked and build the string 
          'to send to the stored proc
          Select Case employeeTypeCheckBox.Text
            Case "Classified"
              strSelectedEmployeeType += "C,"
            Case "Exempt"
              strSelectedEmployeeType += "E,"
            Case "Ft Faculty"
              strSelectedEmployeeType += "F,"
            Case "Student"
              strSelectedEmployeeType += "S,"
            Case "Hourly"
              strSelectedEmployeeType += "H,"
          End Select
        End If

      Next

    Catch ex As Exception

    End Try


    If IsValidSearch() Then
      If strDisplayString = "" Then 'valid, but no user selected
        pnlTimesheetSearchResults.Visible = True
        rptTimesheetSearchResults.DataSource = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_Search_Timesheet", _
                                                                       New SqlParameter("@BeginDate", IIf(clsGeneric.IsValidSQLDate(txtStartDate.Text), txtStartDate.Text, DBNull.Value)), _
                                                                       New SqlParameter("@EndDate", IIf(clsGeneric.IsValidSQLDate(txtEndDate.Text), txtEndDate.Text, DBNull.Value)), _
                                                                       New SqlParameter("@StatusesArray", strSelectedStatuses), _
                                                                       New SqlParameter("@SID", DBNull.Value), _
                                                                       New SqlParameter("@IsSupervisor", clsSession.userIsSupervisor), _
                                                                       New SqlParameter("@IsFinAidAdmin", clsSession.userIsFinAidAdmin), _
                                                                       New SqlParameter("@IsPayrollAdmin", clsSession.userIsPayrollAdmin), _
                                                                       New SqlParameter("@ActiveEmployeesOnly", chkActiveEmployeesOnly.Checked), _
                                                                       New SqlParameter("@EmployeeTypesArray", strSelectedEmployeeType), _
                                                                       New SqlParameter("@SearcherSID", clsSession.userSID), _
                                                                       New SqlParameter("@IsFinanceAdmin", clsSession.userIsFinanceAdmin), _
                                                                       New SqlParameter("@IsHRAdmin", clsSession.userIsHRAdmin), _
                                                                       New SqlParameter("@GrantBudgetAPPR", My.Settings.GrantBudgetAPPR), _
                                                                       New SqlParameter("@TypesArray", IIf(liType.Visible, strSelectedTypes, DBNull.Value)))
        rptTimesheetSearchResults.DataBind()

        If rptTimesheetSearchResults.Items.Count = 0 Then
          pNoSearchResults.Visible = True
          rptTimesheetSearchResults.Visible = False
        Else
          pNoSearchResults.Visible = False
          rptTimesheetSearchResults.Visible = True
        End If
      Else
        Dim dsUserSearchResults As DataSet = clsGeneric.SearchUsers_Dataset(strDisplayString, chkActiveEmployeesOnly.Checked)

        If dsUserSearchResults.Tables(0).Rows.Count = 1 Then
          pnlTimesheetSearchResults.Visible = True

          rptTimesheetSearchResults.DataSource = SqlHelper.ExecuteDataset(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_Search_Timesheet", _
                                                                         New SqlParameter("@BeginDate", IIf(clsGeneric.IsValidSQLDate(txtStartDate.Text), txtStartDate.Text, DBNull.Value)), _
                                                                         New SqlParameter("@EndDate", IIf(clsGeneric.IsValidSQLDate(txtEndDate.Text), txtEndDate.Text, DBNull.Value)), _
                                                                         New SqlParameter("@StatusesArray", strSelectedStatuses), _
                                                                         New SqlParameter("@SID", dsUserSearchResults.Tables(0).Rows(0).Item("SID")), _
                                                                         New SqlParameter("@IsSupervisor", clsSession.userIsSupervisor), _
                                                                         New SqlParameter("@IsFinAidAdmin", clsSession.userIsFinAidAdmin), _
                                                                         New SqlParameter("@IsPayrollAdmin", clsSession.userIsPayrollAdmin), _
                                                                         New SqlParameter("@ActiveEmployeesOnly", chkActiveEmployeesOnly.Checked), _
                                                                         New SqlParameter("@EmployeeTypesArray", strSelectedEmployeeType), _
                                                                         New SqlParameter("@SearcherSID", clsSession.userSID), _
                                                                         New SqlParameter("@IsFinanceAdmin", clsSession.userIsFinanceAdmin), _
                                                                         New SqlParameter("@IsHRAdmin", clsSession.userIsHRAdmin), _
                                                                         New SqlParameter("@GrantBudgetAPPR", My.Settings.GrantBudgetAPPR), _
                                                                         New SqlParameter("@TypesArray", IIf(liType.Visible, strSelectedTypes, DBNull.Value)))
          rptTimesheetSearchResults.DataBind()

          If rptTimesheetSearchResults.Items.Count = 0 Then
            pNoSearchResults.Visible = True
            rptTimesheetSearchResults.Visible = False
          Else
            pNoSearchResults.Visible = False
            rptTimesheetSearchResults.Visible = True
          End If
        ElseIf dsUserSearchResults.Tables(0).Rows.Count = 0 Then
          pnlTimesheetSearchResults.Visible = True
          pNoSearchResults.Visible = True
          rptTimesheetSearchResults.Visible = False
        Else
          rptUserSearchResults.Visible = True
          rptUserSearchResults.DataSource = dsUserSearchResults
          rptUserSearchResults.DataBind()
        End If
      End If
    End If
    End Sub

    Private Function IsValidSearch() As Boolean
        Dim blnValidSearch As Boolean = True
        Dim blnTypeSelected As Boolean = False
        Dim blnStatusSelected As Boolean = False

        For Each li As ListItem In cblStatuses.Items
            If li.Selected Then
                blnStatusSelected = True
                Exit For
            End If
        Next

        If Not blnStatusSelected Then
            blnValidSearch = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_SEARCHTIMESHEETS_StatusNeeded)
        End If


        If liType.Visible Then
            For Each li As ListItem In cblTypes.Items
                If li.Selected Then
                    blnTypeSelected = True
                    Exit For
                End If
            Next

            If Not blnTypeSelected Then
                blnValidSearch = False
                uclFeedback.DisplayError(Resources.GlobalText.Error_SEARCHTIMESHEETS_TypeNeeded)
            End If
        End If


        If blnValidSearch AndAlso clsGeneric.IsValidSQLDate(txtStartDate.Text) AndAlso clsGeneric.IsValidSQLDate(txtEndDate.Text) Then
            If CDate(txtStartDate.Text) > CDate(txtEndDate.Text) Then
                blnValidSearch = False
                uclFeedback.DisplayError(Resources.GlobalText.Error_SEARCHTIMESHEETS_EndBeforeStart)
            End If
        End If



        If blnValidSearch AndAlso txtUserSearch.Text = "" AndAlso ((Not clsGeneric.IsValidSQLDate(txtStartDate.Text)) And (Not clsGeneric.IsValidSQLDate(txtEndDate.Text))) Then
            blnValidSearch = False
            uclFeedback.DisplayError(Resources.GlobalText.Error_SEARCHTIMESHEETS_DateNeededToSearch)
        End If

        Return blnValidSearch
    End Function

    Protected Sub SearchSpecificUser(ByVal sender As Object, ByVal e As System.EventArgs)
        SearchTimesheets(IIf(DirectCast(sender, Button).CommandArgument = "", txtUserSearch.Text, DirectCast(sender, Button).CommandArgument))
    End Sub



  Private Sub rptEmployeeTypes_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles rptEmployeeTypes.ItemDataBound


    Dim MyCheckBox As CheckBox

    Dim MyEmployeeTypeID As String

    Try

    
      MyCheckBox = CType(e.Item.FindControl("cbEmployeeTypes"), CheckBox)
      MyEmployeeTypeID = e.Item.DataItem("Title") 'EmployeeTypeID
      MyCheckBox.Text = MyEmployeeTypeID
      MyCheckBox.Checked = True
      MyCheckBox.Attributes.Add("title", e.Item.DataItem("Title"))

    Catch ex As Exception

    End Try



  End Sub
End Class