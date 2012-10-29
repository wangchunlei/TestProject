<Query Kind="Statements" />

ArrayList classicList = new ArrayList();
classicList.AddRange ( new int[] { 3, 4, 5 } );

DateTime offender = DateTime.Now;
classicList.Add (offender);

IEnumerable<int>
	ofTypeSequence = classicList.OfType<int>(),
	castSequence = classicList.Cast<int>();
  
ofTypeSequence.Dump ("Notice that the offending DateTime element is missing");

try
{ 
	castSequence.Dump();
}
catch (InvalidCastException ex)
{
	ex.Message.Dump ("Notice what the offending DateTime element does to the Cast sequence");
}