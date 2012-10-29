<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

IEnumerable<string> query =
	names.Where (name => name.EndsWith ("y"));

query.Dump ("In fluent syntax");

query =
	from n in names
	where n.EndsWith ("y")
	select n;
	
query.Dump ("In query syntax");