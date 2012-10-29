<Query Kind="Statements" />

// (Without cheating, by looking at the lambda tab!)

var fullNames = new[] { "Anne Williams", "John Fred Smith", "Sue Green" }.AsQueryable();

IEnumerable<string> query =
	from fullName in fullNames
	from name in fullName.Split()
	orderby fullName, name
	select name + " came from " + fullName;
	
query.Dump();