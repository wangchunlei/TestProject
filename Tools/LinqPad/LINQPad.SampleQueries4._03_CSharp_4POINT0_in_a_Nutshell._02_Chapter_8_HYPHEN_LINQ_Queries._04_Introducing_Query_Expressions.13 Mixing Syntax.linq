<Query Kind="Statements" />

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

(from n in names where n.Contains ("a") select n).Count()
  .Dump ("Names containing the letter 'a'");

string first = (from n in names orderby n select n).First()
  .Dump ("First name, alphabetically");

names.Where (n => n.Contains ("a")).Count()
  .Dump ("Original query, entirely in fluent syntax");
  
names.OrderBy (n => n).First()
  .Dump ("Second query, entirely in fluent syntax");