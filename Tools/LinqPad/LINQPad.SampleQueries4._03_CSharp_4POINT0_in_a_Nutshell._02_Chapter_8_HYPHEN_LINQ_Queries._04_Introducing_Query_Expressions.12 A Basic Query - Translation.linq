<Query Kind="Statements" />

// With AsQueryable() added, you can see the translation to fluent syntax in the λ tab below:

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

IEnumerable<string> query =
	from      n in names
	where     n.Contains ("a")    // Filter elements
	orderby   n.Length            // Sort elements
	select    n.ToUpper();        // Translate each element (project)
	
query.Dump();