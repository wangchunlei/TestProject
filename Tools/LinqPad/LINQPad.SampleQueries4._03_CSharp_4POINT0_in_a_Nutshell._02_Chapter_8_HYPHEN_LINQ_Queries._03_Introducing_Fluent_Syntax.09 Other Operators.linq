<Query Kind="Statements" />

int[] numbers = { 10, 9, 8, 7, 6 };

"".Dump ("All of these operators are covered in more detail in Chapter 9.");

// Element operators:

numbers.First().Dump ("First");
numbers.Last().Dump ("Last");

numbers.ElementAt (1).Dump ("Second number");
numbers.OrderBy (n => n).First().Dump ("Lowest number");

// Aggregation operators:

numbers.Count().Dump ("Count");
numbers.Min().Dump ("Min");

// Quantifiers:

numbers.Contains (9).Dump ("Contains (9)");
numbers.Any().Dump ("Any");
numbers.Any (n => n % 2 == 1).Dump ("Has an odd numbered element");

// Set based operators:

int[] seq1 = { 1, 2, 3 };
int[] seq2 = { 3, 4, 5 };
seq1.Concat (seq2).Dump ("Concat");
seq1.Union (seq2).Dump ("Union");