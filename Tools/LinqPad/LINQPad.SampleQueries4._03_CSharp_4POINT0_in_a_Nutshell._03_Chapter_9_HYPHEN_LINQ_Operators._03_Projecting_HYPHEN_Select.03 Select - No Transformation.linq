<Query Kind="Expression">
  <Namespace>System.Drawing</Namespace>
</Query>

from f in FontFamily.Families.AsQueryable() 
where f.IsStyleAvailable (FontStyle.Strikeout)
select f