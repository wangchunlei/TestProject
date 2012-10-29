<query Kind="Expression" />

/*	Knew you'd ask!

	To query a database, first click 'Add connection' (left),
	then select the desired database in the query's "Database" combo (above).

	For example, the following query will work on the NORTHWND sample database:  */

from p in Products
let spanishOrders = p.OrderDetails.Where (o => o.Order.ShipCountry == "Spain")
where spanishOrders.Any()
orderby p.ProductName
select new
{
	p.ProductName,
	p.Category.CategoryName,
	Orders = spanishOrders.Count(),	
	TotalValue = spanishOrders.Sum (o => o.UnitPrice * o.Quantity)
}

// Database and connection details are saved with each query, so next time you
// open the query, the schema tree on the left will conveniently reappear.