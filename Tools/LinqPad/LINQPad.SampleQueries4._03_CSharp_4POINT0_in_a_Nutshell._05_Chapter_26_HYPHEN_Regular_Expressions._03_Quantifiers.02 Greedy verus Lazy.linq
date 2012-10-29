<Query Kind="Statements" />

string html = "<i>By default</i> quantifiers are <i>greedy</i> creatures";

foreach (Match m in Regex.Matches (html, @"<i>.*</i>"))
	m.Value.Dump ("Greedy");
	
foreach (Match m in Regex.Matches (html, @"<i>.*?</i>"))
	m.Value.Dump ("Lazy");