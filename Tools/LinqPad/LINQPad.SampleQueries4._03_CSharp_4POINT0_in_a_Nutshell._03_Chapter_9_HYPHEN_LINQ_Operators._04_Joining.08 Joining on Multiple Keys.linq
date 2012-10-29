<Query Kind="Statements" />

PropertyInfo[] stringProps = typeof (string).GetProperties().Dump ("String Props");
PropertyInfo[] builderProps = typeof (StringBuilder).GetProperties().Dump ("StringBuilder Props");

var query =
	from s in stringProps
	join b in builderProps
		on new { s.Name, s.PropertyType } equals new { b.Name, b.PropertyType }
	select new
	{
		s.Name,
		s.PropertyType,
		StringToken = s.MetadataToken,
		StringBuilderToken = b.MetadataToken
	};
	
query.Dump ("Join query");