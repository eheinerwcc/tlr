<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="UnprocessedTimesheets.aspx.vb" Inherits="TLR.UnprocessedTimesheets" Title="<%$ Resources:GlobalText, PageTitle_UNPROCESSEDTIMESHEETS %>" %>

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
<ucl:Feedback id="uclFeedback" runat="server" />
<h1><asp:Label ID="lblPageHeader" runat="server"/></h1>

<p id="pNoUnprocessedTimesheets" visible="false" runat="server">
    There are no unprocessed timesheets.
</p>

   <asp:Repeater ID="rptTimesheets" runat="server">
   <HeaderTemplate>
   <table class="timesheetlist">
       <thead>
            <tr>
                <th class="pay_period">Pay Period</th>
                <th>Name</th>
                <th>Title</th>
                <th>Department</th>
                <th>Supervisor</th>
                <th>Type</th>
            </tr>
       </thead>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><a href='Timesheet.aspx?TimeSheetID=<%#DataBinder.Eval(Container, "DataItem.TimeSheetID")%>'><%#DataBinder.Eval(Container, "DataItem.BeginDate")%> - <%#DataBinder.Eval(Container, "DataItem.EndDate")%></a></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DisplayName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.JobClassNameShort")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.JobSupervisorDisplayName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.EmployeeTypeID")%></td>
     </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater>  
</asp:Content>
