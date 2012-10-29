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

Customers.Where (c => c.Name.Contains ("a"))  
	.Dump ("Notice the SQL translation uses LIKE");

Customers.Where (c => c.Name.StartsWith ("J"))  
	.Dump ("StartsWith and EndsWith also translate to LIKE");

Customers.Where (c => SqlMethods.Like (c.Name, "_ar%y"))
	.Dump ("A more complex use of LIKE");
	
// The following string functions all have translations to SQL:
//
// StartsWith, EndsWith, Contains, Length, IndexOf,
// Trim, Substring, Insert, Remove, Replace,
// ToUpper, ToLower