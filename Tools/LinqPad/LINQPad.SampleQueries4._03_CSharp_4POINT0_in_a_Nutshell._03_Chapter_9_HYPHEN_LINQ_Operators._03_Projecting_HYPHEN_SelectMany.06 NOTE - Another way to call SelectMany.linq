<Query Kind="Statements" />

// SelectMany is overloaded to help you (slightly!) with queries that perform a Select
// within SelectMany's lambda expression. To illustrate, consider the following query:

string[] fullNames = { "Anne Williams", "John Fred Smith", "Sue Green" };

var query1 =
	fullNames
	.SelectMany (fName => fName.Split().Select (name => new { name, fName } ));

// We can re-write this as follows, and get the same result:

var query2 =
	fullNames
	.SelectMany (fName => fName.Split(), (fName, name) => new { name, fName } );
	
query1.Dump ("Using SelectMany+Select");
query2.Dump ("Using SelectMany with a collection selector + result selector");

// Instead of performing a Select inside the SelectMany, we supply SelectMany with two
// lambda expressions:
//   (a) the collection selector (which would otherwise come just before the .Select)
//   (b) the result selector (which would otherwise be fed into the .Select).
//
// C#, in the current release, favours this version of SelectMany when translating
// query syntax queries. Functionally, it has the same effect as performing
// a Select within a SelectMany.