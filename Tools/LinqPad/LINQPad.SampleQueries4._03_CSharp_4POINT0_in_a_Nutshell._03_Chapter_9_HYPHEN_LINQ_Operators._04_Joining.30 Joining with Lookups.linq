<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>    
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

Customer[] customers = Customers.ToArray();
Purchase[] purchases = Purchases.ToArray();

ILookup<int?, Purchase> purchLookup = purchases.ToLookup (p => p.CustomerID, p => p);

var inner =
	from c in customers
	from p in purchLookup [c.ID]
	select new { c.Name, p.Description, p.Price };

inner.Dump ("Inner join equivalent");

var outer =
	from c in customers
	from p in purchLookup [c.ID].DefaultIfEmpty()
	select new
	{
		c.Name,
		Descript = p == null ? null : p.Description,
		Price = p == null ? (decimal?) null : p.Price
	};
	
outer.Dump ("Outer join equivalent");

var groupJoin =
	from c in customers
	select new 
	{
		 CustName = c.Name,
		 CustPurchases = purchLookup [c.ID]
	};

groupJoin.Dump ("GroupJoin equivalent");