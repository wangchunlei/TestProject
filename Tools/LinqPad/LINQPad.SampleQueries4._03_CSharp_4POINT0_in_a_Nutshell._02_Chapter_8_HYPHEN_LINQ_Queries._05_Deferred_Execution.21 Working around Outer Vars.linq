<Query Kind="Statements" />

IEnumerable<char> query = "Not what you might expect";

foreach (char vowel in "aeiou")
{
	char temp = vowel;
	query = query.Where (c => c != temp);
}

query.Dump ("The workaround");