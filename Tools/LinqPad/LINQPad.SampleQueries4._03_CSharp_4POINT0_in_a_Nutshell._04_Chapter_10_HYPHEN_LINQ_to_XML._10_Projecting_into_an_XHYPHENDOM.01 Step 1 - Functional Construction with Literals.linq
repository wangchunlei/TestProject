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

var customers =
	new XElement ("customers",
		new XElement ("customer", new XAttribute ("id", 1),
			new XElement ("name", "Sue"),
			new XElement ("buys", 3) 
		)
	);
	
customers.Dump();