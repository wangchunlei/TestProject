<Query Kind="Statements" />

int[][] numbers = 
{
  new int[] { 1, 2, 3 },
  new int[] { 4, 5, 6 },
  null                     // this null makes the query below fail.
};

IEnumerable<int> flat = numbers.SelectMany (innerArray => innerArray);

flat.Dump();          // Throws a NullReferenceException