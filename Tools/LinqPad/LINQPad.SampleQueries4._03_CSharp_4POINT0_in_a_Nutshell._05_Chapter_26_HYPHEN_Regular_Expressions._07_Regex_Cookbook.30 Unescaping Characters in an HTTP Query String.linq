<Query Kind="Statements" />

string sample = "C%23 in a Nutshell";

string result = Regex.Replace (
	sample,
	@"%[0-9a-f][0-9a-f]", 
	m => ((char) Convert.ToByte (m.Value.Substring (1), 16)).ToString(),
	RegexOptions.IgnoreCase
);

Console.WriteLine (result);