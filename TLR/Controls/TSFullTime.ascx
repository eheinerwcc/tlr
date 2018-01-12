<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="TSFullTime.ascx.vb" Inherits="TLR.TS_FullTime" %>
<%@ Register Src="Feedback.ascx" TagName="Feedback" TagPrefix="ucl" %>
<%@ Register Src="TSActionLog.ascx" TagName="TSActionLog" TagPrefix="ucl" %>



<ucl:Feedback ID="uclFeedback" runat="server" /> 

<h1>Timesheet for <asp:Label ID="lblTSPeriod" runat="server" /> <br /> &nbsp;&nbsp;  <asp:label ID="lblDelegationInfo" runat="server" /></h1>


<!-- All messages related to importing work schedule-->
<!--*********************************************************************************************-->
<asp:Panel ID="pnlNoWorkSchedule" CssClass="workhours_message" runat="server" Visible="false">
<h2>My Weekly Work Hours</h2>
<p>
Do you usually work the same hours each week? If so,
let's save your weekly work hours. Saving your work hours makes it easier to fill out
a timesheet. Once your hours are saved, you can import those hours into every timesheet you create.
</p>
<asp:Button ID="btnEnterWorkHours" CssClass="button_important" text="Enter My Work Hours" runat="server" />
<asp:Button ID="btnAskMeLater" CssClass="button" text="Ask Me Later" runat="server" />
</asp:Panel>

<asp:Panel ID="pnlImportHours" CssClass="workhours_message" runat="server" Visible="false">
<h2>Import hours from your weekly work hours?</h2>
<p>
Would you like to import hours from your "weekly work hours" to your timesheet? 
Note: importing work hours will delete all entries you've made so far.
</p>
<asp:Button ID="btnImportWorkHours" CssClass="button_important" text="Import My Work Hours" runat="server" />
<asp:Button ID="btnDoNotImportWorkHours" CssClass="button" text="No Thanks" runat="server" />
</asp:Panel>
<asp:Panel ID="pnlWorkScheduleNotApproved" runat="server" CssClass="workhours_message" Visible="false">
    <p>
        Your weekly work hours have not yet been submitted. Submitting your work hours will make it easier to fill out a timesheet. 
        Would you like to pick up where you left off?
    </p>
    <asp:Button ID="btnWorkScheduleContinueEdit" CssClass="button" Text="Pick up where I left off" runat="server" />
</asp:Panel> 
<!--*********************************************************************************************-->

<div class="box">

<!-- Timesheet header / job details -->
<!--*********************************************************************************************-->
<asp:Panel CssClass="ts_details_leave" ID="pnlTSDetails" runat="server">
    <h2 class="hidden"><asp:Literal ID="litTSDetails" runat="server" Text="TS Details" /></h2>
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
        <span class="field_name">Supervisor(s):</span>
        <asp:Label ID="lblSupervisor" CssClass="field_value" runat="server" />
    </li>
    <li>
        <span class="field_name">Job Title:</span>
        <asp:Label ID="lblJobTitle" CssClass="field_value" runat="server" />
    </li>
    <li>
        <span class="field_name">Due Date:</span>
        <asp:Label ID="lblDueDate" CssClass="field_value" runat="server" />
    </li>    
    <li id="liUpdateTimesheet" runat="server" visible="false">
        <span class="field_name">&nbsp;</span>
        <a href="UpdateTimesheet.aspx?TimesheetID=<%=request.querystring("TimesheetID") %>">Update Timesheet Information</a>
    </li>
    </ol>
</asp:Panel>


</div>


<div class="box">


<!--*********************************************************************************************-->

<!-- Interface for adding timesheet entries-->
<!--*********************************************************************************************-->
<asp:Panel CssClass="add_entry" ID="pnlEntryDetails" runat="server" DefaultButton="btnAddEntry" Visible="false">
    <h2><asp:Literal ID="litEntryHeader" runat="server" Text="Add Entry" /></h2>
    <ol>
    <li>
        <label for="<%=ddlDate.ClientID%>">Date:</label>
        <span><asp:DropDownList CssClass="field" width="160" ID="ddlDate" runat="server" /></span>
    </li>
    <li>
        <label for="<%=ddlEntryType.ClientID%>">Entry Type:</label>
        <span><asp:DropDownList CssClass="field" width="160" ID="ddlEntryType" runat="server" DataTextField="LeaveType" DataValueField="EntryTypeID" /></span>
    </li>
    <li>
        <label for="<%=txtHours.ClientID%>">Duration:</label>
        <span><asp:TextBox ID="txtHours" Width="40" MaxLength="5" CssClass="Form_Field" runat="server" /></span>
    </li>
    <li>
        <span class="update">
            <asp:Button ID="btnAddEntry" CssClass="button" Text="Add Entry" runat="server" />
            <asp:Button ID="btnUpdateEntry" CssClass="button" Text="Update" Visible="false" runat="server" />
            <asp:Button ID="btnDeleteEntry" CssClass="button" Text="Delete" Visible="false" runat="server" />
            <asp:Button ID="btnCancelUpdate" CssClass="button" Text="Cancel" Visible="false" runat="server" />
        </span>
    </li>
    </ol>
</asp:Panel>

<!--*********************************************************************************************-->



<!-- Timesheet totals-->
<!--*********************************************************************************************-->
<asp:Panel CssClass="ts_totals" ID="pnlTimesheetTotals" runat="server">
<h2 class="hidden">Timesheet Totals</h2>   
  <asp:Repeater ID="rptTimesheetTotals" runat="server">
   <HeaderTemplate>
   <table class="tbl" title="Timesheet totals and leave balance">
    <tr>
        <th><abbr title="Leave type">Type</abbr></th>
        <th><abbr title="Available balance">Avail Bal</abbr></th>
        <th>Reported</th>
        <th><abbr title="Estimated ending balance">Est New Bal</abbr></th>
    </tr>
   </HeaderTemplate>
   <ItemTemplate>
    <tr>
        <td class="leave_type"><abbr title='<%#DataBinder.Eval(Container, "DataItem.LeaveType")%>'><%#DataBinder.Eval(Container, "DataItem.EntryTypeID")%></abbr></td>
        <td><%#DataBinder.Eval(Container, "DataItem.PrevBalance")%></td>
        <td><%#DataBinder.Eval(Container, "DataItem.TimesheetTotal")%></td>
        <td class='<%#IIf(DataBinder.Eval(Container, "DataItem.NegativeBalance") = 1, "negative_balance", "")%>'>
        <%#IIf(DataBinder.Eval(Container, "DataItem.NegativeBalance") = 1, "<abbr title='Timesheet entries result in negative leave balance'>" & DataBinder.Eval(Container, "DataItem.EndBalance") & "</abbr>", DataBinder.Eval(Container, "DataItem.EndBalance"))%>
        </td>
    </tr>
   </ItemTemplate>
   <FooterTemplate>
     
   
   </table>
   </FooterTemplate>
   </asp:Repeater> 
</asp:Panel>
<!--*********************************************************************************************-->

</div>

<div id="compTimeMsg">
      <asp:Label ID="lblCompTimeMsg" runat="server" Text=""></asp:Label>
</div>

<!-- Timesheet calendar view-->
<!--*********************************************************************************************-->
<h2>Timesheet</h2>
<div class="jaws_note">
<%=Resources.GlobalText.Note_TSFullTime_JawsUsers%>
</div>
<div class="payperiod">
<asp:Repeater ID="rptWeeks" runat="server">
<ItemTemplate>
    <h3>Week of <%#Format(Container.DataItem("FirstDay"), "M")%> - <%#Format(Container.DataItem("LastDay"), "M")%></h3>
    <asp:Repeater ID="rptDays" OnItemDataBound="rptDays_ItemDataBound" runat="server">
        <HeaderTemplate>
            <ul class="week">
        </HeaderTemplate>
        <ItemTemplate>
        
            <li class="<%#IIf(Container.DataItem("InPayPeriod")=0,"inactive","") %> <%#IIf(Container.DataItem("Holiday")=True,"holiday","") %>">
                
                <h4><abbr class="day" title="<%#Format(Container.DataItem("OneDay"), "dddd")%>"><%#Format(Container.DataItem("OneDay"), "ddd")%></abbr><abbr class="date" title="<%#Format(Container.DataItem("OneDay"), "m")%>"><%#Format(Container.DataItem("OneDay"), "M/d")%></abbr></h4>
                
                <asp:Repeater ID="rptEntries" runat="server">
                    <HeaderTemplate>
                    <ul class="entries">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li>
                            <span runat="server" visible='<%# Not ViewState("blnClickableGridMode") %>'><abbr title='<%#Container.DataItem("EntryType") & ": " & Format(Container.DataItem("Duration"), "0.##") & IIf(Not Container.DataItem("LeavePreapproved"), ", Not Pre-approved", "")%>'><%#Container.DataItem("EntryTypeID")%> - <%#Format(Container.DataItem("Duration"), "0.##")%></abbr></span>
                            
                            <asp:Button ID="btnLeave" 
                            cssclass='<%# iif(ViewState("TimesheetStatusID") = GetSupervisorApprovalStatusID(), iif((Container.DataItem("EntryTypeID") = "V"), "ts_entry_button", "ts_entry_button_disabled"), "ts_entry_button") %>'
                            Enabled='<%# iif(ViewState("TimesheetStatusID") = GetSupervisorApprovalStatusID(), (Container.DataItem("EntryTypeID") = "V"), true) %>' 
                            OnClick="EditEntry" 
                            Title='<%#Container.DataItem("EntryType") & ": " & Format(Container.DataItem("Duration"), "0.##") & IIf(Not Container.DataItem("LeavePreapproved"), ", Not Pre-approved", "")%>'
                            Visible='<%#ViewState("blnClickableGridMode") %>' 
                            text='<%#Container.DataItem("EntryTypeID") & " - " & Format(Container.DataItem("Duration"), "0.##") & IIf(Not Container.DataItem("LeavePreapproved"), ", (NP)", "")%>'
                            CommandArgument='<%#Container.DataItem("TimeSheetEntryID")%>'
                            runat="server" />
                            
                            <asp:linkbutton Enabled='<%# iif(ViewState("TimesheetStatusID") = GetSupervisorApprovalStatusID(), (Container.DataItem("EntryTypeID") = "V"), true) %>' ID="btnLeave_old" runat="server" OnClick="EditEntry" Visible="false" CommandArgument='<%#Container.DataItem("TimeSheetEntryID")%>'><abbr title='<%#Container.DataItem("EntryType") & ": " & Format(Container.DataItem("Duration"), "#.##")%>'><%#Container.DataItem("EntryTypeID")%> - <%#Format(Container.DataItem("Duration"), "#.##")%> </abbr><%#IIf(Not Container.DataItem("LeavePreapproved"), "<span> <abbr title='Not Pre-approved'>(NP)</abbr></span>", "")%></asp:linkbutton> 
                        </li>
                    </ItemTemplate>
                    <FooterTemplate>
                    </ul>
                    </FooterTemplate>
                </asp:Repeater>
                
              <%#IIf(Container.DataItem("Holiday") = True, "<abbr class='holiday' title='holiday'>H</abbr>", "")%>
            </li>
            
        </ItemTemplate>
        <FooterTemplate>
            </ul>
            <br style="clear:both" />
        </FooterTemplate>
    </asp:Repeater>
</ItemTemplate>        
</asp:Repeater>

  


</div>
<!--*********************************************************************************************-->

<!--Holidays-->
<!--*********************************************************************************************-->

<div class="holidays">
  <h2>Holidays</h2>
  <ul>
  <li id="liGenericHoliday" class="holidayMsg" runat="server">There are no holidays this pay period.</li>
  </ul>
  <asp:Repeater ID="rptHolidays" runat="server">
      <HeaderTemplate>
        <ul class="entries">
          
      </HeaderTemplate>
      <ItemTemplate>
        <li>
          <%#DataBinder.Eval(Container, "DataItem.Date")%> - 
          <%#DataBinder.Eval(Container, "DataItem.Description")%>
        </li>
      </ItemTemplate>
      <FooterTemplate>
        </ul>
      </FooterTemplate>
    </asp:Repeater>

</div>
<!--*********************************************************************************************-->


<div class="remarks">
<h2 id="h2Remarks" runat="server">Remarks</h2>
<asp:Repeater ID="rptRemarks" runat="server">
<HeaderTemplate>
    <table>
    <tr>
        <th>Remark Text</th>
        <th>Author/Date</th>
        <th>Action</th>
    </tr>
</HeaderTemplate>
<FooterTemplate>
    </table>
</FooterTemplate>
<ItemTemplate>
    <tr>
        <td class="text"><%#TLR.clsGeneric.PrepareForDisplay(Container.DataItem("RemarkText"))%></td>
        <td class="author_date"><%#Container.DataItem("DisplayName")%><br /><%#Format(Container.DataItem("LastUpdatedDate"), "g")%></td>
        <td class="action">
            <asp:Button ID="btnEditRemark" Text="Edit" CssClass="link" runat="server" CommandArgument='<%#Container.DataItem("TimesheetRemarkID")%>' OnClick="EditRemark" Visible='<%#IIf(Container.DataItem("SID") = TLR.clsSession.UserSID and ViewState("blnEditableRemarkMode"), true, false) %>' />                    
            <span onclick="javascript:return confirm('Are you sure you want to delete this remark?')">
                <asp:Button ID="btnDeleteRemark" Text="Delete" CssClass="link" runat="server" CommandArgument='<%#Container.DataItem("TimesheetRemarkID")%>' OnClick="DeleteRemark" Visible='<%#IIf(Container.DataItem("SID") = TLR.clsSession.UserSID and ViewState("blnEditableRemarkMode"), true, false) %>' />
            </span>            
        </td>
    </tr>
</ItemTemplate>
</asp:Repeater>

<asp:Button ID="btnAddRemark" CssClass="button" Text="Add Remark" runat="server" Visible='false' />
    <asp:Panel ID="pnlEditRemark" class="remark_edit" runat="server" Visible="false">
    <label for="<%=ddlDate.ClientID%>">Remark Text:</label>
    <span><asp:TextBox ID="txtRemark" TextMode="MultiLine" class="remark_text" Width="400" Height="60" runat="server" /></span>
        <span class="update">
            <asp:Button ID="btnSaveRemark" CssClass="button" Text="Save Remark" runat="server" />
            <asp:Button ID="btnCancelRemarkUpdate" CssClass="button" Text="Cancel" runat="server"/>
     </span>
     </asp:Panel>
</div>
<!--*********************************************************************************************-->


<!--Overtime submission interface-->
<!--*********************************************************************************************-->
<div class="ts_overtime">
<h2 id="h2Overtime" runat="server" visible="false">Overtime</h2>
<asp:Panel ID="pnlOvertime" CssClass="panel" runat="server" DefaultButton="btnAddEntry" Visible="true">
    <p id="pOT" runat="server" visible="false">
    Before submitting your timesheet please specify how you would like your overtime split. You can either get it paid or converted to compensatory (comp.) time.<br />
    Currently you have <asp:Label CssClass="ot_hours" ID="lblOTHours" runat="server" /> hours of overtime.
    </p>
    <ol>
    <li>
        <span class="field_name">Pay overtime:</span>
        <span class="field_value"><asp:TextBox ID="txtOT_Pay" Width="40" MaxLength="5" CssClass="field" ReadOnly="true" runat="server" /> hours</span>
    </li>
    <li>
        <span class="field_name">Convert to <abbr title="compensatory">comp.</abbr> time:</span>
        <span class="field_value"><asp:TextBox ID="txtOT_Convert" Width="40" MaxLength="5" CssClass="field" ReadOnly="true" runat="server" /> hours</span>
    </li>    
    </ol>
</asp:Panel>
</div>


<!--*********************************************************************************************-->

<div class="ts_actions">
<!--Timesheet submission interface-->
<!--*********************************************************************************************-->
<asp:Panel ID="pnlSubmissionInterface" class="ts_action_panel" runat="server" Visible="false">

<span onclick="javascript:return confirm('Are you sure you want to submit this timesheet to your supervisor for approval?');">
<asp:Button ID="btnSubmitTimesheet" CssClass="button_important" Text="Submit Timesheet" runat="server" />
</span>
<span onclick="javascript:return confirm('Are you sure you want DELETE this timesheet?')">
<asp:Button ID="btnDeleteTimesheet" CssClass="button" Text="Delete Timesheet" runat="server" />
</span>
</asp:Panel>
<!--*********************************************************************************************-->

<!--Approval interface-->
<!--*********************************************************************************************-->
<asp:Panel ID="pnlApprovalInterface" class="ts_action_panel" runat="server" Visible="false">
<span runat="server" id="spanApprove"  onclick="javascript:return confirm('Are you sure you want to approve this timesheet?')">
<asp:Button ID="btnApproveTimesheet" CssClass="button_important" Text="Approve Timesheet" runat="server" />
</span>
<span onclick="javascript:return confirm('Are you sure you want REJECT this timesheet?')">
<asp:Button ID="btnRejectTimesheet" CssClass="button" Text="Reject" runat="server" />
</span>
<label for"<%=txtComment.ClientID%>">Comment:</label>
<asp:TextBox ID="txtComment" CssClass="comment" TextMode="MultiLine" runat="server" />
</asp:Panel>
<!--*********************************************************************************************-->
</div>
        
<ucl:TSActionLog ID="uclTSActionLog" runat="server" Visible="true" />