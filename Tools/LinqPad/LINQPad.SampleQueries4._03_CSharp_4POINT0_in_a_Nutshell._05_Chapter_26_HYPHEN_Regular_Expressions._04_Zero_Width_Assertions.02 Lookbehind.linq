<Query Kind="Statements" />

string regex = "(?i)(?<!however.*)good";

Regex.IsMatch ("However good, we...", regex).Dump();
Regex.IsMatch ("Very good, thanks!" , regex).Dump();