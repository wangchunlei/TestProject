<Query Kind="Expression" />

(
	from vote in new[] { "Bush", "Gore", "Gore", "Bush", "Bush" }
	group vote by vote into g
	orderby g.Count() descending
	select g.Key
)
.First()
