<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <Server>.\SQLEXPRESS</Server>
    <Persist>true</Persist>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
  </Connection>
</Query>

// Welcome to the LINQPad samples. Included are over 200 examples from
// the book "C# 4.0 in a Nutshell" - http://www.albahari.com/nutshell/
//
// If you have the book, you can interactively work through the examples as you
// read Chapters 8-10 and Chapter 26. If you don't have the book, the samples
// will give you an idea of the kind of queries you perform with LINQ.
// 
// Note: Many more chapters are now online, including samples that cover the
//       whole C# 4.0 language! To download, click 'Download more samples'.
//
// The LINQ to SQL samples rely on a simple demo database. If you have SQLExpress
// installed, this database will automatically unpack and display in the database
// explorer on the left (as "Nutshell.mdf"). The two essential tables are "Customer"
// and "Purchase", in a one-to-many relationship.
//
// You can test it now by pressing F5:

Purchases.Dump ("Hooray - you can start running all the examples!");

// If you don't have SQLExpress installed, the connection on the left will display in red
// and this query will throw an exception. This might be the case if you have installed
// the full-blown SQL Server instead of SQLExpress. The solution is to create your own
// database as follows:
//
//   1. Right-click "Nutshell.mdf" on the left, and choose "Edit".
//   2. Select an empty database (or create a new one) on a server of your choice.
//   3. Click "Remember this connection" and OK
//   4. Execute the "Populate Demo Database" script in the next example.