<Query Kind="Statements" />

Regex.Match ("cv15.doc", @"cv\d*\.doc").Success.Dump();
Regex.Match ("cvjoint.doc", @"cv.*\.doc").Success.Dump();

Regex.Matches ("slow! yeah slooow!", "slo+w").Count.Dump();

Regex bp = new Regex (@"\d{2,3}/\d{2,3}");
bp.Match ("It used to be 160/110").Value.Dump();
bp.Match ("Now it's only 115/75").Value.Dump();