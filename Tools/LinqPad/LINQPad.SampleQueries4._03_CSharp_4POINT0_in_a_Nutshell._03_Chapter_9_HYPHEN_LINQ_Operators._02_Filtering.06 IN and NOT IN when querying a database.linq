<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <Persist>true</Persist>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
  </Connection>
</Query>

string[] chosenOnes = { "Tom", "Jay" };

Customers.Where (c => chosenOnes.Contains (c.Name))
	.Dump ("This translates to SQL WHERE ... IN");
	
Customers.Where (c => !chosenOnes.Contains (c.Name))
	.Dump ("This translates to SQL WHERE NOT ... IN");	