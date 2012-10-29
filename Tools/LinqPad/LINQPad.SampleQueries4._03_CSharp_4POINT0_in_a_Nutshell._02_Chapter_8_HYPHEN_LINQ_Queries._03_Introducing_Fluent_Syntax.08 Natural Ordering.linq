<Query Kind="Statements" />

int[] numbers  = { 10, 9, 8, 7, 6 };

// The natural ordering of numbers is honored, making the following queries possible:

numbers.Take (3)  .Dump ("Take(3) returns the first three numbers in the sequence");
numbers.Skip (3)  .Dump ("Skip(3) returns all but the first three numbers in the sequence");
numbers.Reverse() .Dump ("Reverse does exactly as it says");