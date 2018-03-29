<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="DelegatedTimesheets.aspx.vb" Inherits="TLR.DelegatedTimesheets" title="<%$ Resources:GlobalText, PageTitle_DELEGATEDTIMESHEETS %>" ViewStateEncryptionMode="Always" %>

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <ucl:Feedback id="uclFeedback" runat="server" />
    <h1>Create Timesheets (Via Delegated Employee Permission)</h1>



<asp:Panel ID="pnlActiveTimesheets" runat="server">
   <h2>"In Process" Timesheets:</h2>
   <asp:Label ID="lblNoActiveTimesheets" CssClass="MainText" Visible="false" runat="server">None of your employees who have delegated rights to you have active timesheets.</asp:Label>
   <asp:Repeater ID="rptActiveTimesheets" runat="server">
   <HeaderTemplate>
   <table class="tbl timesheetlist">
       <thead>
            <tr>
                <th>Employee</th>
                <th class="pay_period">Pay Period</th>
                <th>Job Title</th>
                <th>Department</th>
                <th class="pay_rate">Pay Rate</th>
            </tr>
       </thead>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><%#DataBinder.Eval(Container, "DataItem.DisplayName")%></td>
        <td><a href='Timesheet.aspx?TimeSheetID=<%#DataBinder.Eval(Container, "DataItem.TimeSheetID")%>'><%#DataBinder.Eval(Container, "DataItem.BeginDate")%> - <%#DataBinder.Eval(Container, "DataItem.EndDate")%></a></td>
        <td><%#DataBinder.Eval(Container, "DataItem.JobClassNameShort")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>        
        <td><%#IIf(DataBinder.Eval(Container, "DataItem.PayRate") <> 0, String.Format("{0:C}", CDbl(Container.DataItem("PayRate"))), "<abbr title='Not Applicable'>N/A</abbr>")%></td>
    </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater>   
</asp:Panel>





<asp:Panel ID="pnlCreateTimesheet" CssClass="create_timesheet panel" runat="server">
    <h2>Create New Timesheet:</h2>
    <asp:Repeater ID="rptJobList" runat="server">
    <HeaderTemplate>
    <table class="tbl joblist">
        <thead>
            <tr>
                <th class="job_number">#</th>
                <th>Employee</th>
                <th>Job Title</th>
                <th>Department</th>
                <th class="pay_rate">Pay Rate</th>
                <th>Pay Period</th>
            </tr>
        </thead>
    </HeaderTemplate>
    <ItemTemplate>
    <tr>
    <td>
        <input id="rdoJobNumber<%#Container.ItemIndex%>" type="radio" <asp:Literal ID="litJobChecked" runat="server" /> name="rdoJobNumber" value="<%#Container.ItemIndex%>" />
        <asp:Label ID="lblJobNumber" runat="server" Visible="False" Text='<%#Container.DataItem("JobNumber") %>' />
        <asp:Label ID="lblEmployeeSID" runat="server" Visible="false" Text='<%#Container.DataItem("SID") %>' />
    </td>
    <td><label for="rdoJobNumber<%#Container.ItemIndex%>"><%#Container.DataItem("DisplayName")%></label></td>
    <td><label for="rdoJobNumber<%#Container.ItemIndex%>"><%#Container.DataItem("TitleShort")%></label></td>
    <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>        
    <td><label for="rdoJobNumber<%#Container.ItemIndex%>"><%#IIf(DataBinder.Eval(Container, "DataItem.PayRate") <> 0, String.Format("{0:C}", CDbl(Container.DataItem("PayRate"))), "<abbr title='Not Applicable'>N/A</abbr>")%></label></td>
    <td><asp:DropDownList CssClass="field" ID="ddlPayPeriod" DataTextField="DisplayText" DataValueField="PayCycleID" runat="server" /></td>
    </tr>
    
        
    </ItemTemplate>
    <FooterTemplate>
    </table>
    </FooterTemplate>
    </asp:Repeater>
    
    <asp:Button ID="btnCreateTimeSheet" CssClass="button" Text="Create New Timesheet" runat="server" />
</asp:Panel>



</asp:Content>
