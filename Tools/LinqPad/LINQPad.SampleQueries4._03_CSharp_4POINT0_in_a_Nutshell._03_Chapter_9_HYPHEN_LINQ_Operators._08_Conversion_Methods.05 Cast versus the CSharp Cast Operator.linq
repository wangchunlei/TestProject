<Query Kind="Statements" />

int[] integers = { 1, 2, 3 };

IEnumerable<long> test1 = integers.OfType<long>();
test1.Dump ("OfType returns no results");

IEnumerable<long> test2 = integers.Cast<long>();
test2.Dump ("Cast returns a sequence of three long integers!");

// Here's an alternative approach, using a projection:

integers.Select (s => (long) s).Dump ("The correct approach");