<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="ApproveTimesheets.aspx.vb" Inherits="TLR.ApproveTimesheets" Title="<%$ Resources:GlobalText, PageTitle_APPROVETIMESHEETS %>" %>
<asp:Content ID="chpBody" ContentPlaceHolderID="cphBody" runat="server">
<div id="approval" class="page">
<h1>Approve Timesheets</h1>

<asp:Panel ID="pnlPrimaryApproval" runat="server">
   <h2>Primary Approval:</h2>
   <p id="pNoTimesheets_Primary" Visible="false" runat="server">You have no timesheets pending approval.</p>
   <asp:Repeater ID="rptPrimaryApproval" runat="server">
   <HeaderTemplate>
   <table class="timesheetlist">
    <tr>
        <th>Pay Period</th>
        <th>Name</th>
        <th>Title</th>
        <th>Department</th>
        <th>Status</th>
    </tr>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><a href='Timesheet.aspx?TimeSheetID=<%#DataBinder.Eval(Container, "DataItem.TimeSheetID")%>'><%#DataBinder.Eval(Container, "DataItem.BeginDate")%> - <%#DataBinder.Eval(Container, "DataItem.EndDate")%></a></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DisplayName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.JobClassNameShort")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.StatusName")%></td>
    </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater>   


</asp:Panel>

<asp:Panel ID="pnlAlternateApproval" runat="server">
   <h2>Alternate Approval:</h2>
   <p id="pNoTimesheets_Alternate" Visible="false" runat="server">You have no timesheets pending approval.</p>
   <asp:Repeater ID="rptAlternateApproval" runat="server">
   <HeaderTemplate>
   <table class="timesheetlist">
    <tr>
         <th>Pay Period</th>
        <th>Name</th>
        <th>Title</th>
        <th>Department</th>
        <th>Status</th>
    </tr>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><a href='Timesheet.aspx?TimeSheetID=<%#DataBinder.Eval(Container, "DataItem.TimeSheetID")%>'><%#DataBinder.Eval(Container, "DataItem.BeginDate")%> - <%#DataBinder.Eval(Container, "DataItem.EndDate")%></a></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DisplayName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.JobClassNameShort")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.StatusName")%></td>
    </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater>   


</asp:Panel>

</div>

</asp:Content>
