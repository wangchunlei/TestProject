<Query Kind="Statements" />

XNamespace ns = "http://domain.com/xmlspace";

var data =
	new XElement (ns + "data",
		new XAttribute (ns + "id", 123)
	);
	
data.Dump();