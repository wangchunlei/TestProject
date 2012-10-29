<Query Kind="Statements" />

string find = @"\bcat\b";
string replace = "dog";
Regex.Replace ("catapult the cat", find, replace).Dump();