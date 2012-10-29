<Query Kind="Statements" />

// In the book, we use the following array to demonstrate simple local queries:
//
//    string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

// In the LINQPad examples, we've changed this in some places to the following:

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

// This doesn't change the result of the query. The effect it has it to populate 
// the λ tab below—so you can see how the query translates into lambda (fluent) syntax.
// To illustrate, press F5 to run the following:
(
	from n in names
	where n.Length > 3
	orderby n descending
	select n.ToUpper()
)
.Dump ("Click the λ button - notice the translation to fluent syntax");

// In Chapter 8, we'll explain how this query works—and the difference between
// query syntax and fluent syntax. Later, we'll go over the difference
// between local and interpreted queries, and explain the role of AsQueryable().
