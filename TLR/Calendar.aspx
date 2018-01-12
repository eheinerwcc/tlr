<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="Calendar.aspx.vb" Inherits="TLR.Calendar" Title="<%$ Resources:GlobalText, PageTitle_CALENDAR %>" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="TLR" %>


<asp:Content ID="chpBody" ContentPlaceHolderID="cphBody" runat="server">

<div>


<TLR:ToolkitScriptManager ID="ToolkitScriptManager" runat="server" />
<ucl:Feedback id="uclFeedback" runat="server" />

<asp:Panel ID="pnlIsPayroll" runat="server">

	<h1>Payroll Calendar Exceptions</h1>
	<p>The following days for this year will be highlighted for part-time employees:</p>


<div class="box specialDayEntry">
<h2>Add Calendar Exception</h2>
	<ol>
		<li>
			<label for="<%=txtDate.ClientID %>">Date:</label>
			<span>
				<asp:TextBox ID="txtDate" runat="server" />
				<asp:ImageButton ID="imgCalendarBtn" runat="server" AlternateText="Click to select date" ImageUrl="~/images/Control_MonthCalendar.gif"/> (m/d/yyyy)
				<TLR:CalendarExtender TargetControlID="txtDate" ID="calAddDate" runat="server" PopupButtonID="imgCalendarBtn" PopupPosition="BottomLeft"/>
			</span>
		</li>
		<li>
			<label for="<%=txtDescription.ClientID %>">Name:</label>
			<span><asp:TextBox ID="txtDescription" runat="server" MaxLength="40" /></span>
		</li>
		<li>
			<span class="action">
				<asp:Button ID="btnAddHoliday" runat="server" Text="Add Holiday" CssClass="button"/>
			</span>
		</li>
	</ol>
</div>	
	


<div class="box specialDays">
<h2>Special Days</h2>

<ol>
	<li>
		<label for="<%=ddlYears.ClientID %>">Select Year:</label>
		<span>
			<asp:DropDownList CssClass="field" ID="ddlYears" runat="server" DataTextField="YearNumber" DataValueField="YearNumber" />
			<asp:Button ID="btnSelectYear" Text="Select Year" runat="server" CssClass="button"/>             
		</span>
	</li>
</ol>
</div>

<div class="box">
<asp:Repeater ID="rptPayrollCalendar" runat="server">
	<HeaderTemplate>
		<table class="tbl">
		<tr>
			<th>Date</th>
			<th>Name</th>
			<th>Action</th>
		</tr>
	</HeaderTemplate>
	<ItemTemplate>
			<tr>
				<td><%#Format(DataBinder.Eval(Container, "DataItem.OneDay"), "d")%> (<%#WeekdayName(DataBinder.Eval(Container, "DataItem.DayOfWeek"), True)%>.)</td>
				<td><%#DataBinder.Eval(Container, "DataItem.Description") %></td>
				<td>
				    <span onclick="javascript:return confirm('Are you sure you want to delete this entry?')">
					    <asp:Button ID="btnDelete" runat="server" Text="Delete"	onclick="DeleteEntry" CommandArgument='<%#DataBinder.Eval(Container, "DataItem.OneDay") %>' CssClass="button"/>
					</span>
				</td>
			</tr>
	</ItemTemplate>
	<FooterTemplate>
		</table>
	</FooterTemplate>
</asp:Repeater>	    
	    
</div>


	
</asp:Panel>   

	
	 
</div>

</asp:Content>
