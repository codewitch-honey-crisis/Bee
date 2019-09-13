using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee
{

	// started with this, but by now, between porting, bugfixes, and 
	// retooling to .NETisms, is almost all rewritten
	// https://gist.github.com/Harish-R/097688ac7f48bcbadfa5
	public class SortedAvlTreeDictionary<TKey,TValue> : IDictionary<TKey,TValue>
	{
		#region _Node
		private sealed class _Node
		{
			public TKey Key;
			public TValue Value;
			public _Node Left;
			public _Node Right;
			public int Height;
		}
		#endregion

		_Node _root;
		IComparer<TKey> _comparer;
		
		_Node _Search(TKey x, _Node t)
		{
			if (null == t)
				return null;
			int c = _comparer.Compare(x, t.Key);
			if (0>c)
			{
				if (0==c) // should never happen
				{
					return t;
				}
				else
					return _Search(x, t.Left);
			}
			else
			{
				if (0==c)
				{
					return t;
				}
				else
					return _Search(x, t.Right);
			}

		}
		_Node _Add(TKey x,TValue v, _Node t)
		{
			int c;
			if (t == null)
			{
				t = new _Node();
				t.Key = x;
				t.Value = v;
				t.Height = 0;
				t.Left = t.Right = null;
			}
			else if (0>(c = _comparer.Compare(x, t.Key)))
			{
				t.Left = _Add(x,v, t.Left);
				if (_GetHeight(t.Left) - _GetHeight(t.Right) == 2)
				{
					if (0>_comparer.Compare(x , t.Left.Key))
						t = _Ror(t);
					else
						t = _Rorr(t);
				}
			}
			else if (0<c)
			{
				t.Right = _Add(x,v, t.Right);
				if (_GetHeight(t.Right) - _GetHeight(t.Left) == 2)
				{
					if (0<_comparer.Compare(x , t.Right.Key))
						t = _Rol(t);
					else
						t = _Roll(t);
				}
			} else
				throw new InvalidOperationException("An item with the specified key already exists in the dictionary.");
			t.Height = Math.Max(_GetHeight(t.Left), _GetHeight(t.Right)) + 1;
			t.Value = v;
			return t;
		}

		_Node _Ror(_Node t)
		{
			if (null!=t.Left)
			{
				_Node u = t.Left;
				t.Left = u.Right;
				u.Right = t;
				t.Height = Math.Max(_GetHeight(t.Left), _GetHeight(t.Right)) + 1;
				u.Height = Math.Max(_GetHeight(u.Left), t.Height) + 1;
				return u;
			}
			return t;
		}
		_Node _Rol(_Node t)
		{
			if (null!=t.Right)
			{
				_Node u = t.Right;
				t.Right = u.Left;
				u.Left = t;
				t.Height = Math.Max(_GetHeight(t.Left), _GetHeight(t.Right)) + 1;
				u.Height = Math.Max(_GetHeight(t.Right), t.Height) + 1;
				return u;
			}
			return t;
		}

		_Node _Roll(_Node t)
		{
			t.Right = _Ror(t.Right);
			return _Rol(t);
		}

		_Node _Rorr(_Node t)
		{
			t.Left = _Rol(t.Left);
			return _Ror(t);
		}

		_Node _GetLeftMost(_Node t)
		{
			if (t == null)
				return null;
			else if (t.Left == null)
				return t;
			else
				return _GetLeftMost(t.Left);
		}

		_Node _Remove(TKey x, _Node t)
		{
			_Node temp;

			// Element not found
			if (t == null)
				return null;

			// Searching for element
			else if (0>_comparer.Compare(x ,t.Key))
				t.Left = _Remove(x, t.Left);
			else if (0<_comparer.Compare(x , t.Key))
				t.Right = _Remove(x, t.Right);

			// Element found
			// With 2 children
			else if (null!=t.Left && null!=t.Right)
			{
				temp = _GetLeftMost(t.Right);
				t.Key = temp.Key;
				t.Value = temp.Value;
				t.Right = _Remove(t.Key, t.Right);
			}
			// With one or zero child
			else
			{
				temp = t;
				if (t.Left == null)
					t = t.Right;
				else if (t.Right == null)
					t = t.Left;
			}
			if (t == null)
				return t;

			t.Height = Math.Max(_GetHeight(t.Left), _GetHeight(t.Right)) + 1;

			// If node is unbalanced
			// If left node is deleted, right case
			if (_GetHeight(t.Left) - _GetHeight(t.Right) == -2)
			{
				// right right case
				if (_GetHeight(t.Right.Right) - _GetHeight(t.Right.Left) == 1)
					return _Rol(t);
					// right left case
				else
					return _Roll(t);
			}
			// If right node is deleted, left case
			else if (_GetHeight(t.Right) - _GetHeight(t.Left) == 2)
			{
				// left left case
				if (_GetHeight(t.Left.Left) - _GetHeight(t.Left.Right) == 1)
				{
					return _Ror(t);
				}
				// left right case
				else
					return _Rorr(t);
			}
			return t;
		}
		bool _TryRemove(TKey x, _Node t, out _Node s)
		{
			s = null;
			_Node temp;
			var res = false;
			// Element not found
			if (null == t)
				return false;
			// Searching for element
			else if (0 > _comparer.Compare(x, t.Key))
			{
				res = _TryRemove(x, t.Left, out s);
				if (res)
					t.Left = s;
			}
			else if (0 < _comparer.Compare(x, t.Key))
			{
				res = _TryRemove(x, t.Right, out s);
				if (res)
					t.Right = s;
			}

			// Element found
			// With 2 children
			else if (null != t.Left && null != t.Right)
			{
				temp = _GetLeftMost(t.Right);
				t.Key = temp.Key;
				t.Value = temp.Value;
				res = _TryRemove(t.Key, t.Right, out s);
				if (res)
					t.Right = s;
			}
			// With one or zero child
			else
			{
				temp = t;
				if (t.Left == null)
					t = t.Right;
				else if (t.Right == null)
					t = t.Left;
				res = true;
			}
			if (t == null)
			{
				s = null;
				return res;
			}

			t.Height = Math.Max(_GetHeight(t.Left), _GetHeight(t.Right)) + 1;

			// If node is unbalanced
			// If left node is deleted, right case
			if (_GetHeight(t.Left) - _GetHeight(t.Right) == -2)
			{
				// right right case
				if (_GetHeight(t.Right.Right) - _GetHeight(t.Right.Left) == 1)
				{
					s = _Rol(t);
					return true;
				}
				// right left case
				else
				{
					s=_Roll(t);
					return true;
				}
			}
			// If right node is deleted, left case
			else if (_GetHeight(t.Right) - _GetHeight(t.Left) == 2)
			{
				// left left case
				if (_GetHeight(t.Left.Left) - _GetHeight(t.Left.Right) == 1)
				{
					s = _Ror(t);
					return true;
				}
				// left right case
				else
				{
					s = _Rorr(t);
					return true;
				}
			}
			s=t;
			return res;
		}

		int _GetHeight(_Node t)
		{
			return (t == null ? -1 : t.Height);
		}

		int _GetBalance(_Node t)
		{
			if (t == null)
				return 0;
			else
				return _GetHeight(t.Left) - _GetHeight(t.Right);
		}
		// TODO: Consider making a custom enumerator for this to kick perf up slightly
		IEnumerable<KeyValuePair<TKey,TValue>> _EnumNodes(_Node t)
		{
			if (null != t)
			{
				foreach (var item in _EnumNodes(t.Left))
					yield return item;
				yield return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
				foreach (var item in _EnumNodes(t.Right))
					yield return item;
			}
		}
		int _GetCount(_Node t)
		{
			if (null == t) return 0;
			var result = 1;
			result += _GetCount(t.Left);
			result += _GetCount(t.Right);
			return result;
		}
		public SortedAvlTreeDictionary(IComparer<TKey> comparer)
		{
			_comparer = comparer??Comparer<TKey>.Default;
			_root = null;
		}
		public SortedAvlTreeDictionary() : this(null)
		{

		}
		public int Count {
			get {
				return _GetCount(_root);
			}
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		public bool ContainsKey(TKey key)
		{
			var n = _Search(key, _root);
			return null != n;
		}
		public TValue this[TKey key] {
			get {
				TValue value;
				if (TryGetValue(key, out value))
					return value;
				throw new KeyNotFoundException();
			}
			set {
				var n = _Search(key, _root);
				if (null != n)
					n.Value = value;
				else
					Add(key, value);
			}
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
			var n = _Search(key, _root);
			if(null!=n)
			{
				value = n.Value;
				return true;
			}
			value = default(TValue);
			return false;
		}
		public bool Contains(KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			if (TryGetValue(item.Key, out value) && Equals(value, item.Value))
				return true;
			return false;
		}
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
			=> DictionaryUtility.CopyTo(this, array, index);
		public void Add(TKey key,TValue value)
		{
			_root = _Add(key, value, _root);
		}
		public void Add(KeyValuePair<TKey, TValue> item)
			=> Add(item.Key, item.Value);
		public bool Remove(TKey key)
		{
			_Node s;
			var res = _TryRemove(key, _root, out s);
			if(res)
			{
				_root = s;
				return true;
			}
			return false;
		}
		public bool Remove(KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			if(TryGetValue(item.Key,out value) && Equals(value,item.Value))
				return Remove(item.Key); // returns true
			return false;
		}
		public void Clear()
		{
			_root = null;
		}
		public ICollection<TKey> Keys
			=> DictionaryUtility.CreateKeys(this);
		public ICollection<TValue> Values
			=> DictionaryUtility.CreateValues(this);
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			=> _EnumNodes(_root).GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
