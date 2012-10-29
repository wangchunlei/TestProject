<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <Server>.\SQLEXPRESS</Server>
    <Persist>true</Persist>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
  </Connection>
</Query>

// We saw how to construct a basic subquery in Chapter 8:

var names = new[] { "Tom", "Dick", "Harry", "Mary", "Jay" }.AsQueryable();
(
	from n in names
	where n.Length == names.Min (n2 => n2.Length)
	select n
)
.Dump ("Basic subquery");

// The same principle works well in LINQ to SQL:
(
	from c in Customers
	where c.Name.Length == Customers.Min (c2 => c2.Name.Length)
	select c
)
.Dump ("Basic subquery, LINQ to SQL");

// We can construct similar subqueries across association properties:
(
	from c in Customers
	where c.Purchases.Any (p => p.Price > 1000)
	select c
)
.Dump ("Customers who have purchased at least one item > $1000");