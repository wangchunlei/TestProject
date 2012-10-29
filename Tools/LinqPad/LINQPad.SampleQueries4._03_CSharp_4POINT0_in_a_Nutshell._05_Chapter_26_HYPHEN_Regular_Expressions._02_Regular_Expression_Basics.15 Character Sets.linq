<Query Kind="Statements" />

Regex.Matches ("That is that.", "[Tt]hat").Count
	.Dump ("Matches any of a set");
	
Regex.Match ("quiz qwerty", "q[^aeiou]").Index
	.Dump ("Matches any except those of a set");
	
Regex.Match ("b1-c4", @"[a-h]\d-[a-h]\d").Success
	.Dump ("Matches a range");
	
Regex.IsMatch ("Yes, please", @"\p{P}")
	.Dump ("Matches character category");