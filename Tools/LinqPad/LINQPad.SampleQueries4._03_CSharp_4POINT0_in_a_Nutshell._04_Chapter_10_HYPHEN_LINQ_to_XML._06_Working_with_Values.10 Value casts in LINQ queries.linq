<Query Kind="Statements" />

var data = XElement.Parse (@"
<data>
	<customer id='1' name='Mary' credit='100' />
	<customer id='2' name='John' credit='150' />
	<customer id='3' name='Anne' />
</data>");
	
IEnumerable<string> query =
	from cust in data.Elements()
	where (int?) cust.Attribute ("credit") > 100
	select cust.Attribute ("name").Value;
	
query.Dump();