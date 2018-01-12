<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="TSActionLog.ascx.vb" Inherits="TLR.TSActionLog" %>
<div class="box ts_actionlog">
<h2>Timesheet Action Log</h2>
  <asp:Repeater ID="rptTimesheetActionLog" runat="server">
   <HeaderTemplate>
   <table class="tbl">
    <tr>
        <th id="date">Date</th>
        <th id="action">Action</th>
        <th id="actionby">Action By</th>
        <th id="comment">Comment</th>
    </tr>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><abbr class="date" title="<%#Format(Container.DataItem("ActionDate"), "MMMM d, yyyy h:m:s tt")%>"><%#Container.DataItem("ActionDate")%></abbr></td>
        <td><%#DataBinder.Eval(Container, "DataItem.ActionName")%></td>
       <td><%#DataBinder.Eval(Container, "DataItem.DisplayName")%></td>
       <td><%#DataBinder.Eval(Container, "DataItem.Comment")%></td>
    </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater> 
