<Query Kind="Statements" />

string r = @"(?=[A-Z])";

foreach (string s in Regex.Split ("oneTwoThree", r))
	Console.Write (s + " ");