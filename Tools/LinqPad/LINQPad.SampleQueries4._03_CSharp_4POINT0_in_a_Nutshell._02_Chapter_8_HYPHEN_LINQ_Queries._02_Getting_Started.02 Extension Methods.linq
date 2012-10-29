<Query Kind="Expression" />

// "Where" is an extension method in System.Linq.Enumerable:

(new[] {"Tom", "Dick", "Harry"} ).Where (n => n.Length >= 4)
	
// (Notice that the language dropdown above is now 'C# Expression' rather than 'C# Statement').