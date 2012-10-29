<Query Kind="Statements" />

int[] numbers  = { 1, 2, 3, 4, 5 };

try
{
	numbers.Single (n => n > 10);
}
catch (Exception ex)
{
	ex.Message.Dump ("The Single number > 10");
}

numbers.SingleOrDefault (n => n > 10).Dump ("The SingleOrDefault number > 10");

try
{
	numbers.SingleOrDefault (n => n % 2 == 0);
}
catch (Exception ex)
{
	ex.Message.Dump ("The SingleOrDefault number divisible by two");
}
