<Query Kind="Statements" />

int[] numbers = { 3, 5, 2, 234, 4, 1 };

numbers.TakeWhile (n => n < 100).Dump ("TakeWhile");
numbers.SkipWhile (n => n < 100).Dump ("SkipWhile");