<Query Kind="Statements" />

var fullNames = new[] { "Anne Williams", "John Fred Smith", "Sue Green" }.AsQueryable();

IEnumerable<string[]> query1 = fullNames.Select     (name => name.Split());
IEnumerable<string>   query2 = fullNames.SelectMany (name => name.Split());

query1.Dump ("A simple Select gives us a hierarchical result set");
query2.Dump ("SelectMany gives us a flat result set");

IEnumerable<string> query3 = 
	from fullName in fullNames
	from name in fullName.Split()     // Translates to SelectMany
	select name;

query3.Dump ("Same SelectMany query, but in query syntax");