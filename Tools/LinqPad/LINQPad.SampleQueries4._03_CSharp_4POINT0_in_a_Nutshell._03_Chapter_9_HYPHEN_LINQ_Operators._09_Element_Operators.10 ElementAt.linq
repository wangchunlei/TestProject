<Query Kind="Statements" />

int[] numbers  = { 1, 2, 3, 4, 5 };

numbers.ElementAt (2).Dump ("ElementAt (2)");
try
{
	numbers.ElementAt (9); 
}
catch (Exception ex)
{
	ex.Message.Dump ("ElementAt (9)");
}

numbers.ElementAtOrDefault (9).Dump ("ElementAtOrDefault (9)");