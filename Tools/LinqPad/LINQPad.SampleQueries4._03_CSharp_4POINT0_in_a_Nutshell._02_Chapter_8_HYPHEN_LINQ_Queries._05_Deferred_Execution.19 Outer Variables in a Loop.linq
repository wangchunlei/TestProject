<Query Kind="Statements" />

IEnumerable<char> query = "Not what you might expect";

query = query.Where (c => c != 'a');
query = query.Where (c => c != 'e');
query = query.Where (c => c != 'i');
query = query.Where (c => c != 'o');
query = query.Where (c => c != 'u');

new string (query.ToArray()).Dump ("All vowels are stripped, as you'd expect.");


query = "Not what you might expect";

foreach (char vowel in "aeiou")
	query = query.Where (c => c != vowel);
  
new string (query.ToArray()).Dump ("Notice that only the 'u' is stripped!");