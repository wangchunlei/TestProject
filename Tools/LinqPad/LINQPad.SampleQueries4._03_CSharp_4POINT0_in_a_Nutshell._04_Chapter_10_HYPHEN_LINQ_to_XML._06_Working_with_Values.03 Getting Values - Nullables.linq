<Query Kind="Statements" />

var x = new XElement ("Empty");

try
{
	int timeout1 = (int) x.Element ("timeout");
}
catch (Exception ex)
{
	ex.Message.Dump ("Element (\"timeout\") returns null so the result cannot be cast to int");
}

int? timeout2 = (int?) x.Element ("timeout");
timeout2.Dump ("Casting to a nullable type solve this problem");
