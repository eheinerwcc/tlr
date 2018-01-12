<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpPublic.Master" CodeBehind="Error.aspx.vb" Inherits="TLR._Error" Title="<%$ Resources:GlobalText, PageTitle_ERROR %>" %>

<asp:Content ID="cphFeedback" ContentPlaceHolderID="cphFeedback" runat="server">
    <ucl:Feedback id="uclFeedback" runat="server" />
</asp:Content>
<asp:Content ID="cphBod" ContentPlaceHolderID="cphBody" runat="server">

</asp:Content>
