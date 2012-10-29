<Query Kind="Statements">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <Persist>true</Persist>
  </Connection>
</Query>

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

var query =
	new XElement ("ProjectReport",
		from compileItem in project.Elements (ns + "ItemGroup").Elements (ns + "Compile")
		let include = compileItem.Attribute ("Include")
		where include != null
		select new XElement ("File", include.Value)
	);
	
query.Dump();