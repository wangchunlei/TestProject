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

var slowQuery =
	from c in customers
	from p in purchases where c.ID == p.CustomerID
	select c.Name + " bought a " + p.Description;

var fastQuery =
	from c in customers
	join p in purchases on c.ID equals p.CustomerID
	select c.Name + " bought a " + p.Description;
                
slowQuery.Dump ("Slow local query with SelectMany");                
fastQuery.Dump ("Fast local query with Join");