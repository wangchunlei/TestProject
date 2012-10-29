<Query Kind="Statements" />

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

var query =
	from n in names
	where n.Length > 3
	let u = n.ToUpper()
	where u.EndsWith ("Y")
	select u;
	
query.Dump();