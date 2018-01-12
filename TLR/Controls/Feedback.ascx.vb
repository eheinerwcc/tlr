Partial Public Class Feedback
    Inherits System.Web.UI.UserControl

    '=====> Use to display messages whether it is success or an error
    Public Sub DisplayError(ByVal strMessage As String)
        pnlFeedback.CssClass = "panel_feedback_error"
        lblFeedback.Text = "<strong>Error</strong>"
        lblFeedback.Text += ": " & strMessage
        lblFeedback.Visible = True
        pnlFeedback.Visible = True
    End Sub

    Public Sub DisplayNote(ByVal strMessage As String)
        pnlFeedback.CssClass = "panel_feedback_note"
        lblFeedback.Text = "<strong>Note</strong>"
        lblFeedback.Text += ": " & strMessage
        lblFeedback.Visible = True
        pnlFeedback.Visible = True
    End Sub

    Public Sub DisplaySuccess(ByVal strMessage As String)
        pnlFeedback.CssClass = "panel_feedback_success"
        lblFeedback.Text = "<strong>Success</strong>"
        lblFeedback.Text += ": " & strMessage
        lblFeedback.Visible = True
        pnlFeedback.Visible = True
    End Sub

    Public Sub AddToDisplayMessage(ByRef pnlFeedback As Panel, ByRef lblFeedback As Label, ByVal strMessage As String)
        lblFeedback.Text += strMessage
        lblFeedback.Visible = True
        pnlFeedback.Visible = True
    End Sub

    Public Sub ResetDisplay()
        lblFeedback.Text = ""
        lblFeedback.Visible = False
        pnlFeedback.Visible = False
    End Sub
End Class