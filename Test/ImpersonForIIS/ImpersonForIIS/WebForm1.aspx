<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="ImpersonForIIS.WebForm1" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>WebForm1</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio 7.0">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<P>&nbsp;</P>
			<P>Enter Path of process to be run (with relevant parameters)
				<asp:TextBox id="TextBox1" runat="server"></asp:TextBox></P>
			<P>
				<asp:Button id="Button1" runat="server" Text="CreateProcess" OnClick="Button1_Click"></asp:Button>
			</P>
			<P>
				<asp:Label id="Label1" runat="server">Status:</asp:Label></P>
			<P>
				<asp:Label id="Label2" runat="server">Impersonated Identity:</asp:Label></P>
		</form>
	</body>
</HTML>