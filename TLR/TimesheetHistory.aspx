<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="TimesheetHistory.aspx.vb" Inherits="TLR.TimesheetHistory" Title="<%$ Resources:GlobalText, PageTitle_TIMESHEETHISTORY %>" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="TLR" %>

<asp:Content ID="chpBody" ContentPlaceHolderID="cphBody" runat="server">
	<TLR:ToolkitScriptManager ID="ToolkitScriptManager" runat="server" />

<div id="box">
	
	<ucl:Feedback id="uclFeedback" runat="server" />
	
<h1>My Timesheet History</h1>
<div class="box tsarchive">
    <ol>
    <li>
        <label for="<%=txtStartDate.ClientID%>">Start Date:</label>
        <span>
			<asp:TextBox ID="txtStartDate" CssClass="Form_Field" Width="80" autocomplete="off" MaxLength="10" runat="Server" />
			<asp:ImageButton ID="imgCalStartDate" runat="server" AlternateText="Click to select start date" ImageUrl="~/images/Control_MonthCalendar.gif"/>
			<TLR:CalendarExtender 
			TargetControlID="txtStartDate"
			ID="calStartDate" 
			PopupButtonID="imgCalStartDate"
			runat="server"/>
			(m/d/yyyy)
        </span>
    </li>
    <li>
        <label for="<%=txtEndDate.ClientID%>">End Date:</label>
        <span>
		    <asp:TextBox ID="txtEndDate" CssClass="Form_Field" Width="80" AutoComplete="off" MaxLength="10" runat="SERVER" />
		    <asp:ImageButton ID="imgEndDate" runat="server" AlternateText="Click to select end date" ImageUrl="~/images/Control_MonthCalendar.gif"/>
		    <TLR:CalendarExtender  CssClass=""
		    TargetControlID="txtEndDate"
		    ID="calEndDate" 
		    PopupButtonID="imgEndDate"
		    runat="server"/> 
		     (m/d/yyyy)
		 </span>
    </li>
    <li>
        <span class="search">
        <asp:Button ID="btnLookup" runat="server" CssClass="button" Text="Search" />
        </span>
    </li>
    </ol>
</div>
   <p id="pMessage" class="rpt_message" runat="server" visible="false" />
   <asp:Repeater ID="rptTimesheets" Visible="false" runat="server">
   <HeaderTemplate>
   <table class="timesheetlist">
       <thead>
            <tr>
                <th class="pay_period">Pay Period</th>
                <th>Title</th>
                <th>Department</th>
                <th>Supervisor</th>
                <th>Pay Rate</th>
                <th>Status</th>
            </tr>
       </thead>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><a href='Timesheet.aspx?TimeSheetID=<%#DataBinder.Eval(Container, "DataItem.TimeSheetID")%>'><%#DataBinder.Eval(Container, "DataItem.BeginDate")%> - <%#DataBinder.Eval(Container, "DataItem.EndDate")%></a></td>
        <td><%#DataBinder.Eval(Container, "DataItem.JobClassNameShort")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.JobSupervisorDisplayName")%></td>
        <td><%#IIf(DataBinder.Eval(Container, "DataItem.PayRate") <> 0, String.Format("{0:C}", CDbl(Container.DataItem("PayRate"))), "<abbr title='Not Applicable'>N/A</abbr>")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.StatusName")%></td>
    </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater>  
</div>
<%--<div style="color:#FFFFFF;">Note - this page was served from '<%response.write(System.Environment.MachineName)%>'</div>--%>
</asp:Content>




