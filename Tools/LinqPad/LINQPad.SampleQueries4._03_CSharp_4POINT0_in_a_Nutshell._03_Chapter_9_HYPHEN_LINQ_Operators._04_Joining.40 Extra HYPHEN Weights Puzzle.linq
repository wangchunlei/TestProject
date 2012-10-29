<Query Kind="Expression"/>

// Luke Hoban, from Microsoft, has found a way to solve a puzzle with a LINQ query!
// His solution is dramatically faster with Join than SelectMany:
//
// http://blogs.msdn.com/lukeh/archive/2007/03/19/using-linq-to-solve-puzzles.aspx
//
// Here's the query in LINQPad: 

from a in Enumerable.Range(1, 13)
join b in Enumerable.Range(1, 13) on 4 * a equals b
from c in Enumerable.Range(1, 13)
join d in Enumerable.Range(1, 13) on 5 * c equals d
from e in Enumerable.Range(1, 13)
join f in Enumerable.Range(1, 13) on 3 * e equals 2 * f  
join g in Enumerable.Range(1, 13) on 2 * (c + d) equals 3 * g
from h in Enumerable.Range(1, 13)
join i in Enumerable.Range(1, 13) on 3 * h - 2 * (e + f) equals 3 * i
from j in Enumerable.Range(1, 13)
join k in Enumerable.Range(1, 13) on 3 * (a + b) + 2 * j - 2 * (g + c + d) equals k
from l in Enumerable.Range(1, 13)
join m in Enumerable.Range(1, 13) on (h + i + e + f) - l equals 4 * m
where (4 * (l + m + h + i + e + f) == 3 * (j + k + g + a + b + c + d))
select new
{
	a, b, c, d, e, f, g, h, i, j, k, l, m,
    Total = a + b + c + d + e + f + g + h + i + j + k + l + m
}