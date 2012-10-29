<Query Kind="Expression">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <Persist>true</Persist>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

// You can also use a Where subquery to join manually, rather than 
// through Association properties:

from c in Customers
from p in Purchases.Where (p => p.CustomerID == c.ID).DefaultIfEmpty()
select new
{
	c.Name,
	p.Description,
	Price = (decimal?) p.Price
}

// You can use a similar strategy to perform nonequi-outer joins.