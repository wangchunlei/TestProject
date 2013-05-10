<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: ViewData["Message"] %></h2>
    <p>
        This website demonstrates the following:
    </p>
    <ul>
        <li>Logging with ELMAH</li>
        <li>Logging with NLog</li>
        <li>Logging with Log4Net</li>
        <li>Logging with Health Monitoring</li>
        <li>RSS feeds</li>        
        <li>Google Visualization (charting)</li>        
        <li>Paging</li>
        <li>OpenID login facility</li>        
    </ul>    

</asp:Content>
