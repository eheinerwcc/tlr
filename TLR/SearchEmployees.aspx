<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="SearchEmployees.aspx.vb" Inherits="TLR.SearchEmployees"Title="<%$ Resources:GlobalText, PageTitle_SEARCHEMPLOYEES %>" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="TLR" %>

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
	<TLR:ToolkitScriptManager ID="ToolkitScriptManager" EnablePageMethods="true" runat="server" />

    <ucl:Feedback id="uclFeedback" runat="server" />
    <h1 id="hPageHeader" runat="server">Search Employees</h1>
    <asp:Panel ID="pnlEmployeeLookup" runat="server" Visible="false">
       <p class="search_note">
       Click on an employee to see their Work Schedule, Current Leave Balances, and Timesheets. Note - only 
       classified and exempt employees will be shown.
       </p>
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
        
        <asp:Button ID="btnSearchUsers" runat="server" CssClass="button" Text="Search" />
        <p id="pNoSearchResults" Visible="false" runat="server">No employees matched your search.</p>
    </asp:Panel>
 
    <p id="pNoFulltimeEmployees" Visible="false" runat="server">This page is intended for those who supervise full-time employees.</p>
    <asp:Panel ID="pnlUserList" runat="server" Visible="false">
        <div class="person_list">
        <asp:Repeater ID="rptUserSearchResults" Visible="false" runat="server">
        <HeaderTemplate>
        <p class="search_note">
       Click on an employee to see their Work Schedule, Current Leave Balances, and Timesheets. Note - only 
       classified and exempt employees will be shown.
       </p>
        <h2>Please choose a person from the list</h2>
        <table class="tbl">
        <tr>
            <th>Name</th>
            <th>SID</th>
        </tr>
        </HeaderTemplate>
        <ItemTemplate>
        <tr>
            <td><asp:button ID="btnSearchUser" CssClass="link_button" runat="server" text='<%#DataBinder.Eval(Container, "DataItem.DisplayName")%>' OnClick='SelectUser' CommandArgument='<%#Container.DataItem("SID")%>'  /></td>
            <td><%#DataBinder.Eval(Container, "DataItem.SID")%></td>
        </tr>
        </ItemTemplate>
        <FooterTemplate>
        </table>
        </FooterTemplate>
        </asp:Repeater>
        </div>           
    </asp:Panel>

    <asp:Panel ID="pnlEmployeeDetails" class="employee_details" runat="server" Visible="false">
        <asp:Label ID="lblEmployeeName" CssClass="employee_name" runat="server" />
        <asp:Button ID="btnSelectNewEmployee" runat="server" Text="<< Select New Employee" CssClass="link_button" Visible="false" />
        <div class="box">
        
        <!-- Work Schedule -->
        <asp:panel ID="pnlWorkSchedule" CssClass="work_schedule" runat="server">
        <h2>Work Schedule (<asp:Label ID="lblWorkScheduleStatus" runat="server" />)</h2>
        <asp:Repeater ID="rptWorkSchedule" runat="server">
        <HeaderTemplate>
        <table class="tbl">
            <tr>
                <th id="day">Day</th>
                <th id="start">Start</th>
                <th id="end">End</th>
                <th id="break">Break</th>
            </tr>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td><%#System.Enum.GetValues(GetType(DayOfWeek))(Container.DataItem("DayOfWeek") - 1).ToString%></td>
                <td><%#Format(Container.DataItem("StartTime"), "t")%></td>
                <td><%#Format(Container.DataItem("EndTime"), "t")%></td>
                <td><%#Container.DataItem("MealTime")%> min.</td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
        </table>
        </FooterTemplate>
        </asp:Repeater>    
        </asp:panel>

        <!-- Balances -->
        <asp:Panel ID="pnlLeaveBalances" runat="server" Visible="false">
        <h2>Current Leave Balances</h2>
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
        </asp:Panel>
        
        </div>
    
        <!-- Timesheets -->
        <h2>Timesheets</h2>
        <asp:DropDownList ID="ddlViewPeriod" runat="server">
            <asp:ListItem Text="Last 6 Months" Value="6" />    
            <asp:ListItem Text="Last Year" Value="12" />
            <asp:ListItem Text="Last 3 Years" Value="36" />
            <asp:ListItem Text="All" Value="-1" />
        </asp:DropDownList>
        <asp:Button id="btnViewPeriod" runat="server" CssClass="button" text="View" />
        
        <asp:Repeater ID="rptTimesheets" runat="server">
            <HeaderTemplate>
                <table class="timesheetlist">
                    <tr>
                        <th class="pay_period">Pay Period</th>
                        <th>Title</th>
                        <th>Department</th>
                        <th>Supervisor</th>
                        <th>Status</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><a href='Timesheet.aspx?TimeSheetID=<%#DataBinder.Eval(Container, "DataItem.TimeSheetID")%>'><%#DataBinder.Eval(Container, "DataItem.BeginDate")%> - <%#DataBinder.Eval(Container, "DataItem.EndDate")%></a></td>
                    <td><%#DataBinder.Eval(Container, "DataItem.JobClassNameShort")%></td>
                    <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>
                    <td><%#DataBinder.Eval(Container, "DataItem.JobSupervisorDisplayName")%></td>
                    <td><%#DataBinder.Eval(Container, "DataItem.StatusName")%></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>

    </asp:Panel>
</asp:Content>
