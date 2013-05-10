<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.DateTime>" %>

<%: Html.TextBox("", String.Format("{0:yyyy-MM-dd}", Model), new { @class = "datePicker" })%>
<script type="text/javascript">
    $(function () {
        $('.datePicker').datepicker({
            dateFormat: "yy-mm-dd"
        });
});
</script>