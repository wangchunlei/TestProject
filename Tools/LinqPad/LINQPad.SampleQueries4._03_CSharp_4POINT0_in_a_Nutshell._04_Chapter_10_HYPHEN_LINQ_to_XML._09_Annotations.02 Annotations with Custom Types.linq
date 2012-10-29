<Query Kind="Program" />

void Main()
{
	XElement e = new XElement ("test");
	
	e.AddAnnotation (new CustomData { Message = "Hello" } );
	e.Annotations<CustomData>().First().Message.Dump();
	
	e.RemoveAnnotations<CustomData>();
	e.Annotations<CustomData>().Count().Dump();	
}

class CustomData				// Private nested type
{
	 internal string Message;
}
