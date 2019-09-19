using System;
using System.Collections.Generic;
using System.Diagnostics;
// port and largely a rewrite of https://www.geeksforgeeks.org/introduction-of-b-tree-2/
namespace Bee
{
	// A BTree implemented as a sorted dictionary
	// Keys must implement IComparable<TKey> or a Comparer<TKey> must be specified
	[DebuggerDisplay("Count = {Count}")]
	public class SortedBTreeDictionary<TKey,TValue> : IDictionary<TKey,TValue>
	{
		IComparer<TKey> _comparer;
		const int _DefaultMininumDegree = 3;
		_Node _root; 
		int _minimumDegree;  // Minimum degree 
		int _count;
		public SortedBTreeDictionary(int minimumDegree,IComparer<TKey> comparer)
		{
			_count = 0;
			_comparer = comparer ?? Comparer<TKey>.Default;
			_root = null;
			_minimumDegree = minimumDegree;
			
		}
		public SortedBTreeDictionary(int minimumDegree) : this(minimumDegree,null) { }
		public SortedBTreeDictionary(IComparer<TKey> comparer) : this(_DefaultMininumDegree,comparer)
		{
			
		}
		public SortedBTreeDictionary() : this(_DefaultMininumDegree) { }
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			=> _EnumNodes(_root).GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> GetEnumerator();
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
			=> CollectionUtility.CopyTo(this, array, index);
		
		public bool ContainsKey(TKey key)
		{
			TValue v;
			var n = _SearchInterior(_root, key);
			if (null != n && _TryGet(n, key, out v))
				return true;
			return false;
			
		}
		public bool Contains(KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			return TryGetValue(item.Key, out value) && Equals(value, item.Value);
		}
		public bool TryGetValue(TKey key,out TValue value)
		{
			if(null!=_root)
			{
				var node = _SearchInterior(_root,key);
				if (null != node)
					return _TryGet(node,key, out value);
			}
			value = default(TValue);
			return false;
		}
		public TValue this[TKey key] {
			get {
				TValue value;
				if (TryGetValue(key, out value))
					return value;
				throw new KeyNotFoundException();
			}
			set {
				if(null!=_root)
				{
					var node = _SearchInterior(_root,key);
					if (null != node && _TrySet(node,key, value))
						return;
				}
				_Add(key, value);
				++_count;
			}
		}
		public int Count {
			get {
				return _count;
			}
		}
		public void Add(TKey key, TValue value)
		{
			if (ContainsKey(key))
				throw new ArgumentException("The specified key already exists in the dictionary.", nameof(key));
			_Add(key, value);
			++_count;
		}
		// adds an item (no validation)
		void _Add(TKey key,TValue value)
		{
			// tree is empty?
			if (null == _root)
			{
				_root = new _Node(_minimumDegree, true);
				_root.Items[0] = new KeyValuePair<TKey, TValue>(key,value);  // Insert key 
				_root.KeyCount = 1;
				return;
			}

			// grow tree if root is full
			if (_root.KeyCount == 2 * _minimumDegree - 1)
			{
				_Node newRoot = new _Node(_minimumDegree, false);

				newRoot.Children[0] = _root;
				_Split(newRoot,0, _root);
				// figure out which child gets the key (sort)
				int i = 0;
				if (0>_comparer.Compare(newRoot.Items[0].Key,key))
					++i;
				_Add(newRoot.Children[i],key,value);

				_root = newRoot;
			}
			else  // just insert
				_Add(_root,key,value);
		}
		public void Add(KeyValuePair<TKey,TValue> kvp)
			=>Add(kvp.Key, kvp.Value);
		public bool Remove(KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			if(TryGetValue(item.Key,out value) && Equals(item.Value,value))
				return Remove(item.Key);
			return false;
		}
		public bool Remove(TKey k)
		{
			if (null != _root)
			{
				if (!_Remove(_root,k))
				{
					// if root node has 0 keys collapse the tree by one
					if (0 == _root.KeyCount)
					{
						_Node tmp = _root;
						if (_root.IsLeaf)
							_root = null;
						else
							_root = _root.Children[0];
					}
				}
				--_count;
				return true;
			}
			return false;
		}
		public void Clear()
		{
			// just erase the root and set the count
			_root = null;
			_count = 0;
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
		public ICollection<TKey> Keys
			=> CollectionUtility.CreateKeys(this);
		public ICollection<TValue> Values
			=> CollectionUtility.CreateValues(this);

		static int _GetHeight(_Node node)
		{
			if (null == node)
				return 0;
			if (node.IsLeaf)
				return 1;
			int max = 0;
			for(var i = 0;i<node.Children.Length;i++)
			{
				var m = _GetHeight(node.Children[i]);
				if (m > max)
					max = m;
			}
			return 1 + max;
		}
		public int Height {
			get {
				
				return _GetHeight(_root);
				
			}
		}
		// the root must not be full when this is called.
		void _Add(_Node t,TKey key, TValue value)
		{
			int i = t.KeyCount - 1;
			if (t.IsLeaf)
			{
				// shift items
				while (i >= 0 && 0 < _comparer.Compare(t.Items[i].Key, key))
				{
					t.Items[i + 1] = t.Items[i];
					--i;
				}

				t.Items[i + 1] = new KeyValuePair<TKey, TValue>(key, value);
				++t.KeyCount;
			}
			else // is not leaf
			{
				while (i >= 0 && 0 < _comparer.Compare(t.Items[i].Key, key))
					i--;
				if (t.Children[i + 1].KeyCount == 2 * _minimumDegree - 1)
				{
					_Split(t,i + 1, t.Children[i + 1]);
					if (0 > _comparer.Compare(t.Items[i + 1].Key, key))
						++i;
				}
				_Add(t.Children[i + 1],key, value);
			}
		}
		// A function to remove the key k from the sub-tree rooted with this node 
		bool _Remove(_Node t,TKey k)
		{
			int idx = _GetIndexOfKey(t,k);

			// The key to be removed is present in this node 
			if (idx < t.KeyCount && 0 == _comparer.Compare(t.Items[idx].Key, k))
			{

				// If the node is a leaf node - removeFromLeaf is called 
				// Otherwise, removeFromNonLeaf function is called 
				if (t.IsLeaf)
					_RemoveFromLeaf(t,idx);
				else
					_RemoveFromNonLeaf(t,idx);
			}
			else
			{

				// If this node is a leaf node, then the key is not present in tree 
				if (t.IsLeaf)
					return false;
				
				// The key to be removed is present in the sub-tree rooted with this node 
				// The flag indicates whether the key is present in the sub-tree rooted 
				// with the last child of this node 
				bool flag = ((idx == t.KeyCount) ? true : false);

				// If the child where the key is supposed to exist has less that t keys, 
				// we fill that child 
				if (t.Children[idx].KeyCount < _minimumDegree)
					_Fill(t,idx);

				// If the last child has been merged, it must have merged with the previous 
				// child and so we recurse on the (idx-1)th child. Else, we recurse on the 
				// (idx)th child which now has atleast t keys 
				if (flag && idx > t.KeyCount)
					_Remove(t.Children[idx - 1],k);
				else
					_Remove(t.Children[idx],k);
			}
			return true;
		}
		// A function to remove the idx-th key from this node - which is a non-leaf node 
		void _RemoveFromNonLeaf(_Node t,int idx)
		{

			TKey k = t.Items[idx].Key;

			// If the child that precedes k (C[idx]) has atleast t keys, 
			// find the predecessor 'pred' of k in the subtree rooted at 
			// C[idx]. Replace k by pred. Recursively delete pred 
			// in C[idx] 
			if (t.Children[idx].KeyCount >= _minimumDegree)
			{
				var pred = _GetPreviousItem(t,idx);
				t.Items[idx] = pred;
				_Remove(t.Children[idx],pred.Key);
			}

			// If the child C[idx] has less that t keys, examine C[idx+1]. 
			// If C[idx+1] has atleast t keys, find the successor 'succ' of k in 
			// the subtree rooted at C[idx+1] 
			// Replace k by succ 
			// Recursively delete succ in C[idx+1] 
			else if (t.Children[idx + 1].KeyCount >= _minimumDegree)
			{
				var succ = _GetNextItem(t,idx);
				t.Items[idx] = succ;
				_Remove(t.Children[idx + 1],succ.Key);
			}

			// If both C[idx] and C[idx+1] has less that t keys,merge k and all of C[idx+1] 
			// into C[idx] 
			// Now C[idx] contains 2t-1 keys 
			// Free C[idx+1] and recursively delete k from C[idx] 
			else
			{
				_Merge(t,idx);
				_Remove(t.Children[idx],k);
			}
			return;
		}
		// A function to remove the idx-th key from this node - which is a leaf node 
		static void _RemoveFromLeaf(_Node t,int idx)
		{

			// Move all the keys after the idx-th pos one place backward 
			for (int i = idx + 1; i < t.KeyCount; ++i)
				t.Items[i - 1] = t.Items[i];

			// Reduce the count of keys 
			--t.KeyCount;

			return;
		}
		static KeyValuePair<TKey, TValue> _GetNextItem(_Node t,int idx)
		{

			// Keep moving the left most node starting from C[idx+1] until we reach a leaf 
			_Node cur = t.Children[idx + 1];
			while (!cur.IsLeaf)
				cur = cur.Children[0];

			// Return the first key of the leaf 
			return cur.Items[0];
		}
		// A utility function that returns the index of the first key that is 
		// greater than or equal to k 
		int _GetIndexOfKey(_Node t,TKey k)
		{
			int idx = 0;
			while (idx < t.KeyCount && 0 > _comparer.Compare(t.Items[idx].Key, k))
				++idx;
			return idx;
		}
		
		// A function to get predecessor of keys[idx] 
		static KeyValuePair<TKey, TValue> _GetPreviousItem(_Node t,int idx)
		{
			// Keep moving to the right most node until we reach a leaf 
			_Node cur = t.Children[idx];
			while (!cur.IsLeaf)
				cur = cur.Children[cur.KeyCount];

			// Return the last key of the leaf 
			return cur.Items[cur.KeyCount - 1];
		}
		// A function to borrow an item from C[idx-1] and insert it 
		// into C[idx] 
		static void _BorrowFromPrevious(_Node t,int idx)
		{

			_Node child = t.Children[idx];
			_Node sibling = t.Children[idx - 1];

			// The last key from C[idx-1] goes up to the parent and key[idx-1] 
			// from parent is inserted as the first key in C[idx]. Thus, the  loses 
			// sibling one key and child gains one key 

			// Moving all key in C[idx] one step ahead 
			for (int i = child.KeyCount - 1; i >= 0; --i)
				child.Items[i + 1] = child.Items[i];

			// If C[idx] is not a leaf, move all its child pointers one step ahead 
			if (!child.IsLeaf)
			{
				for (int i = child.KeyCount; i >= 0; --i)
					child.Children[i + 1] = child.Children[i];
			}

			// Setting child's first key equal to keys[idx-1] from the current node 
			child.Items[0] = t.Items[idx - 1];

			// Moving sibling's last child as C[idx]'s first child 
			if (!child.IsLeaf)
				child.Children[0] = sibling.Children[sibling.KeyCount];

			// Moving the key from the sibling to the parent 
			// This reduces the number of keys in the sibling 
			t.Items[idx - 1] = sibling.Items[sibling.KeyCount - 1];

			child.KeyCount += 1;
			sibling.KeyCount -= 1;

			return;
		}

		// A function to borrow a key from the C[idx+1] and place 
		// it in C[idx] 
		static void _BorrowFromNext(_Node t,int idx)
		{

			_Node child = t.Children[idx];
			_Node sibling = t.Children[idx + 1];

			// keys[idx] is inserted as the last key in C[idx] 
			child.Items[child.KeyCount] = t.Items[idx];

			// Sibling's first child is inserted as the last child 
			// into C[idx] 
			if (!child.IsLeaf)
				child.Children[(child.KeyCount) + 1] = sibling.Children[0];

			//The first key from sibling is inserted into keys[idx] 
			t.Items[idx] = sibling.Items[0];

			// Moving all keys in sibling one step behind 
			for (int i = 1; i < sibling.KeyCount; ++i)
				sibling.Items[i - 1] = sibling.Items[i];

			// Moving the child pointers one step behind 
			if (!sibling.IsLeaf)
			{
				for (int i = 1; i <= sibling.KeyCount; ++i)
					sibling.Children[i - 1] = sibling.Children[i];
			}

			// Increasing and decreasing the key count of C[idx] and C[idx+1] 
			// respectively 
			++child.KeyCount;
			--sibling.KeyCount;

			return;
		}
		// A function to merge C[idx] with C[idx+1] 
		// C[idx+1] is freed after merging 
		void _Merge(_Node t,int idx)
		{
			_Node child = t.Children[idx];
			_Node sibling = t.Children[idx + 1];

			// Pulling a key from the current node and inserting it into (t-1)th 
			// position of C[idx] 
			child.Items[_minimumDegree - 1] = t.Items[idx];

			// Copying the keys from C[idx+1] to C[idx] at the end 
			for (int i = 0; i < sibling.KeyCount; ++i)
				child.Items[i + _minimumDegree] = sibling.Items[i];

			// Copying the child pointers from C[idx+1] to C[idx] 
			if (!child.IsLeaf)
			{
				for (int i = 0; i <= sibling.KeyCount; ++i)
					child.Children[i + _minimumDegree] = sibling.Children[i];
			}

			// Moving all keys after idx in the current node one step before - 
			// to fill the gap created by moving keys[idx] to C[idx] 
			for (int i = idx + 1; i < t.KeyCount; ++i)
				t.Items[i - 1] = t.Items[i];

			// Moving the child pointers after (idx+1) in the current node one 
			// step before 
			for (int i = idx + 2; i <= t.KeyCount; ++i)
				t.Children[i - 1] = t.Children[i];

			// Updating the key count of child and the current node 
			child.KeyCount += sibling.KeyCount + 1;
			--t.KeyCount;

			return;
		}
		// A function to fill child C[idx] which has less than t-1 keys 
		void _Fill(_Node t,int idx)
		{

			// If the previous child(C[idx-1]) has more than t-1 keys, borrow a key 
			// from that child 
			if (idx != 0 && t.Children[idx - 1].KeyCount >= _minimumDegree)
				_BorrowFromPrevious(t,idx);

			// If the next child(C[idx+1]) has more than t-1 keys, borrow a key 
			// from that child 
			else if (idx != t.KeyCount && t.Children[idx + 1].KeyCount >= _minimumDegree)
				_BorrowFromNext(t,idx);

			// Merge C[idx] with its sibling 
			// If C[idx] is the last child, merge it with with its previous sibling 
			// Otherwise merge it with its next sibling 
			else
			{
				if (idx != t.KeyCount)
					_Merge(t,idx);
				else if (0 != idx)
					_Merge(t,idx - 1);
			}
			return;
		}

		void _Split(_Node t,int i, _Node target)
		{
			// Create a new node which is going to store (t-1) keys 
			// of y 
			_Node newNode = new _Node(_minimumDegree, target.IsLeaf);
			newNode.KeyCount = _minimumDegree - 1;

			// Copy the last (t-1) keys of y to z 
			for (int j = 0; j < _minimumDegree - 1; j++)
				newNode.Items[j] = target.Items[j + _minimumDegree];

			// Copy the last t children of y to z 
			if (!target.IsLeaf)
			{
				for (int j = 0; j < _minimumDegree; j++)
					newNode.Children[j] = target.Children[j + _minimumDegree];
			}

			// Reduce the number of keys in y 
			target.KeyCount = _minimumDegree - 1;

			// Since this node is going to have a new child, 
			// create space of new child 
			for (int j = t.KeyCount; j >= i + 1; j--)
				t.Children[j + 1] = t.Children[j];

			// Link the new child to this node 
			t.Children[i + 1] = newNode;

			// A key of y will move to this node. Find the location of 
			// new key and move all greater keys one space ahead 
			for (int j = t.KeyCount - 1; j >= i; j--)
				t.Items[j + 1] = t.Items[j];

			// Copy the middle key of y to this node 
			t.Items[i] = target.Items[_minimumDegree - 1];

			// Increment count of keys in this node 
			++t.KeyCount;
		}
		_Node _SearchInterior(_Node t,TKey k)
		{
			if (null == t) return null;
			// returns NULL if k is not present. 
			// Find the first key greater than or equal to k 
			var i = 0;
		
			while (i < t.KeyCount && 0 < _comparer.Compare(k, t.Items[i].Key))
				++i;
			// If the found key is equal to k, return this node 
			if (i >= t.KeyCount)
				return null;
			if (0 == _comparer.Compare(t.Items[i].Key, k))
				return t;

			// If the key is not found here and this is a leaf node 
			if (t.IsLeaf)
				return null;

			// Go to the appropriate child 
			return _SearchInterior(t.Children[i],k);
		}
		bool _TryGet(_Node t,TKey k, out TValue v)
		{   // returns NULL if k is not present. 
			// Find the first key greater than or equal to k 
			var i = 0;
			while (i < t.KeyCount && 0 < _comparer.Compare(k, t.Items[i].Key))
				i++;

			// If the found key is equal to k, return this node 
			var item = t.Items[i];
			if (0 == _comparer.Compare(item.Key, k))
			{
				v = item.Value;
				return true;
			}

			// If the key is not found here and this is a leaf node 
			if (t.IsLeaf)
			{
				v = default(TValue);
				return false;
			}

			// Go to the appropriate child 
			return _TryGet(t.Children[i],k, out v);
		}
		bool _TrySet(_Node t,TKey k, TValue v)
		{   // returns false if k is not present. 
			// Find the first key greater than or equal to k 
			if (null == t) return false;
			int i = 0;
			while (i < t.KeyCount && 0 < _comparer.Compare(k, t.Items[i].Key))
				i++;

			// If the found key is equal to k, set this node 
			var item = t.Items[i];
			if (0 == _comparer.Compare(item.Key, k))
			{
				t.Items[i] = new KeyValuePair<TKey, TValue>(k, v);
				return true;
			}

			// If the key is not found here and this is a leaf node 
			if (t.IsLeaf)
			{
				v = default(TValue);
				return false;
			}

			// Go to the appropriate child and set it
			return _TrySet(t.Children[i],k, v);
		}
		// A function to traverse all nodes in a subtree rooted with this node 
		static IEnumerable<KeyValuePair<TKey, TValue>> _EnumNodes(_Node t)
		{
			if (null != t)
			{
				int i;
				for (i = 0; i < t.KeyCount; ++i)
				{
					// If this is not leaf, then before returning Item[i], 
					// traverse the subtree rooted with child Children[i]. 
					if (!t.IsLeaf)
						foreach (var item in _EnumNodes(t.Children[i]))
							yield return item;
					yield return t.Items[i];
				}

				// report the subtree rooted with last child 
				if (!t.IsLeaf)
					foreach (var item in _EnumNodes(t.Children[i]))
						yield return item;
			}
		}

		#region _Node
		private sealed class _Node  
		{
			public KeyValuePair<TKey, TValue>[] Items;  
			public _Node[] Children; 
			public int KeyCount;
			public bool IsLeaf;
			public _Node(int minimumDegree, bool isLeaf)    
			{
				IsLeaf = isLeaf;

				// allocate array for maximum number of possible keys per node
				// and child pointers 
				Items = new KeyValuePair<TKey, TValue>[2 * minimumDegree - 1];
				Children = new _Node[2 * minimumDegree];

				KeyCount = 0;
			}
			
		
		}
		#endregion	
	}
}

