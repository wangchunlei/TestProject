<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>    
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

try
{
	Purchases.Min ();
}
catch (Exception ex)
{
	ex.Message.Dump ("Purchases.Min()");
}

Purchases.Min (p => p.Price).Dump ("Lowest price");

Purchases
	.Where (p => p.Price == Purchases.Min (p2 => p2.Price))
	.FirstOrDefault()
	.Dump ("The cheapest purchase");