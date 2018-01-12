<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="LeaveBalance.aspx.vb" Inherits="TLR.LeaveBalance" Title="<%$ Resources:GlobalText, PageTitle_LEAVEBALANCE %>" %>
<%@ Import Namespace="TLR" %>

<asp:Content ID="chpBody" ContentPlaceHolderID="cphBody" runat="server">
<div id="home" class="page">
<ucl:Feedback id="uclFeedback" runat="server" />
<h1 class="PanelTitle">My Leave Balance</h1>

  <asp:Repeater ID="rptLeaveBalance" runat="server">
   <HeaderTemplate>
   <table class="tbl">
    <tr>
        <th id="leavetype">Leave Type</th>
        <th id="balance">Balance</th>
        <th id="accrualrate">Accrual Rate</th>
    </tr>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><%#DataBinder.Eval(Container, "DataItem.LeaveType")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.Balance")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.AccrueRate")%></td>
    </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater> 
   
   <div id="anniversary_date">
	<span class="title">Anniversary Month:</span>
	<%=clsGeneric.GetEmployeeAnniversaryDate(clsSession.userSID)%>
   </div>
</div>
</asp:Content>
