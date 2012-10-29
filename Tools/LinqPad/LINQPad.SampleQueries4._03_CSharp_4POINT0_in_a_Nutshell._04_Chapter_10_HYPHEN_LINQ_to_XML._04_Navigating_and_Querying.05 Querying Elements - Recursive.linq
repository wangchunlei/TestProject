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

bench.Descendants ("handtool").Count().Dump ("Count of all handtools");

foreach (XNode node in bench.DescendantNodes())
	Console.WriteLine (node.ToString (SaveOptions.DisableFormatting));

(  
	from c in bench.DescendantNodes().OfType<XComment>()
	where c.Value.Contains ("careful")
	orderby c.Value
	select c.Value
)
.Dump ("Comments anywhere in the X-DOM containing the word 'careful'");