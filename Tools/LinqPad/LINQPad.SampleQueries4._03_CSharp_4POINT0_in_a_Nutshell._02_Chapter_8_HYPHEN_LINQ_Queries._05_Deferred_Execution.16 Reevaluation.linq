<Query Kind="Statements" />

var numbers = new List<int>() { 1, 2 };

IEnumerable<int> query = numbers.Select (n => n * 10);

query.Dump ("Both elements are returned");

numbers.Clear();

query.Dump ("All the elements are now gone!");