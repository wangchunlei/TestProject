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

// The following groups purchases by year, then returns only those groups where
// the average purchase across the year was greater than $1000:

from p in Purchases
group p.Price by p.Date.Year into salesByYear
where salesByYear.Average (x => x) > 1000
select new
{
	Year       = salesByYear.Key,
	TotalSales = salesByYear.Count(),
	AvgSale    = salesByYear.Average(),
	TotalValue = salesByYear.Sum()
}