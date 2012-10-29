<Query Kind="Statements" />

XElement items = XElement.Parse (@"
<items>
	<one/><two/><three/>	
</items>");

items.Dump ("Original XML");

items.FirstNode.ReplaceWith (new XComment ("One was here"));

items.Dump ("After calling ReplaceWith");