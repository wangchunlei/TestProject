<Query Kind="Statements" />

decimal[] numbers  = { 3, 4, 8 };

numbers.Sum()      .Dump ("Sum");
numbers.Average()  .Dump ("Average (mean)");

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };
names.Sum (s => s.Length).Dump ("Combined string lengths");