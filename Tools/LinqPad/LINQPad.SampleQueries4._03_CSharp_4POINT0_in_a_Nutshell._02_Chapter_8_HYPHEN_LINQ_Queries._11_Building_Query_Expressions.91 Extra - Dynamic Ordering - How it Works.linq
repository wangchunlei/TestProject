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

// Let's break down the last example. We started with a simple unordered query (remember
// that the query does not evaluate at this point, thanks to deferred execution):

IQueryable<Purchase> query =            // The original unordered query
	from p in Purchases
	where p.Price > 100
	select p;

// Here's the property or field name upon which we want to order:

string propToOrderBy = "Price";     // Try changing this to "Description" or "Date"

// The aim is to dynamically constuct the following:
// var orderedQuery = query.OrderBy (p => p.Price);

// Starting from the inside out, we start by creating the lambda expression, p => p.Price.
// To dynamically build a LambaExpression, we first create the parameter, in this case, p.
// Our parameter is of type Purchase, and is called "p":

ParameterExpression purchaseParam = Expression.Parameter (typeof (Purchase), "p");
purchaseParam.Dump ("purchaseParam");

// Next, we need to create "p.Price". The static method Expression.PropertyOrField returns
// a MemberExpression that finds a property or field with the given name:

MemberExpression member = Expression.PropertyOrField (purchaseParam, propToOrderBy);
member.Dump ("member");

// With these two things, we build the LambdaExpression:

LambdaExpression lambda = Expression.Lambda (member, purchaseParam);
lambda.Dump ("lambda");
lambda.ToString().Dump ("lambda.ToString");

// We now need to wrap the lambda expression in a MethodCallExpression that
// references the Queryable.OrderBy method. For this, we call the static Expresion.Call
// method, which is overloaded especially to simplify the task of invoking methods
// that accept lambda expressions:

Type[] exprArgTypes = { query.ElementType, lambda.Body.Type };

MethodCallExpression methodCall =
	Expression.Call (
		typeof (Queryable),   // Type defining method we want to call
		"OrderBy",                // Name of method to call
		exprArgTypes,          // Generic argument types
		query.Expression,     // First argument (the query expression body) 
		lambda);                 // Second argument (the lambda expression)

methodCall.Dump ("methodCall (notice all the work that Expression.Call does for us)");

// The final step is to create the new query, which calls the expression we've just
// created. For this, we use the Provider property exposed by the IQueryable interface,
// which returns an object upon which we call CreateQuery:

IQueryable orderedQuery = query.Provider.CreateQuery (methodCall);

// (Exactly the same thing happens when you ordinarily call Queryable.OrderBy;
// a good way to see this is to download Lutz Roeder's Reflector at
// http://www.aisto.com/roeder/dotnet/ and look at Queryable's OrderBy method). 

// Here's the final result: 

orderedQuery.Expression.ToString().Dump ("The final result");