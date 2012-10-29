<query Kind="Statements">
	<IncludePredicateBuilder>true</IncludePredicateBuilder>
</query>

// LINQPad also includes PredicateBuilder - to use, simply press F4 and tick
// 'Include PredicateBuilder'. PredicateBuilder is a simple class for dynamically
// building query filter expressions. Here's an example with Northwind's Categories table:

var predicate = PredicateBuilder.False<Categories>();

predicate = predicate.Or (c => c.CategoryName.Contains ("Dairy"));
predicate = predicate.Or (c => c.CategoryName.Contains ("Meat"));

Categories.Where (predicate).Dump();

// Go to http://www.albahari.com/expressions/ for more info on PredicateBuilder.