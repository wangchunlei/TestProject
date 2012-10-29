<Query Kind="Statements" />

int[] numbers = { 1, 2, 3 };
numbers.Aggregate (0, (seed, n) => seed + n).Dump();

