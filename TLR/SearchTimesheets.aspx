<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="SearchTimesheets.aspx.vb" Inherits="TLR.SearchTimesheets" Title="<%$ Resources:GlobalText, PageTitle_SEARCHTIMESHEETS %>" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="TLR" %>

<asp:Content ID="chpBody" ContentPlaceHolderID="cphBody" runat="server">
	<TLR:ToolkitScriptManager ID="ToolkitScriptManager" EnablePageMethods="true" runat="server" />

<div class="box timesheet_search">	

<ucl:Feedback id="uclFeedback" runat="server" />
<h1>Search Timesheets</h1>

<asp:Panel ID="pnlSearch" DefaultButton="btnSearch" runat="server">
    <ol>
    <li>
        <label for="<%=txtUserSearch.ClientID%>">Name/SID:</label>
        <span>
			<asp:TextBox ID="txtUserSearch" CssClass="Form_Field" width="500" runat="Server" AutoComplete="off" />
			<TLR:AutoCompleteExtender 
			    ID="aceNameOrSID" 
			    CompletionListItemCssClass="AutoExtenderList"  
			    CompletionListHighlightedItemCssClass="AutoExtenderHighlight"
			    CompletionListCssClass="AutoExtender"
			    runat="server" 
			    TargetControlID="txtUserSearch" 
			    MinimumPrefixLength="3" 
			    servicemethod="SearchUsers" 
			    CompletionInterval="300" 
			    enablecaching="true" 
			    CompletionSetCount="10"
			    />
        </span>
    </li>    
    <li class="active_employees">
        <span>
            <asp:CheckBox ID="chkActiveEmployeesOnly" runat="server" Text="" Checked="true" />
        </span>
        <label for="<%=chkActiveEmployeesOnly.ClientID%>">Search active employees only</label>
    </li>   
    <li>
        <label for="rptEmployeeTypeList">Employee Type:</label>
        

        
        <asp:Repeater ID="rptEmployeeTypes" runat="server">
         <HeaderTemplate>
          <ul class="employeeType" id="rptEmployeeTypeList">
         </HeaderTemplate>
         <ItemTemplate>
          <li>
              <!--label for="cbEmployeeTypes_<%#DataBinder.Eval(Container, "DataItem.EmployeeTypeID")%>" title="<%#DataBinder.Eval(Container, "DataItem.Title")%>"><%#DataBinder.Eval(Container, "DataItem.EmployeeTypeID")%></label-->
              <!--input id="cbEmployeeTypes_<%#DataBinder.Eval(Container, "DataItem.EmployeeTypeID")%>" value="<%#DataBinder.Eval(Container, "DataItem.EmployeeTypeID")%>" type="checkbox" checked="checked" name="cblEmployeeTypes_1" title="<%#DataBinder.Eval(Container, "DataItem.Title")%>" /-->
              <asp:CheckBox ID="cbEmployeeTypes" runat="server" Text="A" AutoPostBack="true" /> 
           </li>
          
         </ItemTemplate>
         <FooterTemplate>
        </ul>
         </FooterTemplate>
        </asp:Repeater>
        
        
        
        
        
        
        
    </li>    
    <li>
        <label for="<%=txtStartDate.ClientID%>">Start Date:</label>
        <span>
			<asp:TextBox ID="txtStartDate" CssClass="Form_Field" Width="80" autocomplete="off" MaxLength="10" runat="Server" />
			<asp:ImageButton ID="imgCalStartDate" runat="server" AlternateText="Click to select start date" ImageUrl="~/images/Control_MonthCalendar.gif"/>
			<TLR:CalendarExtender TargetControlID="txtStartDate" ID="calStartDate" PopupButtonID="imgCalStartDate" runat="server"/> (m/d/yyyy)
        </span>
    </li>
    <li>
        <label for="<%=txtEndDate.ClientID%>">End Date:</label>
        <span>
		    <asp:TextBox ID="txtEndDate" CssClass="Form_Field" Width="80" AutoComplete="off" MaxLength="10" runat="SERVER" />
		    <asp:ImageButton ID="imgEndDate" runat="server" AlternateText="Click to select end date" ImageUrl="~/images/Control_MonthCalendar.gif"/>
		    <TLR:CalendarExtender  CssClass="" TargetControlID="txtEndDate" ID="calEndDate" PopupButtonID="imgEndDate" runat="server"/> (m/d/yyyy)
		 </span>
    </li>
    <li id="liType" runat="server" visible="false">
        <label for="<%=cblStatuses.ClientID%>">Type:</label>
        <span class="statuses">
            <asp:CheckBoxList ID="cblTypes" runat="server" DataTextField="TypeName" DataValueField="TimesheetTypeID" RepeatLayout="table" RepeatColumns="1" role="presentation" />
        </span>
    </li>     
    <li>
        <label for="<%=cblStatuses.ClientID%>">Status:</label>
        <span class="statuses">
            <asp:CheckBoxList ID="cblStatuses" runat="server" DataTextField="StatusName" DataValueField="TimesheetStatusID" RepeatLayout="table" RepeatColumns="1" role="presentation" />
        </span>
    </li>    
    <li style="clear: both;">
        <span class="action">
        <asp:Button ID="btnSearch" runat="server" CssClass="button" Text="Search" OnClick='SearchSpecificUser' CommandArgument="" />
        </span>
    </li>
    </ol>
</asp:Panel>    
</div>

<asp:Panel ID="pnlTimesheetSearchResults" runat="server" Visible="false">
   <h2>Search Results:</h2>
   <p id="pNoSearchResults" Visible="false" runat="server">No timesheets matched your search.</p>
   <asp:Repeater ID="rptTimesheetSearchResults" runat="server" EnableViewState="true">
   <HeaderTemplate>
   <table class="timesheetlist">
       <thead>
            <tr>
                <th>Pay Period</th>
                <th>Name</th>
                <th>Title</th>
                <th>Department</th>
                <th>Status</th>
                <th>Type</th>
        
       
               <%#IIf(TLR.clsSession.userIsPayrollAdmin = True, "<th>Auto Processed</th>", "")%>
            </tr>
        </thead>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td><a href='Timesheet.aspx?TimeSheetID=<%#DataBinder.Eval(Container, "DataItem.TimeSheetID")%>'><%#DataBinder.Eval(Container, "DataItem.BeginDate")%> - <%#DataBinder.Eval(Container, "DataItem.EndDate")%></a></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DisplayName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.JobClassNameShort")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.StatusName")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.EmployeeTypeID")%></td>     
        <%#IIf(TLR.clsSession.userIsPayrollAdmin = True, "<td>" & DataBinder.Eval(Container, "DataItem.AutoProcessed") & "</td>", "")%>
        
        
    </tr>
   </ItemTemplate>
   <FooterTemplate>
   </table>
   </FooterTemplate>
   </asp:Repeater>   
</asp:Panel>


<div class="person_list">
<asp:Repeater ID="rptUserSearchResults" Visible="false" runat="server">
<HeaderTemplate>
<h2>Please choose a person from the list</h2>
<table class="tbl">
    <thead>
        <tr>
            <th>Name</th>
            <th>SID</th>
            <th>Department</th>
        </tr>
    </thead>
</HeaderTemplate>
<ItemTemplate>
<tr>
    <td><asp:button ID="btnSearchUser" CssClass="link_button" runat="server" text='<%#DataBinder.Eval(Container, "DataItem.DisplayName")%>' OnClick='SearchSpecificUser' CommandArgument='<%#Container.DataItem("SearchString")%>'  /></td>
    <td><%#DataBinder.Eval(Container, "DataItem.SID")%></td>
    <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>
</tr>
</ItemTemplate>
<FooterTemplate>
</table>
</FooterTemplate>
</asp:Repeater>
</div>
   
   
   
</asp:Content>
