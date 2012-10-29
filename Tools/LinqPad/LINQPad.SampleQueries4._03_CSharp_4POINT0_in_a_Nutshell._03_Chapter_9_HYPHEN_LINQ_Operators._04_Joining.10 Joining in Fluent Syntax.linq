<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

var querySyntax =
	from c in Customers
	join p in Purchases on c.ID equals p.CustomerID
	select new
	{
		c.Name, p.Description, p.Price
	};

var fluentSyntax =
	Customers.Join (         // outer collection
		Purchases,             // inner collection
		c => c.ID,             // outer key selector
		p => p.CustomerID,     // inner key selector
		(c, p) => new          // result selector 
		{
			c.Name, p.Description, p.Price
		}    
	);
	
querySyntax.Dump ("Query syntax");
fluentSyntax.Dump ("Same query in fluent syntax");