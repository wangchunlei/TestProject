<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

var query1 =
	from c in Customers
	join p in Purchases on c.ID equals p.CustomerID
	select c.Name + " bought a " + p.Description;
	
var query2 =
	from p in Purchases
	join c in Customers on p.CustomerID equals c.ID
	select c.Name + " bought a " + p.Description;

query1.Dump();
query2.Dump();