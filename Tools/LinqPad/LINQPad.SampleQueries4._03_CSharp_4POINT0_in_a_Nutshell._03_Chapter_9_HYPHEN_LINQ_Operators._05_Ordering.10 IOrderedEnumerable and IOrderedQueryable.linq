<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

IOrderedEnumerable<string> query1 = names.OrderBy (s => s.Length);
IOrderedEnumerable<string> query2 = query1.ThenBy (s => s);

query2.Dump();

var query = names.OrderBy (s => s.Length).AsEnumerable();
query = query.Where (n => n.Length > 3);

query.Dump();