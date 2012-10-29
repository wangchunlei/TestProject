<Query Kind="Statements" />

// (The following example requires the NORTHWIND database to run. You can
//  download the Northwind database here:
//  http://www.microsoft.com/downloads/details.aspx?FamilyID=06616212-0356-46A0-8DA2-EEBC53A68034 )
//
// Suppose you want to project a nested grouping, for example, producing
// a hierarchical output of countries, cities and post codes as follows:
//
//   Brazil
//      Campinas
//         04876-786
//      Resende
//         08737-363
//      Rio de Janeiro
//         02389-673
//         02389-890
//         05454-876
//
// We'll assume the country, city and post code information is all in one table called Orders 
// (as is it in the NORTHWIND sample database).
//
// To write this query, you need to nest one "group by" within the projection of another:

var query =
    from o in Orders
    group o by o.ShipCountry into countryGroups
    select new
    {
        Country = countryGroups.Key,
        Cities =
            from cg in countryGroups
            group cg.ShipPostalCode by cg.ShipCity into cityGroups
            select new
            {
                City = cityGroups.Key,
                PostCodes = cityGroups.Distinct()
            }
    };
   
query.Dump();

// In this case, at the bottom level we are interested only in a simple list of postal codes (not in
// any further information about those orders) so we can use a simple Distinct to get the desired list.
//
// Here's how to programmatically enumerate the result:

foreach (var countryGroup in query)
{
    Console.WriteLine (countryGroup.Country);
    foreach (var cityGroup in countryGroup.Cities)
    {
        Console.WriteLine ("   " + cityGroup.City);
        foreach (string postCode in cityGroup.PostCodes)
            Console.WriteLine ("      " + postCode);
    }
}