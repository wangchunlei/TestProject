<Query Kind="Statements" />

foreach (Match m in Regex.Matches ("One color? There are two colours in my head!", @"colou?rs?"))
	m.Value.Dump();
