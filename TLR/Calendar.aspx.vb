Imports System.Data.SqlClient

Partial Public Class Calendar
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        uclFeedback.ResetDisplay()

        If Not Me.IsPostBack Then
            clsGeneric.RedirectIfNotPayrollAdmin()
            ddlYears.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_PayrollCalendar_Years")
            ddlYears.DataBind()

            LoadEntries(Now.Year.ToString)
            Try
                ddlYears.SelectedValue = Now.Year.ToString
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub btnSelectYear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSelectYear.Click
        LoadEntries()
    End Sub

    Private Sub LoadEntries(Optional ByVal strYear As String = "")
        rptPayrollCalendar.DataSource = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_PayrollCalendar_SpecialDays", _
                                                                New SqlParameter("@Year", IIf(strYear = "", ddlYears.SelectedItem.Value, strYear)))
        rptPayrollCalendar.DataBind()
    End Sub

    Protected Sub DeleteEntry(ByVal sender As Object, ByVal e As System.EventArgs)
        SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_PayrollCalendar", _
                                  New SqlParameter("@OneDay", DirectCast(sender, Button).CommandArgument), _
                                  New SqlParameter("@SpecialDay", False), _
                                  New SqlParameter("@Description", DBNull.Value))
        LoadEntries()
    End Sub

    Private Sub btnAddHoliday_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddHoliday.Click
        If clsGeneric.IsValidSQLDate(txtDate.Text) Then
            SqlHelper.ExecuteNonQuery(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_UPDATE_PayrollCalendar", _
                                      New SqlParameter("@OneDay", txtDate.Text), _
                                      New SqlParameter("@SpecialDay", True), _
                                      New SqlParameter("@Description", Server.HtmlEncode(txtDescription.Text)))

            ddlYears.SelectedValue = CDate(txtDate.Text).Year.ToString
            LoadEntries(CDate(txtDate.Text).Year.ToString)
            txtDate.Text = ""
            txtDescription.Text = ""
        Else
            uclFeedback.DisplayError(Resources.GlobalText.Error_CALENDAR_InvalidDate)
        End If
    End Sub

    Private Sub Calendar_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        SetFocus(txtDate)
    End Sub
End Class