<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

names.Where ((n, i) => i % 2 == 0).Dump ("Skipping every second element");