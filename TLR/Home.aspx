<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Home.aspx.vb" MasterPageFile="~/mpDefault.Master" Inherits="TLR._Default" Title="<%$ Resources:GlobalText, PageTitle_HOME %>" %>

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" Runat="Server">
  <div id="home" class="page">
<ucl:Feedback id="uclFeedback" runat="server" />
<asp:Panel ID="pnlLeave" runat="server">

<h1 class="PanelTitle">Report Time and Leave</h1>

<asp:Panel ID="pnlActiveTimesheets" runat="server">
   <h2>My Active Timesheets:</h2>
   <asp:Label ID="lblNoActiveTimesheets" CssClass="MainText" Visible="false" runat="server">You have no active timesheets.</asp:Label>
   <asp:Repeater ID="rptActiveTimesheets" runat="server">
   <HeaderTemplate>
   <table class="tbl timesheetlist">
        <thead>
            <tr>
                <th class="pay_period">Pay Period</th>
                <th>Job Title</th>
                <th>Department</th>
                <th>Supervisor</th>
                <th class="pay_rate">Pay Rate</th>
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
</asp:Panel>


<asp:Panel ID="pnlCreateTimesheet" CssClass="create_timesheet panel" runat="server">
    <h2>Create New Timesheet:</h2>
    <asp:Label ID="lblNoActiveJobs" CssClass="MainText" Visible="false" runat="server">You are not allowed to create timesheets.</asp:Label>
    <asp:Repeater ID="rptJobList" runat="server">
    <HeaderTemplate>
    <table class="tbl joblist">
        <thead>
            <tr>
                <th class="job_number">#</th>
                <th>Job Title</th>
                <th>Department</th>
                <th>Supervisor</th>
                <th class="pay_rate">Pay Rate</th>
                <th>Pay Period</th>
            </tr>
        </thead>
    </HeaderTemplate>
    <ItemTemplate>
    <tr>
    <td>
        <input id="rdoJobNumber<%#Container.ItemIndex%>" type="radio" aria-label="Job Number" <asp:Literal ID="litJobChecked" runat="server" /> name="rdoJobNumber" value="<%#Container.ItemIndex%>" />
        
        <asp:Label ID="lblJobNumber" runat="server" Visible="false" for="rdoJobNumber<%#Container.ItemIndex%>" Text='<%#Container.DataItem("JobNumber")%>' />
    </td>
    <td><span><%#Container.DataItem("TitleShort")%></span></td>
    <td><span><%#Container.DataItem("DepartmentName")%></span></td>
    <td><span><%#Container.DataItem("Supervisor")%></span></td>
    <td><span><%#IIf(DataBinder.Eval(Container, "DataItem.PayRate") <> 0, String.Format("{0:C}", CDbl(Container.DataItem("PayRate"))), "<abbr title='Not Applicable'>N/A</abbr>")%></span></td>
    <td><asp:DropDownList CssClass="field" ID="ddlPayPeriod" DataTextField="DisplayText" DataValueField="PayCycleID" runat="server" aria-label="Pay Cycle" /></td>
    </tr>
    
        
    </ItemTemplate>
    <FooterTemplate>
    </table>
    </FooterTemplate>
    </asp:Repeater>
    
    <asp:Button ID="btnCreateTimeSheet" CssClass="button" Text="Create New Timesheet" runat="server" />
</asp:Panel>





<asp:Panel CssClass="pnl_workschedule" ID="pnlWorkSchedule" runat="server" Visible="false">
    <h2>My Weekly Work Hours</h2>
    <asp:Panel ID="pnlWorkScheduleOffer" runat="server" CssClass="workhours_message" Visible="false">
        <p>
        Do you usually work the same hours each week?  If so, let's save your weekly work hours. 
        Saving your work hours makes it easy to fill out a timesheet.
        </p>
        <asp:Button ID="btnEnterWorkSchedule" CssClass="button_important" Text="Enter My Work Hours" runat="server" />
    </asp:Panel>
    
    <asp:Panel ID="pnlWorkScheduleCurrent" runat="server" Visible="false">
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
        <asp:Button ID="btnEditWorkSchedule" CssClass="button" Text="Edit Work Hours" runat="server" />
    </asp:Panel>
    
    <asp:Panel ID="pnlWorkScheduleNotApproved" runat="server" CssClass="workhours_message" Visible="false">
    <p>
        Your weekly work hours have not yet been submitted. Submitting your work hours will make it easier to fill out a timesheet. 
        Would you like to pick up where you left off?
    </p>
        <asp:Button ID="btnWorkScheduleContinueEdit" CssClass="button" Text="Pick up where I left off" runat="server" />
    </asp:Panel>        
</asp:Panel>



</asp:Panel>

</div><!-- End #page -->
</asp:Content>

