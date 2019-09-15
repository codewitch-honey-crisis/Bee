//
// Library: KwData
// File     BPlusTreePath.cs
// Purpose: Defines internal class that stores a element location path.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bee
{
	partial class SortedBPlusTreeDictionary<TKey, TValue>
	{
		/// <summary>Stack trace from root to leaf of a key/value pair.</summary>
		/// <remarks>Performs search function for key. Provides directions to existing key
		/// or insertion point for non-existing key.
		/// </remarks>
		/// <typeparam name="TKey">Key type.</typeparam>
		/// <typeparam name="TValue">Value type.</typeparam>
		internal partial class TreePath
		{
			private List<int> indexStack;
			private List<SortedBPlusTreeDictionary<TKey, TValue>.Node> nodeStack;

			/// <summary>
			/// <b>true</b> if leaf key is an exact match; otherwise <b>false</b>.</summary>
			internal bool IsFound { get; private set; }

			#region Constructors

			/// <summary>Perform search and store each level of path on the stack.</summary>
			/// <param name="tree">Tree to search.</param>
			/// <param name="key">Value to find.</param>
			internal TreePath(SortedBPlusTreeDictionary<TKey, TValue> tree, TKey key)
			{
				indexStack = new List<int>();
				nodeStack = new List<SortedBPlusTreeDictionary<TKey, TValue>.Node>();

				SortedBPlusTreeDictionary<TKey, TValue>.Node node = tree._root;

				for (; ; )
				{
					Debug.Assert(node != null);

					nodeStack.Add(node);
					int i = node.Search(key, tree._comparer);

					if (node is SortedBPlusTreeDictionary<TKey, TValue>.LeafNode)
					{
						IsFound = i >= 0;
						if (!IsFound)
							i = ~i;
						indexStack.Add(i);
						return;
					}

					if (i < 0)
						i = ~i;
					else
						++i;

					indexStack.Add(i);
					node = ((SortedBPlusTreeDictionary<TKey, TValue>.InternalNode)node).GetChild(i);
				}
			}

			#endregion

			#region Properties

			internal SortedBPlusTreeDictionary<TKey, TValue>.Node TopNode { get { return nodeStack[indexStack.Count - 1]; } }

			internal int TopNodeIndex { get { return indexStack[indexStack.Count - 1]; } }

			internal int Height { get { return indexStack.Count; } }


			internal TValue LeafValue {
				get {
					int leafIndex = indexStack.Count - 1;
					return ((SortedBPlusTreeDictionary<TKey, TValue>.LeafNode)nodeStack[leafIndex]).GetValue(indexStack[leafIndex]);
				}
				set {
					int leafIndex = indexStack.Count - 1;
					((SortedBPlusTreeDictionary<TKey, TValue>.LeafNode)nodeStack[leafIndex]).SetValue(indexStack[leafIndex], value);
				}
			}


			/// <summary>
			/// Get the node to the immediate left of the node at TreePath.
			/// </summary>
			internal SortedBPlusTreeDictionary<TKey, TValue>.Node GetLeftNode()
			{
				Debug.Assert(indexStack.Count == nodeStack.Count);

				for (int depth = indexStack.Count - 2; depth >= 0; --depth)
				{
					if (indexStack[depth] > 0)
					{
						SortedBPlusTreeDictionary<TKey, TValue>.Node result = ((SortedBPlusTreeDictionary<TKey, TValue>.InternalNode)nodeStack[depth]).GetChild(indexStack[depth] - 1);
						for (; depth < indexStack.Count - 2; ++depth)
							result = ((SortedBPlusTreeDictionary<TKey, TValue>.InternalNode)result).GetChild(result.KeyCount);
						return result;
					}
				}
				return null;
			}


			/// <summary>Get nearest key where left child path taken.</summary>
			/// <remarks>On entry, top of path refers to a branch.</remarks>
			internal TKey GetPivot()
			{
				Debug.Assert(TopNode is SortedBPlusTreeDictionary<TKey, TValue>.InternalNode);
				for (int depth = indexStack.Count - 2; depth >= 0; --depth)
				{
					if (indexStack[depth] > 0)
						return nodeStack[depth].GetKey(indexStack[depth] - 1);
				}

				Debug.Fail("no left pivot");
				return default(TKey);
			}


			/// <summary>Set nearest key where left child path taken.</summary>
			/// <remarks>On entry, top of path refers to a branch.</remarks>
			internal void SetPivot(TKey newPivot)
			{
				for (int depth = indexStack.Count - 2; depth >= 0; --depth)
					if (indexStack[depth] > 0)
					{
						nodeStack[depth].SetKey(indexStack[depth] - 1, newPivot);
						return;
					}
			}

			#endregion

			#region Methods

			internal void Clear()
			{
				indexStack.Clear();
				nodeStack.Clear();
			}

			internal void Pop()
			{
				nodeStack.RemoveAt(nodeStack.Count - 1);
				indexStack.RemoveAt(indexStack.Count - 1);
			}

			internal void Push(SortedBPlusTreeDictionary<TKey, TValue>.Node newNode, int newNodeIndex)
			{
				nodeStack.Add(newNode);
				indexStack.Add(newNodeIndex);
			}


			/// <summary>Adjust tree path to node to the right.</summary>
			/// <returns>Node to immediate right of current path; <b>null</b> if current path
			/// at rightmost node.</returns>
			internal SortedBPlusTreeDictionary<TKey, TValue>.Node TraverseRight()
			{
				SortedBPlusTreeDictionary<TKey, TValue>.Node node = null;
				int height = indexStack.Count;
				for (; ; )
				{
					if (indexStack.Count < 2)
					{
						Clear();
						node = null;
						break;
					}

					Pop();
					node = TopNode;
					int newIndex = TopNodeIndex + 1;

					if (newIndex < ((SortedBPlusTreeDictionary<TKey, TValue>.InternalNode)node).ChildCount)
					{
						indexStack[indexStack.Count - 1] = newIndex;
						node = ((SortedBPlusTreeDictionary<TKey, TValue>.InternalNode)node).GetChild(newIndex);
						for (; ; )
						{
							Push(node, 0);
							if (indexStack.Count >= height)
								break;
							node = ((SortedBPlusTreeDictionary<TKey, TValue>.InternalNode)node).FirstChild;
						}
						break;
					}
				}

				return node;
			}

			#endregion
		}
	}
}
