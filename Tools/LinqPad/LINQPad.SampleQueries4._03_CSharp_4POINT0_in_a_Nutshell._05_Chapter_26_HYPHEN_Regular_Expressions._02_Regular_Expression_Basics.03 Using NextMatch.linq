<Query Kind="Statements" />

Match m1 = Regex.Match ("One color? There are two colours in my head!", @"colou?rs?");
Match m2 = m1.NextMatch();

m1.Dump ("Match 1");
m2.Dump ("Match 2");
