<Query Kind="Statements" />

foreach (string s in Regex.Split ("a5b7c", @"\d"))
	Console.Write (s + " ");
	
Console.WriteLine();
	
foreach (string s in Regex.Split ("oneTwoThree", @"(?=[A-Z])"))
	Console.Write (s + " ");