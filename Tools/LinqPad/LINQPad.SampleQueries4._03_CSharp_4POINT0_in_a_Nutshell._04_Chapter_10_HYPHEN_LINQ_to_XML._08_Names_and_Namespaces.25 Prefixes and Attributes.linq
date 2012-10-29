<Query Kind="Statements" />

XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
var nil = new XAttribute (xsi + "nil", true);

var cust =
	new XElement ("customers",
		new XAttribute (XNamespace.Xmlns + "xsi", xsi),
		new XElement ("customer",
			new XElement ("lastname", "Bloggs"),
			new XElement ("dob", nil),
			new XElement ("credit", nil)
		)
	);

cust.Dump();