<Query Kind="Statements" />

Regex r = new Regex (@"sausages?");

r.Match ("sausage").Success.Dump();
r.Match ("sausages").Success.Dump();