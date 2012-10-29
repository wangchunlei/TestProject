<Query Kind="Expression">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <Server>.\SQLEXPRESS</Server>
    <Persist>true</Persist>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
  </Connection>
</Query>

// The following skips the first 5 purchases (ordered by price) and takes the next 3:

Purchases.OrderBy (p => p.Price).Skip (5).Take(3)

// Take a look at the SQL.  So much easier with LINQ!