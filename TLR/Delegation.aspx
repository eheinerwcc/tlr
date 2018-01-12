<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="Delegation.aspx.vb" Inherits="TLR.Delegation" title="<%$ Resources:GlobalText, PageTitle_DELEGATION %>" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="TLR" %>

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <TLR:ToolkitScriptManager ID="ToolkitScriptManager" runat="server" />
    <ucl:Feedback id="uclFeedback" runat="server" />
    <h1>Signature Delegation</h1>

    <asp:Panel ID="pnlUserSearch" runat="server">
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
        
        
    <asp:Panel ID="pnlUserList" runat="server" Visible="false">
        <div class="person_list">
        <asp:Repeater ID="rptUserSearchResults" Visible="false" runat="server">
        <HeaderTemplate>
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
        
        <!-- Signature Delegation Info -->
        <asp:panel ID="pnlSignatureDelegation" runat="server">


            <asp:Repeater ID="rptExistingDelegations" runat="server">
            <HeaderTemplate>
            <h2>Existing Delegations:</h2>
            <table class="tbl">
            <tr>
                <th>Supervisor</th>
                <th>Delete</th>
            </tr>
            </HeaderTemplate>
            <ItemTemplate>
            <tr>
                <td><%#DataBinder.Eval(Container, "DataItem.SupervisorDisplayName")%></td>
                <td>
                    <span onclick="javascript:return confirm('Are you sure you want to remove this delegation?')">
                        <asp:button ID="btnDeleteDelegation" CssClass="link_button" runat="server" text="Delete" OnClick='DeleteDelegation' CommandArgument='<%#Container.DataItem("DelegationID")%>' />
                    </span>
                </td>
            </tr>
            </ItemTemplate>
            <FooterTemplate>
            </table>
            </FooterTemplate>
            </asp:Repeater>

        <h2>Add New Delegation:</h2>
            Select User: <asp:DropDownList ID="ddlSupervisors" runat="server" DataTextField="DisplayName" DataValueField="SuperID" />
            <asp:Button ID="btnAddDelegation" runat="server" Text="Add Delegation" />
        </asp:panel>
                </div>
    </asp:Panel>

        

</asp:Content>
