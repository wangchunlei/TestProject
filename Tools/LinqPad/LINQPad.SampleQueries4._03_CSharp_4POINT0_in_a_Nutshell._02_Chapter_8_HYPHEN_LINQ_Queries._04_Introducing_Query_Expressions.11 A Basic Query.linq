<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

IEnumerable<string> query =
	from     n in names
	where    n.Contains ("a")   // Filter elements
	orderby  n.Length           // Sort elements
	select   n.ToUpper();       // Translate each element (project)
	
query.Dump();