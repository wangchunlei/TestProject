<Query Kind="Program">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <Persist>true</Persist>
  </Connection>
</Query>

void Main()
{
	XElement project = XElement.Parse (@"
	<Project DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
		<PropertyGroup>
			<Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
			<ProductVersion>9.0.11209</ProductVersion>
		</PropertyGroup>
		<ItemGroup>
			<Compile Include=""ObjectGraph.cs"" />
			<Compile Include=""Program.cs"" />
			<Compile Include=""Properties\AssemblyInfo.cs"" />
			<Compile Include=""Tests\Aggregation.cs"" />
			<Compile Include=""Tests\Advanced\RecursiveXml.cs"" />
		</ItemGroup>
	</Project>");
	
	XNamespace ns = project.Name.Namespace;
	
	IEnumerable<string> paths =
		from compileItem in project.Elements (ns + "ItemGroup").Elements (ns + "Compile")
		let include = compileItem.Attribute ("Include")
		where include != null
		select include.Value;
		
	var query = new XElement ("Project", ExpandPaths (paths));
		
	query.Dump();
}

static IEnumerable<XElement> ExpandPaths (IEnumerable<string> paths)
{
	var brokenUp =
		from path in paths
		let split = path.Split (new char[] { '\\' }, 2)
		orderby split[0]
		select new
		{
			name = split[0],
			remainder = split.ElementAtOrDefault (1)
		};

	IEnumerable<XElement> files =
		from b in brokenUp
		where b.remainder == null
		select new XElement ("file", b.name);

	IEnumerable<XElement> folders =
		from b in brokenUp
		where b.remainder != null
		group b.remainder by b.name into grp
		select new XElement ("folder",
			new XAttribute ("name", grp.Key),
			ExpandPaths (grp)
		);

	return files.Concat (folders);
}
