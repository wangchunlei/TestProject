<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

IEnumerable<string> query = names
	.Where   (n => n.Contains ("a"))
	.OrderBy (n => n.Length)
	.Select  (n => n.ToUpper());
	
query.Dump();

// The same query constructed progressively:

IEnumerable<string> filtered   = names.Where      (n => n.Contains ("a"));
IEnumerable<string> sorted     = filtered.OrderBy (n => n.Length);
IEnumerable<string> finalQuery = sorted.Select    (n => n.ToUpper());

filtered.Dump   ("Filtered");
sorted.Dump     ("Sorted");
finalQuery.Dump ("FinalQuery");