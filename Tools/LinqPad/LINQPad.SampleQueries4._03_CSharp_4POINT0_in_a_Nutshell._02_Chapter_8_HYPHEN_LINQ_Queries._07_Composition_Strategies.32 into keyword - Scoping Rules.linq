<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

// The following will not compile - "The name 'n1' does not exist in the current context" (try it).

var query =
	from   n1 in names
	select n1.ToUpper()
	into   n2                          // Only n2 is visible from here on.
	where  n1.Contains ("x")           // Illegal: n1 is not in scope.
	select n2;
		
// The equivalent in fluent syntax (you wouldn't expect this to compile!):

var query = names
	.Select (n1 => n1.ToUpper())
	.Where (n2 => n1.Contains ("x"));     // Error: n1 no longer in scope				