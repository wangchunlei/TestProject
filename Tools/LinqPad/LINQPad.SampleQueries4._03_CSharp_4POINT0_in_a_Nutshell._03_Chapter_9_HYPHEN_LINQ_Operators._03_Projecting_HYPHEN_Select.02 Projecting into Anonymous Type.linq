<Query Kind="Expression">
  <Namespace>System.Drawing</Namespace>
</Query>

from f in FontFamily.Families.AsQueryable() 
select new 
{
	f.Name,
	LineSpacing = f.GetLineSpacing (FontStyle.Bold)
}