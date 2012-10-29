<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <Persist>true</Persist>
  </Connection>
</Query>

IQueryable<XElement> sqlQuery =
	from c in Customers
		select 
			new XElement ("customer", new XAttribute ("id", c.ID),
				new XElement ("name", c.Name),
				new XElement ("buys", c.Purchases.Count)
			);
	
var customers = new XElement ("customers", sqlQuery);

sqlQuery.Dump ("SQL Query");
customers.Dump ("Final projection");