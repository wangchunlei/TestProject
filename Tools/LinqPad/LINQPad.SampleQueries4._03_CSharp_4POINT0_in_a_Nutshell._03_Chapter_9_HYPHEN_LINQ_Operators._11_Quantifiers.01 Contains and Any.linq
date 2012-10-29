<Query Kind="Statements" />

new int[] { 2, 3, 4 }.Contains (3)               .Dump ("Contains (3)");
new int[] { 2, 3, 4 }.Any (n => n == 3)          .Dump ("Any (n => n == 3)");
new int[] { 2, 3, 4 }.Any (n => n > 10)          .Dump ("Has a big number");
new int[] { 2, 3, 4 }.Where (n => n > 10).Any()  .Dump ("Has a big number");