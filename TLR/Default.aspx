<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpPublic.Master" CodeBehind="Default.aspx.vb" Inherits="TLR.LogIn" Title="<%$ Resources:GlobalText, PageTitle_LOGIN %>" %>


<asp:Content ID="cphFeedback" ContentPlaceHolderID="cphFeedback" runat="server">
<ucl:Feedback id="uclFeedback" runat="server" />
</asp:Content>
<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" Runat="Server">


<div id="content"></div>
<div class="box login">
<h1>Login</h1>
    <ol>
        <li>
            <label for="<%=txtSID.ClientID%>" title="System Identifier">SID:</label>
            <span><asp:TextBox CssClass="field" ID="txtSID" MaxLength="9" runat="server" TextMode="Password" value=""/></span>
        </li>
        <li>
            <label for="<%=txtPIN.ClientID%>" title="Personal Identification Number">PIN:</label>
            <span><asp:TextBox CssClass="field" ID="txtPIN" MaxLength="6" runat="server" TextMode="Password" value="" /></span>
        </li>
        <li>
            <span class="action"><asp:Button CssClass="button" ID="btnLogin" Text="Log In" runat="server" /></span>
        </li>   
    </ol>

</div>

</asp:Content>