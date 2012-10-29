<Query Kind="Expression" />

new XElement ("customer",	new XAttribute ("id", 123),
	new XElement ("firstname", "joe"),
	new XElement ("lastname", "bloggs",
		new XComment ("nice name")
	)
)