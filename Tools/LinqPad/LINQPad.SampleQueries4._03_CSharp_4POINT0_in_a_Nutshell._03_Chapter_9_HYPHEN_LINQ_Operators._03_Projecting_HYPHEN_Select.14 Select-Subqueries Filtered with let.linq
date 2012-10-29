<Query Kind="Expression">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <Persist>true</Persist>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

from c in Customers
let highValueP =
	from p in c.Purchases
	where p.Price > 1000
	select new { p.Description, p.Price }
where highValueP.Any()
select new 
{
	c.Name,
	Purchases = highValueP
}