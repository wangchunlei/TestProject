<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>    
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

var groupJoinQuery =
	from c in Customers
	join p in Purchases on c.ID equals p.CustomerID
	into custPurchases
	select new
	{
		CustName = c.Name,
		custPurchases
	};

var selectEquivalent =
	from c in Customers
	select new
	{
		CustName = c.Name,
		custPurchases = Purchases.Where (p => c.ID == p.CustomerID)
	};

@"Notice in the SQL results pane, that there's no difference between these two queries.
The second query, however, is more flexibile.".Dump();	

groupJoinQuery.Dump ("Group Join Query");
selectEquivalent.Dump ("Equivalent with Select");

