<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

names.Select (n => n.Length).Dump ("Notice result is IEnumerable<Int32>; Int32 is inferred");

IEnumerable<string> sortedByLength, sortedAlphabetically;

names.OrderBy (n => n.Length)  .Dump ("Integer sorting key");
names.OrderBy (n => n)         .Dump ("String sorting key");
