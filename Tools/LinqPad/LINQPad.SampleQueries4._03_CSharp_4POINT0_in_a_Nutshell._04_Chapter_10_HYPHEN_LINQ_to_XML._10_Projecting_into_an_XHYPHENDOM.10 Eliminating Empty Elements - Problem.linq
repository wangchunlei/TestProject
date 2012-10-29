<Query Kind="Expression">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <Persist>true</Persist>
  </Connection>
</Query>

new XElement ("customers",
	from c in Customers
		let lastBigBuy = (
			from p in c.Purchases
			where p.Price > 1000
			orderby p.Date descending
			select p
		).FirstOrDefault()
	select 
		new XElement ("customer", new XAttribute ("id", c.ID),
			new XElement ("name", c.Name),
			new XElement ("buys", c.Purchases.Count),
			new XElement ("lastBigBuy",
				new XElement ("description",
					lastBigBuy == null ? null : lastBigBuy.Description),
				new XElement ("price",
					lastBigBuy == null ? 0m : lastBigBuy.Price)
				)
			)
		)