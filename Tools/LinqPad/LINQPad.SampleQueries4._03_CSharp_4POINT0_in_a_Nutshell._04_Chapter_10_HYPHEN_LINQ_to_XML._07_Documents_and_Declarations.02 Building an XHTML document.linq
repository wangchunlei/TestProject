<Query Kind="Statements" />

var styleInstruction = new XProcessingInstruction (
	"xml-stylesheet", "href='styles.css' type='text/css'"
);

var docType = new XDocumentType ("html",
  "-//W3C//DTD XHTML 1.0 Strict//EN",
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);

XNamespace ns = "http://www.w3.org/1999/xhtml";
var root =
	new XElement (ns + "html",
		new XElement (ns + "head",
			new XElement (ns + "title", "An XHTML page")),
		new XElement (ns + "body",
			new XElement (ns + "h1", "This is a heading."),
			new XElement (ns + "p", "This is some content."))
	);
	
var doc =
	new XDocument (
		new XDeclaration ("1.0", "utf-8", "no"),
		new XComment ("Reference a stylesheet"),
		styleInstruction,
		docType,
		root
	);

string tempPath = Path.Combine (Path.GetTempPath(), "sample.html");
doc.Save (tempPath);
Process.Start (tempPath);                      // This will display the page in IE or FireFox
File.ReadAllText (tempPath).Dump();

doc.Root.Name.LocalName.Dump ("Root element's local name");

XElement bodyNode = doc.Root.Element (ns + "body");
(bodyNode.Document == doc).Dump ("bodyNode.Document == doc");

(doc.Root.Parent == null).Dump ("doc.Root.Parent is null");

foreach (XNode node in doc.Nodes())
  Console.Write (node.Parent == null);