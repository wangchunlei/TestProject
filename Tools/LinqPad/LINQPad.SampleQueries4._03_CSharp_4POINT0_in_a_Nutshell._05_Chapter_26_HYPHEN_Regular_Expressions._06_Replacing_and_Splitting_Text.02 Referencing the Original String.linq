<Query Kind="Statements" />

string text = "10 plus 20 makes 30";
Regex.Replace (text, @"\d+", @"<$0>").Dump();