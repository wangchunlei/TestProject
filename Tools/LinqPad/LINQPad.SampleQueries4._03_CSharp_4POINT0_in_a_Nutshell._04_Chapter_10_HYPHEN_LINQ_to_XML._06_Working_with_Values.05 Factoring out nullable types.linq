<Query Kind="Statements" />

var x = new XElement ("Empty");

double resolution = (double?) x.Attribute ("resolution") ?? 1.0;

resolution.Dump();