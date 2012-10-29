<Query Kind="Statements" />

int[] numbers  = { 1, 2, 3, 4, 5 };

numbers.First().Dump ("First");
numbers.Last().Dump ("Last");

numbers.First (n => n % 2 == 0).Dump ("First even number");
numbers.Last (n => n % 2 == 0).Dump ("Last even number");

try 
{
	numbers.First (n => n > 10);
}
catch (Exception ex) 
{
	ex.Message.Dump ("The First number > 10");
}

