﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bee
{
	// adapted from https://www.geeksforgeeks.org/splay-tree-set-1-insert/
	// about half is a rewrite
	public class SortedSplayTreeDictionary<TKey,TValue> : IDictionary<TKey,TValue>
	{
		_Node _root;
		IComparer<TKey> _comparer;
		public SortedSplayTreeDictionary(IComparer<TKey> comparer)
		{
			_comparer = comparer ?? Comparer<TKey>.Default;
		}
		public SortedSplayTreeDictionary() : this(null)
		{

		}
		public int Count {
			get {
				return _GetCount(_root);
			}
		}
		public TValue this[TKey key] {
			get {
				TValue value;
				if (TryGetValue(key, out value))
					return value;
				throw new KeyNotFoundException();
			}
			set {
				var n = _Splay(_root,key);
				if (null != n)
					n.Value = value;
				else
					Add(key, value);
			}
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
			=> DictionaryUtility.CopyTo(this, array, index);
		public bool ContainsKey(TKey key)
		{
			_root = _Splay(_root, key);
			return 0 == _comparer.Compare(_root.Key, key);
		}
		public bool Contains(KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			return TryGetValue(item.Key, out value) && Equals(item.Value, value);
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
			
			if(0 == _comparer.Compare(_root.Key, key))
			{
				value = _root.Value;
				return true;
			}
			_root = _Splay(_root, key);
			if (0 == _comparer.Compare(_root.Key, key))
			{
				value = _root.Value;
				return true;
			}

			value = default(TValue);
			return false;
		}
		public void Add(TKey key,TValue value)
		{
			_root=_Add(_root, key,value);
		}
		public void Add(KeyValuePair<TKey, TValue> item)
			=> Add(item.Key, item.Value);
		public bool Remove(TKey key)
		{
			_Node temp;
			if(_TryRemove(_root, key, out temp))
			{
				_root = temp;
				return true;
			}
			return false;
		}
		public bool Remove(KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			if (TryGetValue(item.Key, out value) && Equals(item.Value, value))
				return Remove(item.Key); // returns true;
			return false;
		}
		public void Clear()
		{
			_root = null;
		}
		/* Helper function that allocates a new node with the given key and 
			null left and right pointers. */
		_Node _CreateNode(TKey key,TValue value)
		{
			var result = new _Node();
			result.Key = key;
			result.Value = value;
			result.Left = result.Right = null;
			return result;
		}

		// A utility function to right rotate subtree rooted with y 
		// See the diagram given above. 
		_Node _Ror(_Node x)
		{
			_Node y = x.Left;
			x.Left = y.Right;
			y.Right = x;
			return y;
		}

		// A utility function to left rotate subtree rooted with x 
		// See the diagram given above. 
		_Node _Rol(_Node x)
		{
			_Node y = x.Right;
			x.Right = y.Left;
			y.Left = x;
			return y;
		}
		
		// This function brings the key at root if key is present in tree. 
		// If key is not present, then it brings the last accessed item at 
		// root.  This function modifies the tree and returns the new root 
		_Node _Splay(_Node root, TKey key)
		{
			// TODO: if this function wasn't recursive it could deal with more items
			int c;
			// Base cases: root is null or key is present at root 
			if (null==root || 0==(c = _comparer.Compare(root.Key, key)))
				return root;

			// Key lies in left subtree 
			if (0<c)
			{
				// Key is not in tree, we are done 
				if (null==root.Left)
					return root;
				c = _comparer.Compare(root.Left.Key, key);
				// Zig-Zig (Left Left) 
				if (0<c)
				{
					// First recursively bring the key as root of left-left 
					root.Left.Left = _Splay(root.Left.Left, key);

					// Do first rotation for root, second rotation is  
					// done after else 
					root = _Ror(root);
				}
				else if (0>c) // Zig-Zag (Left Right) 
				{
					// First recursively bring the key as root of left-right 
					root.Left.Right = _Splay(root.Left.Right, key);

					// Do first rotation for root.left 
					if (null!=root.Left.Right)
						root.Left = _Rol(root.Left);
				}

				// Do second rotation for root 
				return (null==root.Left) ? root : _Ror(root);
			}
			else // Key lies in right subtree 
			{
				// Key is not in tree, we are done 
				if (null==root.Right)
					return root;
				c = _comparer.Compare(root.Right.Key, key);
				// Zag-Zig (Right Left) 
				if (0<c)
				{
					// Bring the key as root of right-left 
					root.Right.Left = _Splay(root.Right.Left, key);

					// Do first rotation for root.right 
					if (null!=root.Right.Left)
						root.Right = _Ror(root.Right);
				}
				else if (0>c)// Zag-Zag (Right Right) 
				{
					// Bring the key as root of right-right and do  
					// first rotation 
					root.Right.Right = _Splay(root.Right.Right, key);
					root = _Rol(root);
				}

				// Do second rotation for root 
				return (null==root.Right ) ? root : _Rol(root);
			}
		}
		// Function to insert a new key k  
		// in splay tree with given root  
		_Node _Add(_Node root, TKey key,TValue value)
		{
			// Simple Case: If tree is empty  
			if (null==root) return _CreateNode(key,value);

			// Bring the closest leaf node to root  
			root = _Splay(root, key);
			var c = _comparer.Compare(root.Key, key);
			// If key is already present, then throw
			if (0 == c) throw new ArgumentException("An item with the specified key is already present in the dictionary.", nameof(key));

			// Otherwise allocate a new node  
			_Node newnode = _CreateNode(key,value);

			// If root's key is greater, make  
			// root as right child of newnode  
			// and copy the left child of root to newnode  
			if (0<c)
			{
				newnode.Right = root;
				newnode.Left = root.Left;
				root.Left = null;
			}

			// If root's key is smaller, make  
			// root as left child of newnode  
			// and copy the right child of root to newnode  
			else
			{
				newnode.Left = root;
				newnode.Right = root.Right;
				root.Right = null;
			}

			return newnode; // newnode becomes new root  
		}
		// The delete function for Splay tree. Note that this function 
		// returns the new root of Splay Tree after removing the key  
		bool _TryRemove(_Node root, TKey key, out _Node result)
		{
			result = null;
			_Node temp;
			if (null == root)
				return false;
			
			// Splay the given key     
			root = _Splay(root, key);

			// If key is not present, then 
			// return root 
			if (0 != _comparer.Compare(key, root.Key))
			{
				result = root;
				return false;
			}

			// If key is present 
			// If left child of root does not exist 
			// make root.right as root    
			if (null == root.Left)
			{
				temp = root;
				root = root.Right;
			}

			// Else if left child exits 
			else
			{
				temp = root;

				/*Note: Since key == root.key, 
				so after Splay(key, root.lchild), 
				the tree we get will have no right child tree 
				and maximum node in left subtree will get splayed*/
				// New root 
				root = _Splay(root.Left, key);

				// Make right child of previous root  as 
				// new root's right child 
				root.Right = temp.Right;
			}


			// return root of the new Splay Tree 
			result = root;
			return true;

		}
		public ICollection<TKey> Keys
			=> DictionaryUtility.CreateKeys(this);
		public ICollection<TValue> Values
			=> DictionaryUtility.CreateValues(this);

		public IEnumerator<KeyValuePair<TKey,TValue>> GetEnumerator()
		{
			foreach (var item in _EnumNodes(_root))
				yield return item;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> GetEnumerator();
		IEnumerable<KeyValuePair<TKey,TValue>> _EnumNodes(_Node root)
		{
			if (null!=root )
			{
				foreach (var item in _EnumNodes(root.Left))
					yield return item;
				foreach (var item in _EnumNodes(root.Right))
					yield return item;
				yield return new KeyValuePair<TKey, TValue>(root.Key, root.Value);

			}
		}
		int _GetCount(_Node root)
		{
			if (null == root)
				return 0;
			var result = 1;
			result += _GetCount(root.Left);
			result += _GetCount(root.Right);
			return result;
		}
		#region _Node
		private sealed class _Node
		{

			public TKey Key;
			public TValue Value;
			public _Node Left;
			public _Node Right;
		}
		#endregion

	}
}