<Query Kind="Expression">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

// Note: before delving into this section, make sure you've read the preceding two
// sections: Select and SelectMany. The Join operators are actually unnecessary
// in LINQ to SQL, and the equivalent of SQL inner and outer joins is most easily
// achieved in LINQ to SQL using Select/SelectMany and subqueries!

from c in Customers
join p in Purchases on c.ID equals p.CustomerID
select c.Name + " bought a " + p.Description