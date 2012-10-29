<Query Kind="Statements" />

XElement summary =
	new XElement ("summary",
		new XText ("An XAttribute is "),
		new XElement ("bold", "not"),
		new XText (" an XNode")
	);

summary.Dump();