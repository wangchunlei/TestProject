<Query Kind="Statements" />

var files = Directory.GetFiles (Path.GetTempPath()).Take (100).ToArray().AsQueryable();

(
	from file in files
	group file.ToUpper() by Path.GetExtension (file) into grouping
	orderby grouping.Key
	select grouping
)
.Dump();
