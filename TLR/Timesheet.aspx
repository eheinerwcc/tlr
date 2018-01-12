<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="Timesheet.aspx.vb" Inherits="TLR.Timesheet" Title="<%$ Resources:GlobalText, PageTitle_TIMESHEET %>" ViewStateEncryptionMode="Always" %>
<%@ Register src="Controls/TSFullTime.ascx" tagname="TSFullTime" tagprefix="ucl" %>
<%@ Register src="Controls/TSPartTime.ascx" tagname="TSPartTime" tagprefix="ucl" %>

<asp:Content ID="chpBody" ContentPlaceHolderID="cphBody" runat="server">

<ucl:Feedback ID="uclFeedback" runat="server" />
<div id="timesheet" class="page">
    <ucl:TSFullTime ID="uclTSFullTime" runat="server" Visible="false" />
    <ucl:TSPartTime ID="uclTSPartTime" runat="server" Visible="false" />

    
</div> 
</asp:Content>
