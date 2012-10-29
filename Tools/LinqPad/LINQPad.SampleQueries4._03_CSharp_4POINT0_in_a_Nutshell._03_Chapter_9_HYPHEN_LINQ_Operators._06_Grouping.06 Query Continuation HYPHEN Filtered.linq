<Query Kind="Statements" />

var files = Directory.GetFiles (Path.GetTempPath()).AsQueryable();

(
	from file in files
	group file.ToUpper() by Path.GetExtension (file) into grouping
	where grouping.Count() < 5
	select grouping
)
.Dump ("Extensions with less then five files");