Imports System.Web.SessionState

Public Class Global_asax
  Inherits System.Web.HttpApplication

  Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
    ' Fires when the application is started
  End Sub

  Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
    ' Fires when the session is started
  End Sub

  Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)



    If ConfigurationManager.AppSettings.Get("AutomaticallyRedirectToSecureChannel") AndAlso (Not Request.IsSecureConnection) Then
      Dim redirectURL As New StringBuilder(Request.Url.AbsoluteUri)
      redirectURL.Replace("http:", "https:")
      redirectURL.Replace("/Default.aspx", "")
      Response.Redirect(redirectURL.ToString())
    End If
  End Sub

  Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
    ' Fires upon attempting to authenticate the use
  End Sub

  Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
    Try
      clsErrorHandler.RecordError(Context.Error.GetBaseException())
      Server.Transfer("~/Error.aspx")
    Catch ex As Exception
      If Context.Error.GetBaseException.GetType.Name = "HttpException" AndAlso DirectCast(Context.Error.GetBaseException, HttpException).GetHttpCode = "404" Then
        Response.Redirect("~/default.aspx")
      Else
        Server.Transfer("~/Error.aspx")
      End If
    End Try
  End Sub

  Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
    ' Fires when the session ends
  End Sub

  Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
    ' Fires when the application ends
  End Sub

End Class