<Query Kind="Statements" />

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

var intermediate = from n in names
select new
{
	Original = n,
	Vowelless = n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", "")
};

(
	from    item in intermediate
	where   item.Vowelless.Length > 2
	select  item.Original
)
.Dump();

// With the into keyword we can do this in one step:

(
	from n in names
	select new
	{
		Original = n,
		Vowelless = n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", "")
	}
	into   temp
	where  temp.Vowelless.Length > 2
	select temp.Original
)
.Dump ("With the 'into' keyword");