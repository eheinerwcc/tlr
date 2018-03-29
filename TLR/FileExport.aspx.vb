Imports System.Data.SqlClient

Partial Public Class FileExport
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        clsGeneric.RedirectOnSessionTimeout()
        uclFeedback.ResetDisplay()

        If Not Me.IsPostBack Then
            clsGeneric.RedirectIfNotPayrollAdmin()
            If Request.QueryString("ExportType") IsNot Nothing AndAlso (Request.QueryString("ExportType") = My.Settings.TimesheetTypeID_Time Or Request.QueryString("ExportType") = My.Settings.TimesheetTypeID_Leave) Then
                If Request.QueryString("ExportType") = My.Settings.TimesheetTypeID_Time Then
                    lblPageHeader.Text = "Export File - Time"
                    pTimeNote.Visible = True
                ElseIf Request.QueryString("ExportType") = My.Settings.TimesheetTypeID_Leave Then
                    lblPageHeader.Text = "Export File - Leave"
                    pLeaveNote.Visible = True
                Else
                    lblPageHeader.Visible = False
                End If
                BindControls()
            Else
                pnlFileExport.Visible = False
                pnlPreviousExports.Visible = False
                uclFeedback.DisplayError(Resources.GlobalText.Error_FileUpload_InvalidExportType)
            End If
        End If
    End Sub

    Private Sub BindControls()

        Using ppdt As DataTable = DBHelper.ExecuteReader(CommandType.StoredProcedure, "usp_SELECT_FileExportPayPeriod", _
                                                           New SqlParameter("@TimesheetTypeID", Request.QueryString("ExportType")))
            ddlPayPeriods.DataSource = ppdt
            ddlPayPeriods.DataBind()
        End Using

        If ddlPayPeriods.Items.Count = 0 Then
            pnlFileExport.Visible = False
            lblPageHeader.Visible = False
            uclFeedback.DisplayNote(Resources.GlobalText.Note_FileUpload_NoTimesheets)
        End If

        Using yrdt As DataTable = DBHelper.ExecuteReader(CommandType.StoredProcedure, "usp_SELECT_FileExportYears", _
                                                      New SqlParameter("@TimesheetTypeID", Request.QueryString("ExportType")))

            ddlYears.DataSource = yrdt
            ddlYears.DataBind()
        End Using

        pnlPreviousExports.Visible = ddlYears.Items.Count > 0

        BindPreviousExports()

        'bind other display controls based on the timesheetID Type
    End Sub

    Private Sub BindPreviousExports()
        If ddlYears.Items.Count > 0 Then
            Using edt As DataTable = DBHelper.ExecuteReader(CommandType.StoredProcedure, "usp_SELECT_FileList", _
                                                               New SqlParameter("@TimesheetTypeID", Request.QueryString("ExportType")), _
                                                               New SqlParameter("@YearNumber", ddlYears.SelectedValue))

                rptExportedFileByYear.DataSource = edt
                rptExportedFileByYear.DataBind()

            End Using
        End If
    End Sub

    Private Sub btnGenerateFile_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnGenerateFile.Click

        'Dim strUSPName As String = IIf(Request.QueryString("ExportType") = My.Settings.TimesheetTypeID_Leave, "usp_FileExport_LEAVE", "usp_FileExport_HOURLY")
        Dim strUSPName As String

        If Request.QueryString("ExportType") = My.Settings.TimesheetTypeID_Leave Then
            strUSPName = "usp_FileExport_LEAVE"
            generateLeaveData(strUSPName)
        Else
            strUSPName = "usp_FileExport_HOURLY"
            generateTimeData(strUSPName)
        End If

    End Sub

    Private Sub btnLoadPreviousExports_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnLoadPreviousExports.Click
        BindPreviousExports()
    End Sub

    Private Sub generateLeaveData(ByVal strUSPName As String)

        Dim sbUploadData As New StringBuilder

        Dim parFileName As New SqlParameter
        parFileName.ParameterName = "@FileName"
        parFileName.Direction = ParameterDirection.Output
        parFileName.DbType = DbType.String
        parFileName.Size = 50

        Try

            Dim ds As DataSet = DBHelper.ExecuteDataset(CommandType.StoredProcedure, strUSPName,
                                                         New SqlParameter("@SID", clsSession.userSID),
                                                         New SqlParameter("@BeginDate", ddlPayPeriods.SelectedValue),
                                                         parFileName)

            ' Check for empty Table collection and null Rows before processing
            If ds Is Nothing Or ds.Tables Is Nothing Or ds.Tables.Count <= 0 Or ds.Tables(0).Rows Is Nothing Then
                Throw New DataException(String.Format("Stored procedure '{0}' didn't return any records.", strUSPName))
            End If

            If ds.Tables(0).Rows.Count > 0 Then
                For Each dr As DataRow In ds.Tables(0).Rows

                    ' Check to make sure we have a valid value before we try to access it
                    If DBNull.Value.Equals(dr.Item("FileLine")) Then
                        Throw New DataException(String.Format("Unexpected NULL value found in first column of data. Does the database have a valid certificate?"))
                    End If

                    sbUploadData.AppendLine(dr.Item("FileLine"))
                Next

                'Find the release mode from appSettings in web.config
                Dim releaseMode As String
                releaseMode = ConfigurationManager.AppSettings.Get("Release_Mode")

                'and then set email notification addresses 
                If (Not releaseMode = "test") Then
                    My.Computer.FileSystem.WriteAllText(ConfigurationManager.AppSettings.Get("FileExportPath") + parFileName.Value.ToString, sbUploadData.ToString, False, Encoding.ASCII)
                End If

                BindControls()
                uclFeedback.DisplaySuccess(Resources.GlobalText.Success_FileUpload_UploadComplete + parFileName.Value.ToString)
            Else
                BindControls()
                uclFeedback.DisplayError(Resources.GlobalText.Error_FileUpload_EmptyUpload)
            End If
        Catch ex As Exception
            'Undo 'marked as processed'
            DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_FileExport_UNDO",
                                            New SqlParameter("@FileName", parFileName.Value.ToString))

            uclFeedback.DisplayError(Resources.GlobalText.Error_FileUpload_UploadError)

            clsErrorHandler.RecordError(ex)
        End Try
    End Sub

    Private Sub generateTimeData(ByVal strUSPName As String)
        'It's a time export, so generate the two time and leave files

        Dim sbUploadTimeData As New StringBuilder
        Dim sbUploadLeaveData As New StringBuilder

        Dim parFileNameTime As New SqlParameter
        parFileNameTime.ParameterName = "@FileNameTime"
        parFileNameTime.Direction = ParameterDirection.Output
        parFileNameTime.DbType = DbType.String
        parFileNameTime.Size = 50

        Dim parFileNameLeave As New SqlParameter
        parFileNameLeave.ParameterName = "@FileNameLeave"
        parFileNameLeave.Direction = ParameterDirection.Output
        parFileNameLeave.DbType = DbType.String
        parFileNameLeave.Size = 50

        Try

            Dim ds As DataSet = DBHelper.ExecuteDataset(CommandType.StoredProcedure, strUSPName,
                                                     New SqlParameter("@SID", clsSession.userSID),
                                                     New SqlParameter("@BeginDate", ddlPayPeriods.SelectedValue),
                                                     parFileNameTime, parFileNameLeave)

            'pTimeNote.InnerText = "Count: " + ds.Tables.Count.ToString + parFileNameTime.Value.ToString

            ' Check for empty Table collection and null Rows before processing
            If ds Is Nothing OrElse ds.Tables Is Nothing OrElse ds.Tables.Count <= 0 OrElse (ds.Tables(0) Is Nothing And ds.Tables(1) Is Nothing) OrElse (ds.Tables(0).Rows Is Nothing And ds.Tables(1).Rows Is Nothing) Then
                Throw New DataException(String.Format("Stored procedure '{0}' didn't return any records.", strUSPName))
            End If

            'Process time
            If ds.Tables(0).Rows.Count > 0 Then
                For Each dr As DataRow In ds.Tables(0).Rows

                    ' Check to make sure we have a valid value before we try to access it
                    If DBNull.Value.Equals(dr.Item("FileLine")) Then
                        Throw New DataException(String.Format("Unexpected NULL value found in first column of data. Does the database have a valid certificate?"))
                    End If

                    sbUploadTimeData.AppendLine(dr.Item("FileLine"))
                Next
            End If

            'Process leave
            If ds.Tables(1).Rows IsNot Nothing AndAlso ds.Tables(1).Rows.Count > 0 Then
                For Each dr As DataRow In ds.Tables(1).Rows

                    ' Check to make sure we have a valid value before we try to access it
                    If DBNull.Value.Equals(dr.Item("FileLine")) Then
                        Throw New DataException(String.Format("Unexpected NULL value found in first column of data. Does the database have a valid certificate?"))
                    End If

                    sbUploadLeaveData.AppendLine(dr.Item("FileLine"))
                Next
            End If

            'Find the release mode from appSettings in web.config
            Dim releaseMode As String
            releaseMode = ConfigurationManager.AppSettings.Get("Release_Mode")

            If sbUploadLeaveData.Length = 0 And sbUploadTimeData.Length = 0 Then
                BindControls()
                uclFeedback.DisplayError(Resources.GlobalText.Error_FileUpload_EmptyUpload)
            Else

                Dim filenames As String = ""

                If sbUploadTimeData.Length > 0 Then
                    filenames = parFileNameTime.Value.ToString
                    If (Not releaseMode = "test") Then
                        My.Computer.FileSystem.WriteAllText(ConfigurationManager.AppSettings.Get("FileExportPath") + parFileNameTime.Value.ToString, sbUploadTimeData.ToString, False, Encoding.ASCII)
                    End If
                End If

                If sbUploadLeaveData.Length > 0 Then
                    filenames += "," + parFileNameLeave.Value.ToString
                    If (Not releaseMode = "test") Then
                        My.Computer.FileSystem.WriteAllText(ConfigurationManager.AppSettings.Get("FileExportPath") + parFileNameLeave.Value.ToString, sbUploadLeaveData.ToString, False, Encoding.ASCII)
                    End If
                End If

                BindControls()
                uclFeedback.DisplaySuccess(Resources.GlobalText.Success_FileUpload_UploadComplete + " " + filenames)
            End If
        Catch ex As Exception
            'Undo 'marked as processed'
            'Doesn't matter which filename (time or leave) we pass here as the procedure will roll back both
            DBHelper.ExecuteNonQuery(CommandType.StoredProcedure, "usp_FileExport_UNDO",
                                           New SqlParameter("@FileName", parFileNameTime.Value.ToString))

            uclFeedback.DisplayError(Resources.GlobalText.Error_FileUpload_UploadError)

            clsErrorHandler.RecordError(ex)
        End Try
    End Sub
End Class