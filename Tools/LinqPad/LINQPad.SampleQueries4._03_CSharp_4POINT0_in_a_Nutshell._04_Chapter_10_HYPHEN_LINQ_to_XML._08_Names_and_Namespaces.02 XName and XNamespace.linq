<Query Kind="Statements" />

XName localName = "customer";
XName fullName1 = "{http://domain.com/xmlspace}customer";
fullName1.Dump ("fullname1");

XNamespace ns = "http://domain.com/xmlspace";
XName fullName2 = ns + "customer";
fullName2.Dump ("fullname2 - same result, but cleaner and more efficient");
