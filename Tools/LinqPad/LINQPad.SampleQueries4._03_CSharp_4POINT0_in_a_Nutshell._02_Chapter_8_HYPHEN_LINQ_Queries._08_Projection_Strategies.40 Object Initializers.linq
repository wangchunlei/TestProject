<Query Kind="Statements" />

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();

IEnumerable<TempProjectionItem> temp =
	from n in names
	select new TempProjectionItem
	{
		Original  = n,
		Vowelless = n.Replace ("a", "").Replace ("e", "").Replace ("i", "").Replace ("o", "").Replace ("u", "")
	};
	
temp.Dump();
  
}
class TempProjectionItem
{
	public string Original;      // Original name
	public string Vowelless;   // Vowel-stripped name