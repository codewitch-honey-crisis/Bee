using System;
using System.Collections.Generic;
// port and largely a rewrite of https://www.geeksforgeeks.org/introduction-of-b-tree-2/
namespace Bee
{
	// A BTree implemented as a sorted dictionary
	// Keys must implement IComparable<TKey> or a Comparer<TKey> must be specified
	public class SortedBTreeDictionary<TKey,TValue> : IDictionary<TKey,TValue>
	{
		IComparer<TKey> _comparer;
		const int _DefaultMininumDegree = 3;
		_Node root; 
		int _minimumDegree;  // Minimum degree 
		public SortedBTreeDictionary(int minimumDegree,IComparer<TKey> comparer)
		{
			_comparer = comparer ?? Comparer<TKey>.Default;
			root = null;
			_minimumDegree = minimumDegree;
			
		}
		public SortedBTreeDictionary(int minimumDegree) : this(minimumDegree,null) { }
		public SortedBTreeDictionary(IComparer<TKey> comparer) : this(_DefaultMininumDegree,comparer)
		{
			
		}
		public SortedBTreeDictionary() : this(_DefaultMininumDegree) { }
		public IEnumerator<KeyValuePair<TKey,TValue>> GetEnumerator()
		{
			if (null != root)
				foreach (var item in root)
					yield return item;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> GetEnumerator();
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
			=> DictionaryUtility.CopyTo(this, array, index);
		
		public bool ContainsKey(TKey key)
		{
			if (null != root)
			{
				return root.ContainsKey(key);
			}
			return false;
		}
		public bool Contains(KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			return TryGetValue(item.Key, out value) && Equals(value, item.Value);
		}
		public bool TryGetValue(TKey key,out TValue value)
		{
			if(null!=root)
			{
				var node = root.Search(key);
				if (null != node)
					return node.TryGet(key, out value);
			}
			value = default(TValue);
			return false;
		}
		public TValue this[TKey key] {
			get {
				if(null!=root)
				{
					var node = root.Search(key);
					if (null != node)
					{
						TValue result;
						if (node.TryGet(key, out result))
							return result;
					}
				}
				throw new KeyNotFoundException();
			}
			set {
				if(null!=root)
				{
					var node = root.Search(key);
					if (null != node && node.TrySet(key, value))
						return;
				}
				_Add(key, value);
			}
		}
		public int Count {
			get {
				if (null == root)
					return 0;
				return root.GetItemCount();
			}
		}
		public void Add(TKey key, TValue value)
		{
			if (ContainsKey(key))
				throw new ArgumentException("The specified key already exists in the dictionary.", nameof(key));
			_Add(key, value);
		}
		// adds an item (no validation)
		void _Add(TKey key,TValue value)
		{
			// tree is empty?
			if (null == root)
			{
				root = new _Node(_comparer,_minimumDegree, true);
				root.Items[0] = new KeyValuePair<TKey, TValue>(key,value);  // Insert key 
				root.KeyCount = 1; 
				return;
			}

			// grow tree if root is full
			if (root.KeyCount == 2 * _minimumDegree - 1)
			{
				_Node newRoot = new _Node(_comparer,_minimumDegree, false);

				newRoot.Children[0] = root;
				newRoot.Split(0, root);
				// figure out which child gets the key (sort)
				int i = 0;
				if (0>_comparer.Compare(newRoot.Items[0].Key,key))
					++i;
				newRoot.Children[i].Insert(key,value);

				root = newRoot;
			}
			else  // just insert
				root.Insert(key,value);
			
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
			if (null != root)
			{
				if (!root.Remove(k))
				{
					// if root node has 0 keys collapse the tree by one
					if (0 == root.KeyCount)
					{
						_Node tmp = root;
						if (root.IsLeaf)
							root = null;
						else
							root = root.Children[0];
					}
				}
				return true;
			}
			return false;
		}
		public void Clear()
		{
			// just erase the root
			root = null;
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
		public ICollection<TKey> Keys
			=> DictionaryUtility.CreateKeys(this);
		public ICollection<TValue> Values
			=> DictionaryUtility.CreateValues(this);

		#region _Node
		private sealed class _Node : IEnumerable<KeyValuePair<TKey, TValue>> 
		{
			IComparer<TKey> _comparer;
			int _minimumDegree; 
			internal KeyValuePair<TKey, TValue>[] Items;  
			internal _Node[] Children; 
			internal int KeyCount;
			internal bool IsLeaf;
			public _Node(IComparer<TKey> comparer, int minimumDegree, bool isLeaf)    
			{
				_comparer = comparer;
				_minimumDegree = minimumDegree;
				IsLeaf = isLeaf;

				// allocate array for maximum number of possible keys per node
				// and child pointers 
				Items = new KeyValuePair<TKey, TValue>[2 * _minimumDegree - 1];
				Children = new _Node[2 * _minimumDegree];

				KeyCount = 0;
			}
			// the root must not be full when this is called.
			internal void Insert(TKey key, TValue value)
			{
				int i = KeyCount - 1;
				if (IsLeaf)
				{
					// shift items
					while (i >= 0 && 0 < _comparer.Compare(Items[i].Key,key))
					{
						Items[i + 1] = Items[i];
						--i;
					}

					Items[i + 1] = new KeyValuePair<TKey, TValue>(key, value);
					++KeyCount;
				}
				else // is leaf
				{
					while (i >= 0 && 0 < _comparer.Compare(Items[i].Key,key))
						i--;
					if (Children[i + 1].KeyCount == 2 * _minimumDegree - 1)
					{
						Split(i + 1, Children[i + 1]);
						if (0 > _comparer.Compare(Items[i + 1].Key,key))
							++i;
					}
					Children[i + 1].Insert(key, value);
				}
			}
			internal void Split(int i, _Node target)
			{
				// Create a new node which is going to store (t-1) keys 
				// of y 
				_Node newNode = new _Node(_comparer,target._minimumDegree, target.IsLeaf);
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
				for (int j = KeyCount; j >= i + 1; j--)
					Children[j + 1] = Children[j];

				// Link the new child to this node 
				Children[i + 1] = newNode;

				// A key of y will move to this node. Find the location of 
				// new key and move all greater keys one space ahead 
				for (int j = KeyCount - 1; j >= i; j--)
					Items[j + 1] = Items[j];

				// Copy the middle key of y to this node 
				Items[i] = target.Items[_minimumDegree - 1];

				// Increment count of keys in this node 
				++KeyCount;
			}
			// A function to traverse all nodes in a subtree rooted with this node 
			public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				int i;
				for (i = 0; i < KeyCount; ++i)
				{
					// If this is not leaf, then before returning Item[i], 
					// traverse the subtree rooted with child Children[i]. 
					if (!IsLeaf)
						foreach (var item in Children[i])
							yield return item;
					yield return Items[i];
				}

				// report the subtree rooted with last child 
				if (!IsLeaf)
					foreach (var item in Children[i])
						yield return item;
			}
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
				=> GetEnumerator();
			// A function to search a key in the subtree rooted with this node.     
			public _Node Search(TKey k)
			{   // returns NULL if k is not present. 
				// Find the first key greater than or equal to k 
				var i = 0;
				while (i < KeyCount && 0 < _comparer.Compare(k,Items[i].Key))
					++i;
				// If the found key is equal to k, return this node 
				if (i >= KeyCount)
					return null;
				if (0 == _comparer.Compare(Items[i].Key,k))
					return this;

				// If the key is not found here and this is a leaf node 
				if (IsLeaf)
					return null;

				// Go to the appropriate child 
				return Children[i].Search(k);
			}
			internal bool TryGet(TKey k, out TValue v)
			{   // returns NULL if k is not present. 
				// Find the first key greater than or equal to k 
				var i = 0;
				while (i < KeyCount && 0 < _comparer.Compare(k,Items[i].Key))
					i++;

				// If the found key is equal to k, return this node 
				var item = Items[i];
				if (0 == _comparer.Compare(item.Key,k))
				{
					v = item.Value;
					return true;
				}

				// If the key is not found here and this is a leaf node 
				if (IsLeaf)
				{
					v = default(TValue);
					return false;
				}

				// Go to the appropriate child 
				return Children[i].TryGet(k, out v);
			}
			internal bool TrySet(TKey k, TValue v)
			{   // returns false if k is not present. 
				// Find the first key greater than or equal to k 
				int i = 0;
				while (i < KeyCount && 0 < _comparer.Compare(k,Items[i].Key))
					i++;

				// If the found key is equal to k, set this node 
				var item = Items[i];
				if (0 == _comparer.Compare(item.Key,k))
				{
					Items[i] = new KeyValuePair<TKey, TValue>(k, v);
					return true;
				}

				// If the key is not found here and this is a leaf node 
				if (IsLeaf)
				{
					v = default(TValue);
					return false;
				}

				// Go to the appropriate child and set it
				return Children[i].TrySet(k, v);
			}
			public bool ContainsKey(TKey k)
			{   // returns false if k is not present. 
				// Find the first key greater than or equal to k 
				int i = 0;
				while (i < KeyCount && 0 < _comparer.Compare(k,Items[i].Key))
					i++;
				if (i >= KeyCount)
					return false;
				// If the found key is equal to k, return this node 
				var item = Items[i];
				if (0 == _comparer.Compare(item.Key,k))
					return true;
				// If the key is not found here and this is a leaf node 
				if (IsLeaf)
					return false;

				// Go to the appropriate child 
				return Children[i].ContainsKey(k);
			}

			// A utility function that returns the index of the first key that is 
			// greater than or equal to k 
			int _GetIndexOfKey(TKey k)
			{
				int idx = 0;
				while (idx < KeyCount && 0 > _comparer.Compare(Items[idx].Key,k))
					++idx;
				return idx;
			}

			// A function to remove the key k from the sub-tree rooted with this node 
			internal bool Remove(TKey k)
			{
				int idx = _GetIndexOfKey(k);

				// The key to be removed is present in this node 
				if (idx < KeyCount && 0 == _comparer.Compare(Items[idx].Key,k))
				{

					// If the node is a leaf node - removeFromLeaf is called 
					// Otherwise, removeFromNonLeaf function is called 
					if (IsLeaf)
						_RemoveFromLeaf(idx);
					else
						_RemoveFromNonLeaf(idx);
				}
				else
				{

					// If this node is a leaf node, then the key is not present in tree 
					if (IsLeaf)
					{
						return false;
						//cout << "The key " << k << " is does not exist in the tree\n";
						//return;
					}

					// The key to be removed is present in the sub-tree rooted with this node 
					// The flag indicates whether the key is present in the sub-tree rooted 
					// with the last child of this node 
					bool flag = ((idx == KeyCount) ? true : false);

					// If the child where the key is supposed to exist has less that t keys, 
					// we fill that child 
					if (Children[idx].KeyCount < _minimumDegree)
						_Fill(idx);

					// If the last child has been merged, it must have merged with the previous 
					// child and so we recurse on the (idx-1)th child. Else, we recurse on the 
					// (idx)th child which now has atleast t keys 
					if (flag && idx > KeyCount)
						Children[idx - 1].Remove(k);
					else
						Children[idx].Remove(k);
				}
				return true;
			}

			// A function to remove the idx-th key from this node - which is a leaf node 
			void _RemoveFromLeaf(int idx)
			{

				// Move all the keys after the idx-th pos one place backward 
				for (int i = idx + 1; i < KeyCount; ++i)
					Items[i - 1] = Items[i];

				// Reduce the count of keys 
				--KeyCount;

				return;
			}

			// A function to remove the idx-th key from this node - which is a non-leaf node 
			void _RemoveFromNonLeaf(int idx)
			{

				TKey k = Items[idx].Key;

				// If the child that precedes k (C[idx]) has atleast t keys, 
				// find the predecessor 'pred' of k in the subtree rooted at 
				// C[idx]. Replace k by pred. Recursively delete pred 
				// in C[idx] 
				if (Children[idx].KeyCount >= _minimumDegree)
				{
					var pred = _GetPreviousItem(idx);
					Items[idx] = pred;
					Children[idx].Remove(pred.Key);
				}

				// If the child C[idx] has less that t keys, examine C[idx+1]. 
				// If C[idx+1] has atleast t keys, find the successor 'succ' of k in 
				// the subtree rooted at C[idx+1] 
				// Replace k by succ 
				// Recursively delete succ in C[idx+1] 
				else if (Children[idx + 1].KeyCount >= _minimumDegree)
				{
					var succ = _GetNextItem(idx);
					Items[idx] = succ;
					Children[idx + 1].Remove(succ.Key);
				}

				// If both C[idx] and C[idx+1] has less that t keys,merge k and all of C[idx+1] 
				// into C[idx] 
				// Now C[idx] contains 2t-1 keys 
				// Free C[idx+1] and recursively delete k from C[idx] 
				else
				{
					_Merge(idx);
					Children[idx].Remove(k);
				}
				return;
			}
			internal int GetItemCount()
			{
				// There are n keys and n+1 children, travers through n keys 
				// and first n children 
				int i;
				var result = 0;
				for (i = 0; i < KeyCount; ++i)
				{
					// If this is not leaf, then before returning Item[i], 
					// traverse the subtree rooted with child Children[i]. 
					if (!IsLeaf)
						result += Children[i].GetItemCount();

					++result;
					//	result += KeyCount;
				}

				// report the subtree rooted with last child 
				if (!IsLeaf)
					result += Children[i].GetItemCount();
				return result;
			}
			// A function to get predecessor of keys[idx] 
			KeyValuePair<TKey, TValue> _GetPreviousItem(int idx)
			{
				// Keep moving to the right most node until we reach a leaf 
				_Node cur = Children[idx];
				while (!cur.IsLeaf)
					cur = cur.Children[cur.KeyCount];

				// Return the last key of the leaf 
				return cur.Items[cur.KeyCount - 1];
			}

			KeyValuePair<TKey, TValue> _GetNextItem(int idx)
			{

				// Keep moving the left most node starting from C[idx+1] until we reach a leaf 
				_Node cur = Children[idx + 1];
				while (!cur.IsLeaf)
					cur = cur.Children[0];

				// Return the first key of the leaf 
				return cur.Items[0];
			}

			// A function to fill child C[idx] which has less than t-1 keys 
			void _Fill(int idx)
			{

				// If the previous child(C[idx-1]) has more than t-1 keys, borrow a key 
				// from that child 
				if (idx != 0 && Children[idx - 1].KeyCount >= _minimumDegree)
					_BorrowFromPrevious(idx);

				// If the next child(C[idx+1]) has more than t-1 keys, borrow a key 
				// from that child 
				else if (idx != KeyCount && Children[idx + 1].KeyCount >= _minimumDegree)
					_BorrowFromNext(idx);

				// Merge C[idx] with its sibling 
				// If C[idx] is the last child, merge it with with its previous sibling 
				// Otherwise merge it with its next sibling 
				else
				{
					if (idx != KeyCount)
						_Merge(idx);
					else if(0!=idx)
						_Merge(idx - 1);
				}
				return;
			}

			// A function to borrow an item from C[idx-1] and insert it 
			// into C[idx] 
			void _BorrowFromPrevious(int idx)
			{

				_Node child = Children[idx];
				_Node sibling = Children[idx - 1];

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
				child.Items[0] = Items[idx - 1];

				// Moving sibling's last child as C[idx]'s first child 
				if (!child.IsLeaf)
					child.Children[0] = sibling.Children[sibling.KeyCount];

				// Moving the key from the sibling to the parent 
				// This reduces the number of keys in the sibling 
				Items[idx - 1] = sibling.Items[sibling.KeyCount - 1];

				child.KeyCount += 1;
				sibling.KeyCount -= 1;

				return;
			}

			// A function to borrow a key from the C[idx+1] and place 
			// it in C[idx] 
			void _BorrowFromNext(int idx)
			{

				_Node child = Children[idx];
				_Node sibling = Children[idx + 1];

				// keys[idx] is inserted as the last key in C[idx] 
				child.Items[child.KeyCount] = Items[idx];

				// Sibling's first child is inserted as the last child 
				// into C[idx] 
				if (!child.IsLeaf)
					child.Children[(child.KeyCount) + 1] = sibling.Children[0];

				//The first key from sibling is inserted into keys[idx] 
				Items[idx] = sibling.Items[0];

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
			void _Merge(int idx)
			{
				_Node child = Children[idx];
				_Node sibling = Children[idx + 1];

				// Pulling a key from the current node and inserting it into (t-1)th 
				// position of C[idx] 
				child.Items[_minimumDegree - 1] = Items[idx];

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
				for (int i = idx + 1; i < KeyCount; ++i)
					Items[i - 1] = Items[i];

				// Moving the child pointers after (idx+1) in the current node one 
				// step before 
				for (int i = idx + 2; i <= KeyCount; ++i)
					Children[i - 1] = Children[i];

				// Updating the key count of child and the current node 
				child.KeyCount += sibling.KeyCount + 1;
				--KeyCount;

				return;
			}
			// Make the BTree friend of this so that we can access private members of this 
			// class in BTree functions 
			//friend class BTree;
		};
		#endregion
		
	}

}

