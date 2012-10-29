<Query Kind="Statements" />

XElement items =
	new XElement ("items",
		new XElement ("one"),
		new XElement ("three")
	);

items.Dump ("Original XML");
		
items.FirstNode.AddAfterSelf (new XElement ("two"));

items.Dump ("After calling items.FirstNode.AddAfterSelf");