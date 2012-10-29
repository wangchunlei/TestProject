<Query Kind="Statements" />

ArrayList classicList = new ArrayList();
classicList.AddRange ( new int[] { 3, 4, 5 } );

IEnumerable<int> sequence1 = classicList.Cast<int>();

sequence1.Dump ("Because sequence1 implements IEnumerable<int>, we can run queries on it");
