<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
</Query>

var query =
	from f in FontFamily.Families
	select f.Name;

query.Dump ("In query syntax");


FontFamily.Families.Select (f => f.Name)   .Dump ("In lambda syntax");