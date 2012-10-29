<Query Kind="Statements" />

var players = new[] { "Tom", "Jay", "Mary" }.AsQueryable();

IEnumerable<string> query =
	from name1 in players
	from name2 in players
	where name1.CompareTo (name2) < 0
	orderby name1, name2
	select name1 + " vs " + name2;

query.Dump();