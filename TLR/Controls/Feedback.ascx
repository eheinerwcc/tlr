<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Feedback.ascx.vb" Inherits="TLR.Feedback" %>
<asp:Panel ID="pnlFeedback" Visible="False" Runat="server" CssClass="panel_feedback_success">
<asp:Label ID="lblFeedback" Runat="server"></asp:Label></asp:Panel>
<%--
'Add the user control to the web.config in this manner to make the control available without having to register it on the page
<pages>
<controls>
<add tagPrefix="ucl" src="~/controls/Feedback.ascx" tagName="Feedback"/>
</controls>
</pages>

'Once it has been added to the web config, it can be declared anywhere in this manner:
<ucl:Feedback id="uclFeedback" runat="server" />

'Put the following in CSS file:
    
 /* ______________________ Feedback Classes _____________________ */
.panel_feedback_success	{padding: 15px 15px 15px 15px; margin: 10px 10px 10px 0px; border: 3px double #336699; background-color: white; color: #336699; font: 12pt/150% Tahoma, Verdana;}						 
.panel_feedback_error	{padding: 15px 15px 15px 15px; margin: 10px 10px 10px 0px; border: 3px double red; background-color: white; color: red; font: 12pt/150% Tahoma, Verdana; text-align: left;}						 
.panel_feedback_error ul    {padding: 0px 0px 0px 25px; margin: 0px 0px 0px 0px; font-size: 10pt; line-height: 160%;}
.panel_feedback_note	{padding: 15px 15px 15px 15px; margin: 10px 10px 10px 0px; border: 3px double green; background-color: white; color: green; font: 12pt/150% Tahoma, Verdana;}						 

    
--%>