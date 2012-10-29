<Query Kind="Statements" />

var numbers = new[] { 1, 2, 3 }.AsQueryable();
var letters = new[] { "a", "b" }.AsQueryable();

IEnumerable<string> query =
	from n in numbers
	from l in letters
	select n.ToString() + l;
	
query.Dump();