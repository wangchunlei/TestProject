<Query Kind="Statements" />

var doc =
	new XDocument (
		new XDeclaration ("1.0", "utf-16", "yes"),
		new XElement ("test", "data")
	);

string tempPath = Path.Combine (Path.GetTempPath(), "test.xml");
doc.Save (tempPath);
File.ReadAllText (tempPath).Dump();
