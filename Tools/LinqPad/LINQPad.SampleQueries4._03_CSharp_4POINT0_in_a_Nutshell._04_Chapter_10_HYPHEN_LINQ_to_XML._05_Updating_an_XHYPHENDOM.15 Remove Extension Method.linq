<Query Kind="Statements" />

XElement contacts = XElement.Parse (@"
<contacts>
	<customer name='Mary'/>
	<customer name='Chris' archived='true'/>
	<supplier name='Susan'>
		<phone archived='true'>012345678<!--confidential--></phone>
	</supplier>
</contacts>");

contacts.Dump ("Before");

contacts.Elements ("customer").Remove();

contacts.Dump ("After");