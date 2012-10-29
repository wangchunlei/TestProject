<Query Kind="Statements" />

foreach (Match m in Regex.Matches ("Wedding in Sarajevo", @"\b\w+\b"))
	m.Value.Dump();

Regex.Matches ("Wedding in Sarajevo", @"\bin\b").Count.Dump ("With the word boundary operator");
Regex.Matches ("Wedding in Sarajevo", @"in").Count.Dump ("Without the word boundary operator");

string text = "Don't loose (sic) your cool";
Regex.Match (text, @"\b\w+\b\s(?=\(sic\))").Value.Dump();