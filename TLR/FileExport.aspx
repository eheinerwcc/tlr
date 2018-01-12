<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="FileExport.aspx.vb" Inherits="TLR.FileExport" title="<%$ Resources:GlobalText, PageTitle_FileExport %>"%>
<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
<ucl:Feedback id="uclFeedback" runat="server" />
<h1><asp:Label ID="lblPageHeader" runat="server"/></h1>

<asp:panel CssClass="box file_export" ID="pnlFileExport" runat="server">

<p id="pLeaveNote" class="export_note" visible="false" runat="server">
<strong>Note:</strong> Timesheets that contain ONLY entries of types VAC, CSL, CMP, P/H, PRL will be included in the file being exported.
All other timesheets will have to be processed manually.
</p>

<p id="pTimeNote" class="export_note" visible="false" runat="server">
<strong>Note:</strong> This process generates time and leave files for hourly employees. Only timesheets that do not 
    contain overtime are included in the files. All other timesheets will have to be processed manually.
</p>

<ol>
<li>
    <label for="<%=ddlPayPeriods.ClientID%>">Pay Period:</label>
    <span>
        <asp:DropDownList CssClass="field" ID="ddlPayPeriods" runat="server" DataTextField="PayPeriod" DataValueField="BeginDate" />
    </span>
</li>
<li>
    <span class="action">
        <span onclick="javascript:return confirm('Are you sure you want to generate the upload file at this time?')">
            <asp:Button ID="btnGenerateFile" runat="server" CssClass="button" text="Generate File" />
        </span>
     </span>
</li>
</ol>

</asp:panel>

<asp:Panel ID="pnlPreviousExports" CssClass="box" runat="server">
<div class="filetbl_title">
    <label for="<%=ddlYears.ClientID %>">Show files exported for year</label>&nbsp;<asp:dropdownlist ID="ddlYears" runat="server" CssClass="field" DataTextField="YearNumber" DataValueField="YearNumber" />  
    <asp:Button ID="btnLoadPreviousExports" CssClass="button" runat="server" Text="Show Files" />
</div>
    
    <asp:Repeater ID="rptExportedFileByYear" runat="server">
    <HeaderTemplate>
        <table class="tbl">
        <tr>
            <th><strong>Filename</strong></th>
            <th><strong>Date</strong></th>
            <th><strong>Created By</strong></th>
        </tr>
    </HeaderTemplate>
    <ItemTemplate>
        <tr>
            <td><%#DataBinder.Eval(Container, "DataItem.ExportFileName")%></td>
            <td><%#DataBinder.Eval(Container, "DataItem.CreatedOn")%></td>   
            <td><%#DataBinder.Eval(Container, "DataItem.DisplayName")%></td>
        </tr>
    </ItemTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
    </asp:Repeater>
</asp:Panel>
</asp:Content>
