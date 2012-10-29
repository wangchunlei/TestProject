<Query Kind="Statements" />

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

IEnumerable<string> query =
	from n1 in 
	(
		from   n2 in names
		select n2.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", "")
	)
	where n1.Length > 2 orderby n1 select n1;
  
query.Dump ("Here, one query wraps another");

var sameQuery = names
	.Select  (n => n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", ""))
	.Where   (n => n.Length > 2)
	.OrderBy (n => n);
	
sameQuery.Dump ("In fluent syntax, such queries translate to a linear chain of query operators");