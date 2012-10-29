<Query Kind="Program">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <Persist>true</Persist>
  </Connection>
</Query>

void Main()
{
	FilterSortProducts (Products).Dump ("This query executes on SQL Server");
	
	Product[] localProducts =
	{
		new Product { ID = 1, Description = "Local Product Test", LastSale = new DateTime (2007, 2, 3) }
	};
	
	FilterSortProducts (localProducts.AsQueryable()).Dump ("The same query - executing locally");
}

IQueryable<Product> FilterSortProducts (IQueryable<Product> input)
{
	return 
		from p in input
		where !p.Discontinued && p.LastSale < DateTime.Now.AddDays (-7)
		orderby p.Description
		select p;
}
