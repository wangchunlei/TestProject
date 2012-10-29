<Query Kind="Statements" />

// We need a long list of names for this example, so will indulge in a little
// reflection (using LINQ, of course!) The following query extracts all type 
// names in the mscorlib assembly:

string[] typeNames =
	(from t in typeof (int).Assembly.GetTypes() select t.Name).ToArray();

typeNames
	.Where (t => t.Contains ("Exception"))
	.OrderBy (t => t)
	.Take (20)
	.Dump ("The first 20 matches");

typeNames
	.Where   (t => t.Contains ("Exception"))
	.OrderBy (t => t)
	.Skip (20)
	.Take (20)
	.Dump ("Matches 21 through 40");