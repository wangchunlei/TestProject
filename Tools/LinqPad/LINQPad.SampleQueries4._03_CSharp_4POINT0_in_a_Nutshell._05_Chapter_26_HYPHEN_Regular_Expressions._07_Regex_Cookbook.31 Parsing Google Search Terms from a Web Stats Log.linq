<Query Kind="Statements" />

string sample = "http://www.google.com/search?hl=en&q=greedy+quantifiers+regex&btnG=Search";

Match m = Regex.Match (sample, @"(?<=google\..+search\?.*q=).+?(?=(&|$))");

string[] keywords = m.Value.Split (new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
keywords.Dump();

// Note: this may need to be used in conunction with the previous
// example, i.e. "Unescaping Characters in an HTTP Query String".