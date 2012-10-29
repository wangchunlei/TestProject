<Query Kind="Statements" />

var fullNames = new[] { "Anne Williams", "John Fred Smith", "Sue Green" }.AsQueryable();

IEnumerable<string> query =
	from fullName in fullNames                       // fullName = outer variable
	from name in fullName.Split()                    // name = iteration variable
	select name + " came from " + fullName;
	
query.Dump ("Both variables are in scope");