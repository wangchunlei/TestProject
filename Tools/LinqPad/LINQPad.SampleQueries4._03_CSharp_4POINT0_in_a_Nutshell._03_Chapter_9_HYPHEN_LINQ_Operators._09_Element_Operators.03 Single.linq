<Query Kind="Statements" />

int[] numbers  = { 1, 2, 3, 4, 5 };

numbers.Single (n => n % 3 == 0).Dump ("The Single number divisible by 3");

try
{ 
	numbers.Single (n => n % 2 == 0);
}
catch (Exception ex)
{
	ex.Message.Dump ("The Single number divisible by 2");
}
