<Query Kind="Statements" />

var e1 = new XElement ("test", "Hello");
e1.Add ("World");

var e2 = new XElement ("test", "Hello", "World");

var e3 = new XElement ("test", new XText ("Hello"), new XText ("World"));

e1.Dump(); e2.Dump(); e3.Dump();

e1.Nodes().Count().Dump ("Number of children in e1");
e2.Nodes().Count().Dump ("Number of children in e2");
e3.Nodes().Count().Dump ("Number of children in e3");
