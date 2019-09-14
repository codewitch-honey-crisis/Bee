# Bee
A set of binary tree dictionaries in C#

Although not used as much as .NET's Dictionary<TKey,TValue> class, the SortedDictionary<TKey, TValue> is an important part of the BCL's collections suite.

However, the framework class only gives you a red-black tree algorithm to work with in terms of the binary tree it sorts and searches.

Sometimes a B-tree, an AVL tree, or a splay tree is more appropriate. If you need one however, you're out of luck as far as the BCL goes.

Consequently, I've developed a few classes to fill the gap, primarily by porting, debugging, and rewriting swaths of code at https://www.geeksforgeeks.org (specific links for the various portions in the source) so I could learn the algorithms.

_Splay() function ported from https://github.com/w8r/splay-tree/blob/master/src/index.ts
full license for that function (MIT) in the related source - that portion is copyright (c) 2019 Alexander Milevski info@w8r.name
