<Query Kind="Statements" />

// Query syntax provides a shortcut for using the Cast operator on the 
// input sequence. You simply include the type name directly after the from clause:

object[] untyped = { 1, 2, 3 };

var query1 =
	from i in untyped.Cast<int>()      // Without syntactic shortcut
	select i * 10;

var query2 =
	from int i in untyped       // Notice we've slipped in "int"
	select i * 10;

query1.Dump ("Explicitly calling Cast operator");
query2.Dump ("Syntactic shortcut for same query");