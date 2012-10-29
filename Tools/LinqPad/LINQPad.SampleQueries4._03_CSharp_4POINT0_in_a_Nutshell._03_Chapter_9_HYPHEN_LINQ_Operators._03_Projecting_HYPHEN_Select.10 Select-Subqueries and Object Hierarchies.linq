<Query Kind="Statements" />

string sampleDirectory = Environment.GetFolderPath  (Environment.SpecialFolder.MyDocuments);

DirectoryInfo[] dirs = new DirectoryInfo (sampleDirectory).GetDirectories();

var query =
	from d in dirs  
	where (d.Attributes & FileAttributes.System) == 0
	select new
	{
		DirectoryName = d.FullName,
		Created = d.CreationTime,
		Files = from f in d.GetFiles()
			where (f.Attributes & FileAttributes.Hidden) == 0
			select new { FileName = f.Name, f.Length, }
	};

query.Dump();

// Here's how to enumerate the results manually:

foreach (var dirFiles in query)
{
	Console.WriteLine ("Directory: " + dirFiles.DirectoryName);
	foreach (var file in dirFiles.Files)
		Console.WriteLine ("    " + file.FileName + "Len: " + file.Length);
}