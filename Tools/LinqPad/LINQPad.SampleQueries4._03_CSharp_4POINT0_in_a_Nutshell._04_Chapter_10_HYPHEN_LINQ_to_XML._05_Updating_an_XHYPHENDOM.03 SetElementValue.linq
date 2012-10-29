<Query Kind="Statements" />

XElement settings = new XElement ("settings");

settings.SetElementValue ("timeout", 30);   settings.Dump ("Adds child element"); 
settings.SetElementValue ("timeout", 60);   settings.Dump ("Updates child element");