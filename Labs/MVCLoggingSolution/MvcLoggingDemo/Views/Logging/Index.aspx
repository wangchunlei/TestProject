<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MvcLoggingDemo.ViewModels.LoggingIndexModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Index
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Index</h2>

    <div class="grid-options">
        View : 
        <strong>List</strong>
        | <%: Html.ActionLink("Chart", "Chart")%>
        | <%: Html.ActionLink("RSS", "RssFeed", new { LoggerProviderName = Model.LoggerProviderName, Period = Model.Period, LogLevel = Model.LogLevel }, new { target = "_blank" })%>
    </div>  

    <% using (Html.BeginForm("Index", "Logging", new { page = 0 }, FormMethod.Get, new { id = "myform" }))
       {%>

    <div class="grid-filter">        
        <div class="inner">

        Logger : <%: Html.DropDownList("LoggerProviderName", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.LogProviderNames, "Value", "Text"))%>  

        Level : <%: Html.DropDownList("LogLevel", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.LogLevels, "Value", "Text"))%>  

        For : <%: Html.DropDownList("Period", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.CommonTimePeriods, "Value", "Text"))%>  
        
        <input id="btnGo" name="btnGo" type="submit" value="Apply Filter" />                      

        </div>
    </div>

    <div class="grid-header">                           

        <div class="grid-results">
            <div class="inner">
            
                <span style="float: left">                
                    <%: string.Format("{0} records found. Page {1} of {2}", Model.LogEvents.TotalItemCount, Model.LogEvents.PageNumber, Model.LogEvents.PageCount)%>
                </span>

                <span style="float: right">
                    Show <%: Html.DropDownList("PageSize", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.PagingPageSizes, "Value", "Text"), new { onchange = "document.getElementById('myform').submit()" })%> results per page
                </span>

                <div style="clear: both"></div>

            </div>

        </div>

        <div class="paging">
            <div class="pager">
		    <%= Html.Pager(ViewData.Model.LogEvents.PageSize, ViewData.Model.LogEvents.PageNumber, ViewData.Model.LogEvents.TotalItemCount, new { LoggerProviderName = ViewData["LoggerProviderName"], LogLevel = ViewData["LogLevel"], Period = ViewData["Period"], PageSize = ViewData["PageSize"] })%>
            </div>
	    </div>            
        
    </div>

    <% } %>

    <% if (Model.LogEvents.Count() == 0) { %>

    <p>No results found</p>

    <% } else { %>

    <div class="grid-container">
    <table class="grid">
        <tr>
            <th></th>
            <th>
                #
            </th>
            <th>
                Log
            </th>
            <th>
                Date
            </th>
            <th style='white-space: nowrap;'>
                Time ago
            </th>
            <th>
                Host
            </th>
            <th>
                Source
            </th>
            <th>
                Message
            </th>
            <th>
                Type
            </th>
            <th>
                Level
            </th>
        </tr>

    <% int i = 0;  foreach (var item in Model.LogEvents)
       { %>
    
        <tr class="<%= i++ % 2 == 1 ? "alt" : "" %>">
            <td>                
                <%: Html.ActionLink("Details", "Details", new { id = item.Id.ToString(), loggerProviderName = item.LoggerProviderName })%>              
            </td>
            <td>
                <%: i.ToString() %>
            </td>
            <td>
                <%: item.LoggerProviderName%>
            </td>
            <td style='white-space: nowrap;'>
                <%: String.Format("{0:g}", item.LogDate.ToLocalTime())%>
            </td>
            <td style='white-space: nowrap;'>
                <%: item.LogDate.ToLocalTime().TimeAgoString()%>
            </td>
            <td>
                <%: item.MachineName%>
            </td>
            <td>
                <%: item.Source%>
            </td>
            <td>
                <pre><%: item.Message.WordWrap(80) %></pre>
            </td>
            <td>
                <%: item.Type%>
            </td>
            <td>
                <%: item.Level.ToLower().ToPascalCase() %>
            </td>
        </tr>
    
    <% } %>

    </table>  
    </div>
    
    <% } %>  

    <div class="grid-header">                           

        <div class="paging">
            <div class="pager">
		    <%= Html.Pager(ViewData.Model.LogEvents.PageSize, ViewData.Model.LogEvents.PageNumber, ViewData.Model.LogEvents.TotalItemCount, new { LoggerProviderName = ViewData["LoggerProviderName"], LogLevel = ViewData["LogLevel"], Period = ViewData["Period"], PageSize = ViewData["PageSize"] })%>
            </div>
	    </div>            
        
    </div>

</asp:Content>

