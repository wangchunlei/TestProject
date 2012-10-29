<Query Kind="Statements" />

var numbers = new List<int>() { 1, 2 };

List<int> timesTen = numbers
  .Select (n => n * 10) 
  .ToList();                      // Executes immediately into a List<int>

numbers.Clear();
timesTen.Count.Dump ("Still two elements present");