<Query Kind="Statements" />

string r =
	@"(?x)" +                           // Ignore spaces within regex expression, for readability
	@"^"    +                           // Anchor at start of string
	@"(?=.* ( \d | \p{P} | \p{S} ))" +  // String must contain a digit or punctuation char or symbol
	@".{6,}";                           // String must be at least 6 characters in length

Console.WriteLine (Regex.IsMatch ("abc12", r));
Console.WriteLine (Regex.IsMatch ("abcdef", r));
Console.WriteLine (Regex.IsMatch ("ab88yz", r));