<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="MvcLoggingDemo.Models" %>
<%
    if (Request.IsAuthenticated) {
%>
        Welcome <b><%: ((WebIdentity)Page.User.Identity).FriendlyName %></b>!
        [ <%: Html.ActionLink("Log Off", "LogOff", "Account") %> ]
<%
    }
    else {
%> 
        [ <%: Html.ActionLink("Log On", "LogOn", new { controller = "Account", returnUrl = HttpContext.Current.Request.RawUrl }) %> ]
<%
    }
%>
