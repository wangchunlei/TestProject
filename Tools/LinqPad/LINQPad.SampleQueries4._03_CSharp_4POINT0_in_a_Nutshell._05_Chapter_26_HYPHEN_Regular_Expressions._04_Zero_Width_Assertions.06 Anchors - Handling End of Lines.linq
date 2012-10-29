<Query Kind="Statements" />

string fileNames = "a.txt" + "\r\n" + "b.doc" + "\r\n" + "c.txt";
string r = @".+\.txt(?=\r?$)";

foreach (Match m in Regex.Matches (fileNames, r, RegexOptions.Multiline))
	Console.Write (m + " ");
