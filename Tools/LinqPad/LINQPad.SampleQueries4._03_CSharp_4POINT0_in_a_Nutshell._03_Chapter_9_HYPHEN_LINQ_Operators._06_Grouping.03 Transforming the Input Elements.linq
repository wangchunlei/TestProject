<Query Kind="Statements" />

var files = Directory.GetFiles (Path.GetTempPath()).Take (100).ToArray().AsQueryable();

files.GroupBy (file => Path.GetExtension (file), file => file.ToUpper())
	.Dump ("In Fluent Syntax");
	
(
	from file in files
	group file.ToUpper() by Path.GetExtension (file)
)
.Dump ("In query syntax");