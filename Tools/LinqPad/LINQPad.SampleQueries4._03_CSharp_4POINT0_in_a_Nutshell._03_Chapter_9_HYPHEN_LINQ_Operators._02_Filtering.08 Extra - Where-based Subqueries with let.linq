<Query Kind="Expression">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <Server>.\SQLEXPRESS</Server>
    <Persist>true</Persist>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
  </Connection>
</Query>

// The let keyword in query expressions comes in useful with subqueries: it lets
// you re-use the subquery in the projection:

from c in Customers
let highValuePurchases = c.Purchases.Where (p => p.Price > 1000)
where highValuePurchases.Any()
select new
{
	c.Name,
	highValuePurchases
}

// We'll see more examples of this in the following section, "Projecting".