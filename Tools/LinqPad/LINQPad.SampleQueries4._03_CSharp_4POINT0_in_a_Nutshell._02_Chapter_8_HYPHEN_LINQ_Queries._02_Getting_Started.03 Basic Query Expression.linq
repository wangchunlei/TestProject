<Query Kind="Expression" />

from n in new[] { "Tom", "Dick", "Harry" }
where n.Contains ("a")
select n