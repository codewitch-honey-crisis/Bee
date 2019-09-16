using System;
using System.Collections.Generic;

namespace Bee
{
	// ported from https://github.com/w8r/splay-tree/blob/master/src/index.ts
	// license for that follows:
	/*
	 The MIT License (MIT)
	Copyright (c) 2019 Alexander Milevski info@w8r.name
	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
	 */


	public class SortedSplayTreeDictionary<TKey, TValue> : IDictionary<TKey,TValue>
	{

		IComparer<TKey> _comparer;

		public SortedSplayTreeDictionary(IComparer<TKey> comparer)
		{
			_comparer = comparer ?? Comparer<TKey>.Default;
		}
		public SortedSplayTreeDictionary() : this(null) { }
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
		_Node _Splay(TKey i, _Node t)
		{
			var n = new _Node();
			var l = n;
			var r = n;

			while (true)
			{
				var cmp = _comparer.Compare(i, t.Key);
				//if (i < t.key) {
				if (cmp < 0)
				{
					if (t.Left == null) break;
					//if (i < t.left.key) {
					if (_comparer.Compare(i, t.Left.Key) < 0)
					{
						var y = t.Left;                           /* rotate right */
						t.Left = y.Right;
						y.Right = t;
						t = y;
						if (t.Left == null) break;
					}
					r.Left = t;                               /* link right */
					r = t;
					t = t.Left;
					//} else if (i > t.key) {
				}
				else if (cmp > 0)
				{
					if (t.Right == null) break;
					//if (i > t.right.key) {
					if (_comparer.Compare(i, t.Right.Key) > 0)
					{
						var y = t.Right;                          /* rotate left */
						t.Right = y.Left;
						y.Left = t;
						t = y;
						if (t.Right == null) break;
					}
					l.Right = t;                              /* link left */
					l = t;
					t = t.Right;
				}
				else break;
			}
			/* assemble */
			l.Right = t.Left;
			r.Left = t.Right;
			t.Left = n.Right;
			t.Right = n.Left;
			return t;
		}
		public TValue this[TKey key] {
			get {
				TValue value;
				if (TryGetValue(key, out value))
					return value;
				throw new KeyNotFoundException();
			}
			set {
				var n = _Search(key);
				if (null != n)
					n.Value = value;
				else
					Add(key, value);
			}
		}
		public ICollection<TKey> Keys
			=> CollectionUtility.CreateKeys(this);
		public ICollection<TValue> Values
			=> CollectionUtility.CreateValues(this);
		public void CopyTo(KeyValuePair<TKey,TValue>[] array, int index)
			=> CollectionUtility.CopyTo(this, array, index);
		_Node _Add(TKey key, TValue data, _Node t)
		{
			var node = new _Node(key, data);

			if (t == null)
			{
				node.Left = node.Right = null;
				return node;
			}

			t = _Splay(key, t);
			var cmp = _comparer.Compare(key, t.Key);
			if (cmp < 0)
			{
				node.Left = t.Left;
				node.Right = t;
				t.Left = null;
			}
			else if (cmp >= 0)
			{
				node.Right = t.Right;
				node.Left = t;
				t.Right = null;
			}
			return node;
		}
		_Node _root;
		int _size;



		/**
		 * @param  {Key} key
		 * @return {Node|null}
		 */
		public bool Remove(TKey key)
		{
			_Node r;
			if(_TryRemove(key,_root,out r))
			{
				_root = r;
				return true;
			}
			return false;
		}
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			TValue value;
			if (TryGetValue(item.Key, out value) && Equals(value, item.Value))
				return Remove(item.Key); // returns true
			return false;
		}
		public bool TryGetValue(TKey key,out TValue value)
		{
			var node = _Search(key);
			if (null!=node)
			{
				value = node.Value;
				return true;
			}
			value = default(TValue);
			return false;
		}
		/**
		 * Deletes i from the tree if it's there
		 */
		bool _TryRemove(
		  TKey i, _Node t, out _Node x)
		{
			x = null;
			if (t == null) return false;
			t = _Splay(i, t);
			var cmp = _comparer.Compare(i, t.Key);
			if (cmp == 0)
			{               /* found it */
				if (t.Left == null)
				{
					x = t.Right;
				}
				else
				{
					x = _Splay(i, t.Left);
					x.Right = t.Right;
				}

				this._size--;
				return true;

			}
			return false ;                         /* It wasn't there */
		}
		public void Add(TKey key, TValue data)
		{
			var node = new _Node(key, data);

			if (this._root == null)
			{
				node.Left = node.Right = null;
				this._size++;
				this._root = node;
				return;
			}

			var t = _Splay(key, this._root);
			var cmp = _comparer.Compare(key, t.Key);
			if (cmp == 0)
				throw new ArgumentException("An item with the specified key already exists in the dictionary.", nameof(key));
			else
			{
				if (cmp < 0)
				{
					node.Left = t.Left;
					node.Right = t;
					t.Left = null;
				}
				else if (cmp > 0)
				{
					node.Right = t.Right;
					node.Left = t;
					t.Right = null;
				}
				this._size++;
				this._root = node;
			}

		}
		public void Add(KeyValuePair<TKey, TValue> item)
			=> Add(item.Key, item.Value);
		_Node _Search(TKey key)
		{
			if (null != this._root)
			{
				this._root = _Splay(key, this._root);
				if (this._comparer.Compare(key, this._root.Key) != 0) return null;
			}
			return this._root;
		}
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			var current = this._root;
			var Q = new Stack<_Node>();
			var done = false;

			while (!done)
			{
				if (current != null)
				{
					Q.Push(current);
					current = current.Left;
				}
				else
				{
					if (0 < Q.Count)
					{
						current = Q.Pop();
						yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);

						current = current.Right;
					}
					else done = true;
				}
			}
		}
		public int Height {
			get {
				// Base Case 
				if (_root == null)
					return 0;

				// Create an empty queue for level order tarversal 
				var q = new Queue<_Node>();

				// Enqueue Root and initialize height 
				q.Enqueue(_root);
				int height = 0;

				while (true)
				{
					// nodeCount (queue size) indicates number of nodes 
					// at current lelvel. 
					int nodeCount = q.Count;
					if (nodeCount == 0)
						return height;

					height++;

					// Dequeue all nodes of current level and Enqueue all 
					// nodes of next level 
					while (nodeCount > 0)
					{
						_Node node = q.Peek();
						q.Dequeue();
						if (node.Left != null)
							q.Enqueue(node.Left);
						if (node.Right != null)
							q.Enqueue(node.Right);
						nodeCount--;
					}
				}
			}
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> GetEnumerator();
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			TValue value;
			if (TryGetValue(item.Key, out value) && Equals(value, item.Value))
				return true;
			return false;
		}
		public bool ContainsKey(TKey key)
		{
			if(null!=_root)
			{
				_root = _Splay(key, _root);
				if (0==this._comparer.Compare(key, this._root.Key) ) return true;
			}
			return false;
		}



		public void Clear()
		{

			this._root = null;

			this._size = 0;

		}
		public int Count { get { return _size; } }

		#region _Node
		private class _Node
		{
			public TKey Key;
			public TValue Value;
			public _Node Left;
			public _Node Right;


			public _Node(TKey key, TValue data)
			{
				this.Key = key;
				this.Value = data;
			}
			public _Node() { }
		}
		#endregion
	}
}