<Query Kind="Expression" />

from n in new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable()
group n by new
{
	FirstLetter = n[0],
	Length = n.Length
}