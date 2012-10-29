<Query Kind="Statements" />

//       The Regex metacharacters are as follows:
//
//       \  *  +  ?  |  {  [  (  )  ^  $  .  #

Regex.Match ("what?", @"what\?").Value.Dump ("Correct");
Regex.Match ("what?", @"what?").Value.Dump ("Incorrect");

Regex.Escape   (@"?").Dump ("Escape");
Regex.Unescape (@"\?").Dump ("Unescape");

Regex.IsMatch ("hello world", @"hello world").Dump ("Are spaces significant?");
Regex.IsMatch ("hello world", @"(?x) hello world").Dump ("Are spaces are significant?");