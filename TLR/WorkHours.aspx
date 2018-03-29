<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/mpDefault.Master" CodeBehind="WorkHours.aspx.vb" Inherits="TLR.WorkHours" title="<%$ Resources:GlobalText, PageTitle_WORKHOURS %>" ViewStateEncryptionMode="Always" %>


<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <ucl:Feedback id="uclFeedback" runat="server" />

<div class="box">
<h1>My Work Hours</h1>

<p class="workschedule_status">Current schedule status: <asp:Label ID="lblWorkScheduleStatus" runat="server" /></p>

<div class="box enterworkhours">
<h2><asp:Label ID="lblWorkScheduleEntryTitle" runat="server" Text="Add New Entry" /></h2>
    <fieldset title="Add/Edit work hours entry">
    <legend title="Add/Edit work hours entry">Add/Edit work hours entry</legend>
    <ol>
    <li>
        <label for="<%=ddlDay.ClientID%>">On this day of the week:</label>
        <span>
            <asp:DropDownList CssClass="field" width="150" ID="ddlDay" runat="server">
                <asp:ListItem Value="2" Selected="True">Monday</asp:ListItem>
                <asp:ListItem Value="3" Text="Tuesday" />
                <asp:ListItem Value="4" Text="Wednesday" />
                <asp:ListItem Value="5" Text="Thursday" />
                <asp:ListItem Value="6" Text="Friday" />
                <asp:ListItem Value="7" Text="Saturday" />
                <asp:ListItem Value="1" Text="Sunday" />
            </asp:DropDownList>
        </span>
    </li>
    <li>
        <label for="fsWorkStartTime">I start work at:</label>
        <fieldset id="fsWorkStartTime">
            <legend>I start work at</legend>
        <span>
            <label for="<%=ddlStartHour.ClientID%>" class="invisible_label">Start hour:</label>
            <asp:DropDownList CssClass="field" ID="ddlStartHour" runat="server" />
            <label for="<%=ddlStartMinute.ClientID%>" class="invisible_label">Start minute:</label>
            <asp:DropDownList CssClass="field" ID="ddlStartMinute" runat="server" />
            <label for="<%=ddlStartAMPM.ClientID%>" class="invisible_label">Start AM/PM:</label>
            <asp:DropDownList CssClass="field" ID="ddlStartAMPM" runat="server">
                <asp:ListItem Text="AM" Value="AM" />
                <asp:ListItem Text="PM" Value="PM" />
            </asp:DropDownList>
        </span>
        </fieldset>
    </li>
    <li>
        <label for="fsWorkEndTime">And end work at:</label>
        <fieldset id="fsWorkEndTime">
            <legend>And end work at</legend>
        <span>
            <label for="<%=ddlEndHour.ClientID%>" class="invisible_label">End hour:</label>
            <asp:DropDownList CssClass="field" ID="ddlEndHour" runat="server" />
            <label for="<%=ddlEndMinute.ClientID%>" class="invisible_label">End minute:</label>
            <asp:DropDownList CssClass="field" ID="ddlEndMinute" runat="server" />
            <label for="<%=ddlEndAMPM.ClientID%>" class="invisible_label">End AM/PM:</label>
            <asp:DropDownList CssClass="field" ID="ddlEndAMPM" runat="server">
                <asp:ListItem Text="AM" Value="AM" />
                <asp:ListItem Text="PM" Value="PM" />
            </asp:DropDownList>
        </span>
        </fieldset>
    </li>
    <li>
        <label for="<%=ddlMealTime.ClientID%>">With a meal break of:</label>
        <span><asp:DropDownList CssClass="field" ID="ddlMealTime" runat="server" /></span>
    </li>
    <li>
        <span class="update">
            <asp:Button ID="btnAddEntry" CssClass="button" Text="Add to My Work Hours" runat="server" />
            <asp:Button ID="btnUpdateEntry" CssClass="button" Text="Update" Visible="false" runat="server" />
            <asp:Button ID="btnDeleteEntry" CssClass="button" Text="Delete" Visible="false" runat="server" />
            <asp:Button ID="btnCancelUpdate" CssClass="button" Text="Cancel" Visible="false" runat="server" />
        </span>
    </li>
    </ol>
    </fieldset>
</div>  

<asp:Repeater ID="rptWorkSchedule" runat="server">
<HeaderTemplate>
<table class="tbl">
    <thead>
        <tr>
            <th>Day</th>
            <th>Start</th>
            <th>End</th>   
            <th>Meal</th>
            <th>Total</th>
            <th>Action</th>
        </tr>
    </thead>
</HeaderTemplate>
<FooterTemplate>
</table>
</FooterTemplate>
<ItemTemplate>
<tr>
<td><%#System.Enum.GetValues(GetType(DayOfWeek))(Container.DataItem("DayOfWeek") - 1).ToString%></td>
<td><%#Format(Container.DataItem("StartTime"), "t")%></td>
<td><%#Format(Container.DataItem("EndTime"), "t")%></td>
<td><%#Container.DataItem("MealTime")%> <abbr title="Minutes">min.</abbr></td>
<td><%#Math.Floor(Container.DataItem("TotalMinutes") / 60).ToString + "<abbr title='Hours'> hrs.</abbr> " + IIf(Container.DataItem("TotalMinutes") Mod 60 = 0, "", CInt(Container.DataItem("TotalMinutes") Mod 60).ToString + " <abbr title='Minutes'>mins.</abbr>")%></td>
<td><asp:Button ID="btnEdit" Text="Edit" CssClass="link_button" runat="server" CommandArgument='<%#Container.DataItem("WorkScheduleEntryID")%>' OnClick="EditEntry" />
    <asp:Button ID="btnDelete" Text="Delete" CssClass="link_button" OnClientClick="return confirm('Are you sufre you want to delete this entry?');" runat="server" CommandArgument='<%#Container.DataItem("WorkScheduleEntryID")%>' OnClick="DeleteEntry" />
</td>
</tr>

</ItemTemplate>
</asp:Repeater>
    <span class="workschedule_total">
    <asp:Label ID="lblTotalWorkHours" runat="server" />
    </span>
    <asp:Button ID="btnSubmitWorkSchedule" CssClass="button_important" Text="Submit Work Hours" runat="server" />
</div>
</asp:Content>
