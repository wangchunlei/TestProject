<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MvcLoggingDemo.ViewModels.LoggingIndexModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Chart
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Chart</h2>

    <% using (Html.BeginForm("Chart", "Logging", FormMethod.Get, new { id = "myform" }))
       {%>

    <div class="grid-options">
        View : 
        <%: Html.ActionLink("List", "Index")%>
        | <strong>Chart</strong>
        | <%: Html.ActionLink("RSS", "RssFeed", new { LoggerProviderName = Model.LoggerProviderName, Period = Model.Period, LogLevel = Model.LogLevel }, new { target = "_blank" })%>        
    </div>  

    <div class="grid-filter">        
        <div class="inner">

        Logger : <%: Html.DropDownList("LoggerProviderName", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.LogProviderNames, "Value", "Text"))%>  

        Level : <%: Html.DropDownList("LogLevel", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.LogLevels, "Value", "Text"))%>  

        For : <%: Html.DropDownList("Period", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.CommonTimePeriods, "Value", "Text"))%>  
        
        <input id="btnGo" name="btnGo" type="submit" value="Apply Filter" />                      

        </div>
    </div>


    <% } %>

    <script type="text/javascript" src="http://www.google.com/jsapi"></script>

    <script type="text/javascript">

        // Load the Visualization API and the corechart package.
        google.load("visualization", "1", { packages: ["corechart"] });

        // Set a callback to run when the Google Visualization API is loaded.
        google.setOnLoadCallback(drawChart);

        function drawChart() {

            var loggerProviderName = $("#LoggerProviderName").val();
            var period = $("#Period").val();
            var logLevel = $("#LogLevel").val();            

            var actionUrl = "/Logging/ChartData";
            actionUrl += "?LoggerProviderName=" + loggerProviderName;
            actionUrl += "&Period=" + period;
            actionUrl += "&LogLevel=" + logLevel;

            // alert(actionUrl);

            var testJson = $.getJSON(actionUrl, function (chartdata) {

                var data = new google.visualization.DataTable();

                // column
                for (var col = 0; col < chartdata.cols.length; col++) {                    
                    data.addColumn(chartdata.cols[col].type, chartdata.cols[col].label);
                }                

                // row
                for (var row = 0; row < chartdata.rows.length; row++) {                    

                    data.addRow();

                    for (var col = 0; col < chartdata.cols.length; col++) {                        

                        var myRow = chartdata.rows[row];
                        var myCell = myRow.c[col];

                        data.setCell(row, col, myCell.v);                        
                    }
                }

                var options = { 'width': 750, 'height': 450, 'title': 'Logging' };                

                var chart = new google.visualization.LineChart(document.getElementById('chart_div'));
                chart.draw(data, options);

            });

        }    

    </script>    

    <div id="chart_div"></div>

</asp:Content>
