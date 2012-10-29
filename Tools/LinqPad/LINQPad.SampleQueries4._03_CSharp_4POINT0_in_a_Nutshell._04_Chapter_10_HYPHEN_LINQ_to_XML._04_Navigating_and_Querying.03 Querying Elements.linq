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

var toolboxWithNailgun =
	from toolbox in bench.Elements()
	where toolbox.Elements().Any (tool => tool.Value == "Nailgun")
	select toolbox.Value;
	
var handTools =
	from toolbox in bench.Elements()
	from tool in toolbox.Elements()
	where tool.Name == "handtool"
	select tool.Value;

int toolboxCount = bench.Elements ("toolbox").Count();

var handTools2 =
	from tool in bench.Elements ("toolbox").Elements ("handtool")
	select tool.Value.ToUpper();

toolboxWithNailgun.Dump ("The toolbox with the nailgun");
handTools.Dump ("The hand tools in all toolboxes");
toolboxCount.Dump ("Number of toolboxes");
handTools2.Dump ("The hand tools in all toolboxes");