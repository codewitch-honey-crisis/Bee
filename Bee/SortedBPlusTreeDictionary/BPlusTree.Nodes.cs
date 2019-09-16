//
// Library: KwData
// File:    BPlusTreeNodes.cs
// Purpose: Define internal tree structure and its basic operations.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bee
{
	public partial class SortedBPlusTreeDictionary<TKey, TValue>
	{
		/// <summary>Common functionality of a branch and leaf - a list of ordered keys.
		/// </summary>
		/// <typeparam name="TKey">Type of ordered field.</typeparam>
		/// <remarks>In a leaf, this is a contiguous block of keys from the dictionary.
		/// In a branch, this contains the first keys that are contained within every
		/// leaf, except the leftmost leaf.</remarks>
		internal abstract partial class Node
		{
			internal List<TKey> keys { get; private set; }

			protected Node(int newOrder) { keys = new List<TKey>(newOrder - 1); }
			internal abstract int Height {get;}
			internal int KeyCount { get { return keys.Count; } }
			internal int KeyCapacity { get { return keys.Capacity; } }
			internal bool NotFull { get { return keys.Count < keys.Capacity; } }

			internal void AddKey(TKey key) { keys.Add(key); }
			internal TKey GetKey(int i) { return keys[i]; }
			internal int Search(TKey key) { return keys.BinarySearch(key); }
			internal int Search(TKey key, IComparer<TKey> c) { return keys.BinarySearch(key, c); }
			internal void SetKey(int i, TKey key) { keys[i] = key; }
			internal void RemoveKey(int i) { keys.RemoveAt(i); }
			internal void RemoveKeys(int i, int count) { keys.RemoveRange(i, count); }
			internal void TruncateKeys(int i) { keys.RemoveRange(i, keys.Count - i); }
			internal void InsertKey(int i, TKey newKey) { keys.Insert(i, newKey); }
		}


		/// <summary>Any page internal to the tree. Provides subtree functionality.
		/// </summary>
		/// <typeparam name="TKey">Type of ordered field.</typeparam>
		internal class InternalNode : Node
		{
			private List<Node> childNodes;
			
			internal InternalNode(InternalNode leftBranch)
				: base(leftBranch.ChildCount)
			{
				_Initialize(leftBranch.ChildCount);
			}
			internal override int Height {
				get {
					int max = 0;
					for (var i = 0; i < this.ChildCount; i++)
					{
						var m = this.childNodes[i].Height;
						if (m > max)
							max = m;
					}
					return 1 + max;
					
				}

			}
			internal InternalNode(Node child, int newOrder)
				: base(newOrder)
			{
				_Initialize(newOrder);
				Add(child);
			}

			private void _Initialize(int newOrder)
			{ childNodes = new List<Node>(newOrder); }

			internal int ChildCount { get { return childNodes.Count; } }

			internal Node GetChild(int i)
			{ return childNodes[i]; }

			internal Node FirstChild { get { return childNodes[0]; } }
			internal Node LastChild { get { return childNodes[childNodes.Count - 1]; } }

			internal void RemoveChild(int i)
			{ childNodes.RemoveAt(i); }

			internal void Truncate(int index)
			{
				TruncateKeys(index);
				childNodes.RemoveRange(index + 1, childNodes.Count - (index + 1));
			}

			internal void Add(Node newBlock)
			{ childNodes.Add(newBlock); }

			internal void Add(TKey newKey, Node newBlock)
			{
				AddKey(newKey);
				childNodes.Add(newBlock);
			}

			internal void Insert(int index, Node newItem)
			{
				childNodes.Insert(index, newItem);
			}

			internal void Remove(int index, int count)
			{
				RemoveKeys(index, count);
				childNodes.RemoveRange(index, count);
			}
		}


		/// <summary>Terminal node giving the value for each key in the keys list.
		/// </summary>
		/// <typeparam name="TKey">Type of ordered field.</typeparam>
		/// <typeparam name="TValue">Type of field associated with TKey.</typeparam>
		internal class LeafNode: Node
		{
			private LeafNode _rightLeaf;  // For the linked leaf list.
			private LeafNode _leftLeaf;  // For the linked leaf list.
			private List<TValue> _values;           // Payload.
			internal override int Height => 1;
			internal LeafNode(int newOrder)
				: base(newOrder)
			{
				_values = new List<TValue>(newOrder - 1);
				_leftLeaf = null;
				_rightLeaf = null;
			}

			/// <summary>Splice a leaf to right of <paramref name="leftLeaf"/>.</summary>
			/// <param name="leftLeaf">Provides linked list insert point.</param>
			internal LeafNode(LeafNode leftLeaf)
				: base(leftLeaf.KeyCapacity + 1)
			{
				_values = new List<TValue>(leftLeaf.KeyCapacity);

				// Linked list insertion.
				_rightLeaf = leftLeaf._rightLeaf;
				leftLeaf._rightLeaf = this;
				_leftLeaf = leftLeaf._leftLeaf;
			}


			/// <summary>Give next leaf in linked list.</summary>
			internal LeafNode RightLeaf {
				get { return _rightLeaf; }
				set { _rightLeaf = value; }
			}
			internal LeafNode LeftLeaf {
				get { return _leftLeaf; }
				set { _leftLeaf = value; }
			}


			internal int ValueCount { get { return _values.Count; } }


			internal KeyValuePair<TKey, TValue> GetPair(int i)
			{ return new KeyValuePair<TKey, TValue>(keys[i], _values[i]); }


			internal TValue GetValue(int i)
			{ return _values[i]; }


			internal void SetValue(int i, TValue newValue)
			{ _values[i] = newValue; }


			internal void Add(TKey key, TValue value)
			{
				AddKey(key);
				_values.Add(value);
			}

			internal void Add(LeafNode source, int sourceStart, int sourceStop)
			{
				for (int i = sourceStart; i < sourceStop; ++i)
					Add(source.GetKey(i), source.GetValue(i));
			}

			internal void Insert(int index, TKey key, TValue value)
			{
				Debug.Assert(index >= 0 && index <= ValueCount);
				InsertKey(index, key);
				_values.Insert(index, value);
			}

			internal void Remove(int index)
			{
				Debug.Assert(index >= 0 && index <= ValueCount);
				RemoveKey(index);
				_values.RemoveAt(index);
			}

			internal void Remove(int index, int count)
			{
				Debug.Assert(index >= 0 && index + count <= ValueCount);
				RemoveKeys(index, count);
				_values.RemoveRange(index, count);
			}

			internal void Truncate(int index)
			{
				Debug.Assert(index >= 0 && index < ValueCount);
				RemoveKeys(index, KeyCount - index);
				_values.RemoveRange(index, ValueCount - index);
			}
		}
	}
}
