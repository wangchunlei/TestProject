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

Expression<Func<string, bool>> f = s => s.Length < 5;

f.Body.NodeType.Dump ("Body.NodeType");
(((BinaryExpression) f.Body).Right).Dump ("Body.Right");

f.Dump ("The whole expression tree");