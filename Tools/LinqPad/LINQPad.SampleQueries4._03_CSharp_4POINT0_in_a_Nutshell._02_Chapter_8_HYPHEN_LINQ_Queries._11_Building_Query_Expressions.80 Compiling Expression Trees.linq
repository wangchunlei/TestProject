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

Products.DeleteAllOnSubmit (Products.Where (p => p.ID == 999));
Products.InsertOnSubmit (new Product { ID = 999, Description = "Test", LastSale = DateTime.Now } );
SubmitChanges();

Product[] localProducts = Products.ToArray();

Expression<Func<Product, bool>> isSelling = 
	p => !p.Discontinued && p.LastSale > DateTime.Now.AddDays (-30);

IQueryable<Product> sqlQuery = Products.Where (isSelling);
IEnumerable<Product> localQuery = localProducts.Where (isSelling.Compile());

sqlQuery.Dump ("SQL Query");
localQuery.Dump ("Local Query, using same predicate");