<Query Kind="Statements" />

XNamespace ns = "http://domain.com/xmlspace";

var data =
	new XElement (ns + "data",
		new XElement (ns + "customer", "Bloggs"),
		new XElement (ns + "purchase", "Bicycle")
	);	
	
data.Dump ("The whole DOM");

data.Element (ns + "customer").Dump ("The customer element (notice namespace is now present)");