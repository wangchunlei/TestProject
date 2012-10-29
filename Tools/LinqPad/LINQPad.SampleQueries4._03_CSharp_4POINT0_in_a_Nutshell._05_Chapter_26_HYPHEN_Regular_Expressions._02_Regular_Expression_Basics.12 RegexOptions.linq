<Query Kind="Statements" />

Regex.Match ("a", "A", RegexOptions.IgnoreCase).Value.Dump();
Regex.Match ("a", @"(?i)A").Value.Dump();

Regex.Match ("AAAa", @"(?i)a(?-i)a").Value.Dump();