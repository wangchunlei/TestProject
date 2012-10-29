<Query Kind="Statements" />

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

names.OrderBy (s => s.Length).ThenBy (s => s)
	.Dump ("By length, then alphabetically");
	
names.OrderBy (s => s.Length).ThenBy (s => s[1]).ThenBy (s => s[0])
	.Dump ("By length, then second character, then first character");

(
	from s in names
	orderby s.Length, s[1], s[0]
	select s
)
.Dump ("Same query in query syntax");