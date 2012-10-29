<Query Kind="Statements" />

int[] numbers = { 28, 32, 14 };

numbers.Min().Dump ("Min");
numbers.Max().Dump ("Max");

numbers.Max (n => n % 10).Dump ("Maximum remainder after dividing by ten");
