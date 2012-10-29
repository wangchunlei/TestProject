<Query Kind="Statements" />

XElement config = XElement.Parse (
@"<configuration>
	<client enabled='true'>
		<timeout>30</timeout>
	</client>
</configuration>");

foreach (XElement child in config.Elements())
	child.Name.Dump ("Child element name");

XElement client = config.Element ("client");

bool enabled = (bool) client.Attribute ("enabled");   // Read attribute
enabled.Dump ("enabled attribute");

client.Attribute ("enabled").SetValue (!enabled);     // Update attribute

int timeout = (int) client.Element ("timeout");       // Read element
timeout.Dump ("timeout element");

client.Element ("timeout").SetValue (timeout * 2);    // Update element

client.Add (new XElement ("retries", 3));             // Add new elememt

config.Dump ("Updated DOM");