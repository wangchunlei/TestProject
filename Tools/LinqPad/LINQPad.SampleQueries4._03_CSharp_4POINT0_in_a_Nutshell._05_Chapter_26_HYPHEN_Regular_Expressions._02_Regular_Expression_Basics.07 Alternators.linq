<Query Kind="Statements" />

string r = "Jen(ny|nifer)?";

Regex.IsMatch ("Jenny", r).Dump();
Regex.IsMatch ("Jennifer", r).Dump();
Regex.IsMatch ("Jen", r).Dump();
Regex.IsMatch ("Ben", r).Dump();