<Query Kind="Statements" />

XNamespace ns = "http://domain.com/xmlspace";

var data =
	new XElement (ns + "data",
		new XElement ("customer", "Bloggs"),
		new XElement ("purchase", "Bicycle")
	);
	
data.Dump ("Forgetting to specify namespaces in construction");

data = 
	new XElement (ns + "data",
		new XElement (ns + "customer", "Bloggs"),
		new XElement (ns + "purchase", "Bicycle")
	);
	
XElement x = data.Element (ns + "customer");		// OK
XElement y = data.Element ("customer");

y.Dump ("Forgetting to specify a namespace when querying");