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

ParameterExpression p = Expression.Parameter (typeof (string), "s");

MemberExpression stringLength = Expression.Property (p, "Length");
ConstantExpression five = Expression.Constant (5);

BinaryExpression comparison = Expression.LessThan (stringLength, five);

Expression<Func<string, bool>> lambda = Expression.Lambda<Func<string, bool>> (comparison, p);

Func<string, bool> runnable = lambda.Compile();

runnable ("kangaroo")  .Dump ("kangaroo is less than 5 characters");
runnable ("dog")       .Dump ("dog is less than 5 characters");
