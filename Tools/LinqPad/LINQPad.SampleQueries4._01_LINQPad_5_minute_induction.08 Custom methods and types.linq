<query Kind="Program">
</query>

void Main()
{
	// LINQPad lets you write custom methods and classes. To use this feature:
	//   1. Change the query language to 'C# Program'
	//   2. Write your main code inside the Main() method that LINQPad creates for you
	//   3. Define any custom methods or classes below the Main method.

	MyCustomMethod();
	
	new MyCustomClass().GetHelloMessage().Dump();
	
	// You can also add references to external DLLs and EXEs (press F4 for Query Properties).
}

void MyCustomMethod()
{
	"Hello from a custom method!".Dump();
}

class MyCustomClass
{
	public string GetHelloMessage()
	{
		return "Hello from a custom type!";
	}
}