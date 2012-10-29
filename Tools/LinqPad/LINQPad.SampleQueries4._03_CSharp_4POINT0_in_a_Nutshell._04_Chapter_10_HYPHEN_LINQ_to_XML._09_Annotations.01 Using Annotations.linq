<Query Kind="Statements" />

XElement e = new XElement ("test");

e.AddAnnotation ("Hello");
e.Annotation<string>().Dump ("String annotations");

e.RemoveAnnotations<string>();
e.Annotation<string>().Dump ("String annotations");