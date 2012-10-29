<Query Kind="Statements" />

// This replaces \n with \r\n without breaking existing \r\n occurrences.

string n = "\n";
string rn = "\r\n";
string text = "L1" + n + "L2" + rn + "L3";

string result = Regex.Replace (text, "(?<!\r)\n", "\r\n");

result.Select (c => new { c, Code = (int) c } ).Dump();