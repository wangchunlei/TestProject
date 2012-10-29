<Query Kind="Statements" />

var e = new XElement ("date", DateTime.Now);
e.SetValue (DateTime.Now.AddDays(1));
e.Value.Dump();