<Query Kind="Statements" />

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

(
	names
	.Select  (n => n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", ""))
	.Where   (n => n.Length > 2)
	.OrderBy (n => n)
)
.Dump ("This query was written in fluent syntax");

(
	from    n in names
	where   n.Length > 2
	orderby n
	select  n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", "")
)
.Dump ("An incorrect translation to query syntax");

IEnumerable<string> query =
	from   n in names
	select n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", "");

query = from n in query where n.Length > 2 orderby n select n;

query.Dump ("A correct translation to query syntax, querying in two steps");