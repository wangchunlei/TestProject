<Query Kind="Statements" />

XElement settings =
	new XElement ("settings",
		new XElement ("timeout", 30)
	);

settings.Dump ("Original XML");	

settings.SetValue ("blah");
settings.Dump ("Notice the timeout node has disappeared");