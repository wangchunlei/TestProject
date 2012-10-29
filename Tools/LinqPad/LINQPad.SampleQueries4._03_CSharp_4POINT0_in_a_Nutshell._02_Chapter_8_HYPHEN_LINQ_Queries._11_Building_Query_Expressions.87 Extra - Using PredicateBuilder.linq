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

// Refer to http://www.albahari.com/expressions/ for info on PredicateBuilder.

string[] keywords = { "Widget", "Foo", "Bar" };

var predicate = PredicateBuilder.False<Product>();

foreach (string keyword in keywords)
{
	string temp = keyword;
	predicate = predicate.Or (p => p.Description.Contains (temp));
}

Products.Where (predicate).Dump ("Notice the multiple OR clauses in the SQL pane");