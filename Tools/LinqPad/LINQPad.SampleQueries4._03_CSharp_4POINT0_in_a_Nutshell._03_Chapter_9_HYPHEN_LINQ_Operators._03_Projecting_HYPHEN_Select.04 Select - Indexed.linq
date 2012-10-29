<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

names.Select ((s,i) => i + "=" + s)  .Dump();