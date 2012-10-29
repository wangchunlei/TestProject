<Query Kind="Statements" />

IEnumerable<string> query = "one two two three".Split().Distinct();

string[] toArray = query.ToArray();
List<string> toList = query.ToList();

toArray.Dump(); 
toList.Dump();

