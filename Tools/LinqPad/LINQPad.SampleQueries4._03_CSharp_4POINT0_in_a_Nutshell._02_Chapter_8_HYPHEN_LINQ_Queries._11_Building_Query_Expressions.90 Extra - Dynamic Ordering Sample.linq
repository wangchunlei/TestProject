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

// Thanks to Matt Warren, of the Microsoft LINQ to SQL team, for illustrating how this is done.
//
// Suppose you want order a query based on string that you receive at runtime. The string
// indicates a property or field name, such as "Price" or "Description" or "Date".

// For this, you need to dynamically contruct an "OrderBy" MethodCallExpression. This, in turn,
// requires a dynamically constructed LambdaExpression that references the property or field
// upon which to sort. Here's the complete solution:

IQueryable query =            // The original unordered query
	from p in Purchases
	where p.Price > 100
	select p;

string propToOrderBy = "Price";     // Try changing this to "Description" or "Date"

ParameterExpression purchaseParam = Expression.Parameter (typeof (Purchase), "p");
MemberExpression member = Expression.PropertyOrField (purchaseParam, propToOrderBy);
LambdaExpression lambda = Expression.Lambda (member, purchaseParam);

Type[] exprArgTypes = { query.ElementType, lambda.Body.Type };

MethodCallExpression methodCall =
	Expression.Call (typeof (Queryable), "OrderBy", exprArgTypes, query.Expression, lambda);

IQueryable orderedQuery = query.Provider.CreateQuery (methodCall);
orderedQuery.Dump();
