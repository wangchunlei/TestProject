<Query Kind="Statements" />

int[] seq1 = { 1, 2, 3 }, seq2 = { 3, 4, 5 };

seq1.Concat (seq2).Dump ("Concat");
seq1.Union  (seq2).Dump ("Union");