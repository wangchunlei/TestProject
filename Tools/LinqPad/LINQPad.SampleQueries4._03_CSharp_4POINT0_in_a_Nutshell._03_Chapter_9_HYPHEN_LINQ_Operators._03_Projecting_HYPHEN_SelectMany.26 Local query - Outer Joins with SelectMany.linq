<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

Customer[] localCustomerCollection = Customers.ToArray();

var query =
	from c in localCustomerCollection
	from p in c.Purchases.DefaultIfEmpty()
	select new
	{
		Descript = p == null ? null : p.Description,
		Price = p == null ? (decimal?) null : p.Price
	};

query.Dump();