<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Search
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<script type="text/javascript">
    $(function () {
        $('.datePicker').datepicker({
            dateFormat: "yy-mm-dd"
        });
    });
</script>


    <h2>Search</h2>

    <div class="grid-filter">        
        <div class="inner">

        <table>
            <tr>
                <td>Logger</td>
                <td><%: Html.DropDownList("LoggerProviderName", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.LogProviderNames, "Value", "Text"))%> </td>
            </tr>
            <tr>
                <td>Level</td>
                <td><%: Html.DropDownList("LogLevel", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.LogLevels, "Value", "Text"))%>  </td>
            </tr>
            <tr>
                <td>For</td>
                <td>
                    <input type="radio" id="dateOption1" name="dateoption" /> <%: Html.DropDownList("Period", new SelectList(MvcLoggingDemo.Helpers.FormsHelper.CommonTimePeriods, "Value", "Text"))%>                    
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <input type="radio" id="dateOption2" name="dateoption" /> 
                    <input style="width: 85px;" class="datePicker" type="text" id="txtDateStart" name="txtDateStart" /> and <input style="width: 85px;" class="datePicker" type="text" id="txtDateEnd" name="txtDateEnd" />
                </td>
            </tr>
            <tr>
                <td>Search criteria</td>
                <td>
                    <textarea id="criteria" name="criteria" rows="3" cols="60"></textarea>
                </td>
            </tr>
            <tr>
                <td></td>
                <td style="text-align: right;">
                    <input id="btnGo" name="btnGo" type="submit" value="Search" />                      
                </td>
            </tr>
        </table>
        
        

        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeadArea" runat="server">
</asp:Content>
