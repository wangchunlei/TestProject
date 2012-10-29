<Query Kind="Statements" />

var numbers = new List<int>();
numbers.Add (1);

IEnumerable<int> query = numbers.Select (n => n * 10);    // Build query

numbers.Add (2);                    // Sneak in an extra element

query.Dump ("Notice both elements are returned in the result set");