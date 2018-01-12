<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="UpdateTimesheet.aspx.vb" Inherits="TLR.UpdateTimesheet" title="<%$ Resources:GlobalText, PageTitle_UpdateTimesheet %>"%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="TLR" %>

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
	<TLR:ToolkitScriptManager ID="ToolkitScriptManager" EnablePageMethods="true" runat="server" />
    <ucl:Feedback id="uclFeedback" runat="server" />

<asp:Panel ID="pnlUpdateTimesheet" CssClass="box" runat="server">
    <h1>Timesheet for <asp:Label ID="lblTSPeriod" runat="server" /></h1>

    <!-- Timesheet header / job details -->
    <!--*********************************************************************************************-->
    <asp:Panel ID="pnlTSDetails" CssClass="ts_details_leave" runat="server">
        <ol>
        <li>
            <span class="field_name">Status:</span>
            <asp:Label ID="lblStatus" CssClass="field_value_status" runat="server" />
        </li>    
        <li>
            <span class="field_name">Name (SID):</span>
            <asp:Label CssClass="field_value" ID="lblName" runat="server" />
        </li>
        <li>
            <span class="field_name">Job Title:</span>
            <asp:Label ID="lblJobTitle" CssClass="field_value" runat="server" />
        </li>  
        <li id="liPayRate" runat="server" visible="false">
            <span class="field_name">Pay Rate:</span>
            <span class="field_value">
                <asp:Label ID="lblPayRate" runat="server" />&nbsp;&nbsp;
                <asp:Button ID="btnEditPayRate" runat="server" ToolTip="Edit Pay Rate" Text="Edit" CssClass="link_button" />
            </span>
        </li> 
        <li id="liBudgets" runat="server" visible="false">
            <span class="field_name">Budget(s):</span>
            <span class="field_value">
                <asp:Label ID="lblBudget" runat="server" />&nbsp;&nbsp;
                <asp:Button ID="btnEditBudgets" runat="server" ToolTip="Edit Budget(s)" Text="Edit" CssClass="link_button" />
            </span>
        </li>
        <li>
            <span class="field_name">Supervisor:</span>
            <span class="field_value">
                <asp:Label ID="lblSupervisor" runat="server" /> &nbsp;&nbsp;
                <asp:Button ID="btnEditSupervisor" runat="server" ToolTip="Edit Supervisor" Text="Edit" CssClass="link_button" />
            </span>
        </li>
        </ol>
    </asp:Panel>
    <!--*********************************************************************************************-->
</asp:Panel>

<asp:Panel ID="pnlUpdateSupervisor" CssClass="box" runat="server" Visible="false">
    <h2>Update Supervisor:</h2>
	<asp:TextBox ID="txtNewSupervisor" CssClass="field" width="500" runat="Server" />
	<p class="supervisor_note">(note: selection limited to active employees listed in the HP supervisors table.)</p>
	<TLR:AutoCompleteExtender 
	    ID="aceNameOrSID" 
	    CompletionListItemCssClass="AutoExtenderList"  
	    CompletionListHighlightedItemCssClass="AutoExtenderHighlight"
	    CompletionListCssClass="AutoExtender"
	    runat="server" 
	    TargetControlID="txtNewSupervisor" 
	    MinimumPrefixLength="3" 
	    servicemethod="SearchUsers" 
	    CompletionInterval="300" 
	    enablecaching="true" 
	    CompletionSetCount="10"
	    />
		 
	 <asp:Button ID="btnSearchSupervisors" runat="server" CssClass="button" Text="Update/Search" />
	 
    <div class="person_list">
    <h2 id="h2NoSearchResults" runat="server" visible="false">Your search returned no users.</h2>
    <asp:Repeater ID="rptUserSearchResults" Visible="false" runat="server">
    <HeaderTemplate>
    <h2>Please choose a person from the list</h2>
    <table class="tbl">
    <tr>
        <th>Name</th>
        <th>SID</th>
        <th>Department</th>
    </tr>
    </HeaderTemplate>
    <ItemTemplate>
    <tr>
        <td><asp:button ID="btnSelectUser" CssClass="link_button" runat="server" text='<%#DataBinder.Eval(Container, "DataItem.DisplayName")%>' OnClick='SupervisorSelected' CommandArgument='<%#Container.DataItem("SID")%>'  /></td>
        <td><%#DataBinder.Eval(Container, "DataItem.SID")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.DepartmentName")%></td>
    </tr>
    </ItemTemplate>
    <FooterTemplate>
    </table>
    </FooterTemplate>
    </asp:Repeater>
    </div>	       
</asp:Panel>

<asp:Panel ID="pnlUpdateBudgets" runat="server" CssClass="box" Visible="false">
    <h2>Manage Budgets:</h2>
    <fieldset title="Add New Budget">
    <asp:TextBox ID="txtNewBudget" runat="server" CssClass="field" MaxLength="14" />
    <asp:DropDownList ID="ddlEarningTypes" CssClass="field" runat="server" DataValueField="EarningTypeID" DataTextField="EarningTypeID" />
    <asp:Button ID="btnAddBudget" CssClass="button" runat="server" Text="Add Budget" />
    </fieldset>
    <asp:Repeater ID="rptBudgets" runat="server">
    <HeaderTemplate>
    <h2>Existing Budgets:</h2>
    <table class="tbl">
    <tr>
        <th>Budget</th>
        <th>Earning Type</th>
        <th>Delete</th>
    </tr>
    </HeaderTemplate>
    <ItemTemplate>
    <tr>
        <td><%#DataBinder.Eval(Container, "DataItem.BudgetNumber")%></td>    
        <td><%#DataBinder.Eval(Container, "DataItem.EarningTypeID")%></td>
        <td>
            <span onclick="javascript:return confirm('Are you sure you want to remove this budget from the timesheet?')">
                <asp:button ID="btnDeleteBudget" CssClass="link_button" runat="server" text="Delete" OnClick='DeleteBudget' CommandArgument='<%#Container.DataItem("TimesheetBudgetID")%>' />
            </span>
        </td>
    </tr>
    </ItemTemplate>
    <FooterTemplate>
    </table>
    </FooterTemplate>
    </asp:Repeater>
</asp:Panel>

<asp:Panel ID="pnlUpdatePayRate" CssClass="box" runat="server" Visible="false">
    <h2>Update Pay Rate:</h2>
    <asp:TextBox ID="txtNewPayRate" CssClass="field" runat="server" MaxLength="11" />
    <asp:Button ID="btnUpdatePayRate" CssClass="button" runat="server" Text="Update Pay Rate" />
</asp:Panel>

</asp:Content>
