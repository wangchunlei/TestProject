<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

names.OrderBy (s => s).Dump ("Ordered alphabetically");
names.OrderBy (s => s.Length).Dump ("Ordered by length");