<Query Kind="Statements" />

string[] files = Directory.GetFiles (Path.GetTempPath()).Take (100).ToArray();

IEnumerable<IGrouping<string,string>> query = 
	files.GroupBy (file => Path.GetExtension (file));

foreach (IGrouping<string,string> grouping in query)
{
	Console.WriteLine ("Extension: " + grouping.Key);
	
	foreach (string filename in grouping)
		Console.WriteLine ("   - " + filename);
}