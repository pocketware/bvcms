<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/onlinereg.Master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="registerContent" ContentPlaceHolderID="MainContent" runat="server">
   <h2>Transaction Completed</h2>
    <p>
        Thank you for your payment of <%=ViewData["AmountDue"] %> for <%=ViewData["Header"] %>  
        You should receive a confirmation email at <%=ViewData["Email"] %> shortly.
    </p>
</asp:Content>