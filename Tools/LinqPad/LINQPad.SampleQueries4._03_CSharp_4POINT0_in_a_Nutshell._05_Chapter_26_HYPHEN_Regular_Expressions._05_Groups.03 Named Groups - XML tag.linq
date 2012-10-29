<Query Kind="Statements" />

string regFind = 
  @"<(?'tag'\w+?).*>" +  // match first tag, and name it 'tag'
  @"(?'text'.*?)"     +  // match text content, name it 'text'
  @"</\k'tag'>";         // match last tag, denoted by 'tag'

Match m = Regex.Match ("<h1>hello</h1>", regFind);
m.Groups ["tag"].Value.Dump();
m.Groups ["text"].Value.Dump();