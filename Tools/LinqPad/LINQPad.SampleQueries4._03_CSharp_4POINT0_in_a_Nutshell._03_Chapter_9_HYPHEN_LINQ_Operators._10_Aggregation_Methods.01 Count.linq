<Query Kind="Statements" />

// Note: refer to 'Grouping' for examples on using aggregations in groupby clauses.

new int[] { 5, 6, 7 }.Count()
	.Dump ("Simple Count");

"pa55w0rd".Count (c => char.IsDigit (c))
	.Dump ("Count with predicate");