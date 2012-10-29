<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

int shortest = names.Min (n => n.Length);
(
	from    n in names
	where   n.Length == shortest
	select  n
)
.Dump ("No subquery");
	