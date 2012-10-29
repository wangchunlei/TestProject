<Query Kind="Statements" />

// Here's the preceding query, with the 'foreach' statement expanded into what the compiler emits:

IEnumerable<char> query = "Not what you might expect";
IEnumerable<char> vowels = "aeiou";

IEnumerator<char> rator = vowels.GetEnumerator ();
char vowel;
while (rator.MoveNext())
{
	vowel = rator.Current;
	query = query.Where (c => c != vowel);
}

query.Dump();
