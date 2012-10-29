<Query Kind="FSharpProgram" />

//  Note: To run F# in LINQPad, you must download and install the correct version of F#.
//  http://research.microsoft.com/en-us/um/cambridge/projects/fsharp/release.aspx
//
//  For LINQPad 2.x (Framework 3.5), you'll need F# for Visual Studio 2008.
//  For LINQPad 4.x (Framework 4.0), you'll need F# for Visual Studio 2010.

"Hello World".Dump ("It works!")

// Here's another way to call Dump:

"Hello World" |> Dump

// Note that querying databases via LINQ is not supported for F# queries.

// Learn more about F# at http://fsharp.net