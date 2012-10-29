<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>    
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

string[] names = { "Tom", "Dick", "Harry", "Mary", "Jay" };

names.OrderBy (n => n, StringComparer.CurrentCultureIgnoreCase)
	.Dump ("Case insensitive ordering");

(
	from c in Customers
	orderby c.Name.ToUpper()
	select c.Name
)
.Dump ("Closest equivalent in LINQ to SQL");