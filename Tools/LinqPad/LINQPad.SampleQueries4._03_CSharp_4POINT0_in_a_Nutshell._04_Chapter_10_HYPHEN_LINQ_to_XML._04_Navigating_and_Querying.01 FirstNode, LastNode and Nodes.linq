<Query Kind="Statements" />

var bench =
	new XElement ("bench",
		new XElement ("toolbox",
			new XElement ("handtool", "Hammer"),
			new XElement ("handtool", "Rasp")
		),
		new XElement ("toolbox",
			new XElement ("handtool", "Saw"), 
			new XElement ("powertool", "Nailgun")
		),
		new XComment ("Be careful with the nailgun")
	);

bench.FirstNode.Dump ("FirstNode");
bench.LastNode.Dump ("LastNode");
		
foreach (XNode node in bench.Nodes())
	Console.WriteLine (node.ToString (SaveOptions.DisableFormatting) + ".");