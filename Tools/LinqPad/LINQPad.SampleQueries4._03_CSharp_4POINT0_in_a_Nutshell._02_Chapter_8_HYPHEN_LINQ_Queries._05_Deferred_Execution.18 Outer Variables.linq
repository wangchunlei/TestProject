<Query Kind="Statements" />

int[] numbers = { 1, 2 };

int factor = 10;
IEnumerable<int> query = numbers.Select (n => n * factor);

factor = 20;

query.Dump ("Notice both numbers are multiplied by 20, not 10");