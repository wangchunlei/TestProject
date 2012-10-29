<Query Kind="Statements" />

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

(
	from   n in names
	select n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", "")
	into   noVowel 
	where  noVowel.Length > 2 orderby noVowel select noVowel
)
.Dump ("The preceding query revisited, with the 'into' keyword");