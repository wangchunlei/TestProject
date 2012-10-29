<Query Kind="Statements" />

Regex.Match ("color",   @"colou?r").Success.Dump();
Regex.Match ("colour",  @"colou?r").Success.Dump();
Regex.Match ("colouur", @"colou?r").Success.Dump();