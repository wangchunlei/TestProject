<Query Kind="Statements" />

Match m = Regex.Match ("206-465-1918", @"(\d{3})-(\d{3}-\d{4})");

m.Groups[0].Value.Dump();
m.Groups[1].Value.Dump();
m.Groups[2].Value.Dump();

Console.WriteLine();

foreach (Match ma in Regex.Matches ("pop pope peep", @"\b(\w)\w+\1\b"))
	Console.Write (ma + " ");