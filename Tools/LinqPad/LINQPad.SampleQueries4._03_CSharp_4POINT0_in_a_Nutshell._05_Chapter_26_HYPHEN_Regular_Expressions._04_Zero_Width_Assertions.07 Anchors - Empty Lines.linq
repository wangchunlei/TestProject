<Query Kind="Statements" />

string s = @"The
second to last line

has some

spaces
   
in it!";

MatchCollection emptyLines = Regex.Matches (s, "^(?=\r?$)", RegexOptions.Multiline);
emptyLines.Count.Dump();

MatchCollection blankLines = Regex.Matches (s, "^[ \t]*(?=\r?$)", RegexOptions.Multiline);
blankLines.Count.Dump();