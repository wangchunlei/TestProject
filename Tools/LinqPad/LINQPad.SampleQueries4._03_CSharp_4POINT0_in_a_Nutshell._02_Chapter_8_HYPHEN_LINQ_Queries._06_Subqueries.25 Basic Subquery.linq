<Query Kind="Statements" />

string[] musos = { "Roger Waters", "David Gilmour", "Rick Wright" };

musos.OrderBy (m => m.Split().Last())   .Dump ("Sorted by last name");