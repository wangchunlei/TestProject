<Query Kind="Statements" />

int[] seq1 = { 1, 2, 3 }, seq2 = { 3, 4, 5 };

seq1.Intersect (seq2).Dump ("Intersect");
seq1.Except    (seq2).Dump ("seq1.Except (seq2)");
seq2.Except    (seq1).Dump ("seq2.Except (seq1)");