<Query Kind="Statements" />

XNamespace ns = "http://domain.com/xmlspace";

var data =
	new XElement (ns + "data",
		new XElement ("customer", "Bloggs"),
		new XElement ("purchase", "Bicycle")
	);
	
data.Dump ("Before");
	
foreach (XElement e in data.DescendantsAndSelf())
	if (e.Name.Namespace == "")
		e.Name = ns + e.Name.LocalName;

data.Dump ("After");