<Query Kind="Statements" />

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();
(
	from n in names
	let vowelless = n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", "")
	where vowelless.Length > 2
	orderby vowelless
	select n		       // Thanks to let, n is still in scope.
)
.Dump();