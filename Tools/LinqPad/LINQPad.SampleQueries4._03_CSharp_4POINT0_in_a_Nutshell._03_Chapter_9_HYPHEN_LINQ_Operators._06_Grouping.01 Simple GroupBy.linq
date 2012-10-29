<Query Kind="Statements" />

string[] files = Directory.GetFiles (Path.GetTempPath()).Take (100).ToArray();

files.GroupBy (file => Path.GetExtension (file))
	.Dump ("Your temporary files, grouped by extension.");

if (files.Length == 100) "(Maybe you need a cleanup!)".Dump();