<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>    
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

Purchases
	.OrderByDescending (p => p.Price)
	.ThenBy (p => p.Description)
	.Dump ("In fluent syntax");

(
	from p in Purchases
	orderby p.Price descending, p.Description
	select p
)
.Dump ("In query syntax");