<Query Kind="Statements" />

string regFind = 
  @"<(?'tag'\w+?).*>" +  // match first tag, and name it 'tag'
  @"(?'text'.*?)"     +  // match text content, name it 'text'
  @"</\k'tag'>";         // match last tag, denoted by 'tag'

string regReplace =
  @"<${tag}"         +  // <tag
  @" value="""       +  // value="
  @"${text}"         +  // text
  @"""/>";              // "/>

Regex.Replace ("<msg>hello</msg>", regFind, regReplace).Dump();