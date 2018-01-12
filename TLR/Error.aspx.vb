Public Partial Class _Error
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        uclFeedback.DisplayError(Resources.GlobalText.Error_Error_GenericErrorMessage)
    End Sub

End Class