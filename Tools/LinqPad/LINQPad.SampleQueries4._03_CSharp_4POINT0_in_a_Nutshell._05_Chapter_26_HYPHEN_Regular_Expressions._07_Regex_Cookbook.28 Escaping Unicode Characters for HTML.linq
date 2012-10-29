<Query Kind="Statements" />

string htmlFragment = "© 2007";

string result = Regex.Replace (
	htmlFragment,
	@"[\u0080-\uFFFF]",
	m => @"&#" + ((int)m.Value[0]).ToString() + ";");

Console.WriteLine (result);