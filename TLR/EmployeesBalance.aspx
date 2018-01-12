<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="EmployeesBalance.aspx.vb" Inherits="TLR.EmployeesBalance" Title="<%$ Resources:GlobalText, PageTitle_EMPLOYEESBALANCE %>" %>
<asp:Content ID="chpBody" ContentPlaceHolderID="cphBody" runat="server">
<div>
<ucl:Feedback id="uclFeedback" runat="server" />
<h1 class="PanelTitle">My Employees' Leave Balance</h1>

  <asp:Repeater ID="rptLeaveBalance" runat="server">
   <HeaderTemplate>
   <table class="tbl employeesbalance">
    <tr>
        <th>Name</th>
        <th><abbr class="leavecodeabbr" title="Vacation (and accrual rate)">V (Accrue Rate)</abbr></th>
        <th><abbr title="Sick leave (and accrual rate)">S (Accrue Rate)</abbr></th>
        <th><abbr title="Compensatory time">C</abbr></th>
        <th><abbr title="Personal holiday">P</abbr></th>
        <th><abbr title="Personal leave">X</abbr></th>
        <th><abbr title="Non-compensable sick leave">N</abbr></th>
        <th><abbr title="Hourly sick leave">HL</abbr></th>
        <th><abbr title="Student sick leave">SL</abbr></th>
        <th><abbr title="Leave expiration month">Expir. Month</abbr></th>
        <th><abbr title="Employment anniversary date">Anniv. Date</abbr></th>
    </tr>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><%#DataBinder.Eval(Container, "DataItem.DisplayName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.VAC")%> (<span class="accrue">+<%#DataBinder.Eval(Container, "DataItem.VACAccrueRate")%></span>)</td>
        <td><%#DataBinder.Eval(Container, "DataItem.CSL")%> (<span class="accrue">+<%#DataBinder.Eval(Container, "DataItem.CSLAccrueRate")%></span>)</td>
        <td><%#DataBinder.Eval(Container, "DataItem.CMP")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.PH")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.PRL")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.NSL")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.HSL")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.SSL")%></td>
        <td><%#MonthName(DataBinder.Eval(Container, "DataItem.LeaveExpirMonth"))%></td>
        <td><abbr title='<%#MonthName(Month((DataBinder.Eval(Container, "DataItem.AnniversaryDate"))))%> <%#Day(DataBinder.Eval(Container, "DataItem.AnniversaryDate"))%>'><%#Format(DataBinder.Eval(Container, "DataItem.AnniversaryDate"), "MMM")%> <%#Day(DataBinder.Eval(Container, "DataItem.AnniversaryDate"))%></abbr></td>
    </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater> 
</div>
</asp:Content>
