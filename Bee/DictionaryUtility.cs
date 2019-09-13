using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee
{
	internal static class DictionaryUtility
	{
		public static void CopyTo<TKey,TValue>(ICollection<KeyValuePair<TKey,TValue>> source,KeyValuePair<TKey, TValue>[] array, int index)
		{
			var i = source.Count;
			if (i > array.Length)
				throw new ArgumentException("The array is not big enough to hold the dictionary entries.", nameof(array));
			if (0 > index || i > array.Length + index)
				throw new ArgumentOutOfRangeException(nameof(index));
			i = 0;
			foreach (var item in source)
			{
				array[i + index] = item;
				++i;
			}
		}
		public static ICollection<TKey> CreateKeys<TKey, TValue>(IDictionary<TKey, TValue> parent)
			=> new _KeysCollection<TKey, TValue>(parent);
		public static ICollection<TValue> CreateValues<TKey, TValue>(IDictionary<TKey, TValue> parent)
			=> new _ValuesCollection<TKey, TValue>(parent);
		#region _KeysCollection
		private sealed class _KeysCollection<TKey,TValue> : ICollection<TKey>
		{
			const string _readOnlyMsg = "The collection is read-only.";
			IDictionary<TKey, TValue> _outer;
			public _KeysCollection(IDictionary<TKey, TValue> outer)
			{
				_outer = outer;
			}
			public bool Contains(TKey key)
				=> _outer.ContainsKey(key);
			public bool IsReadOnly => true;
			public void Add(TKey key)
				=> throw new InvalidOperationException(_readOnlyMsg);
			public bool Remove(TKey key)
				=> throw new InvalidOperationException(_readOnlyMsg);
			public void Clear()
				=> throw new InvalidOperationException(_readOnlyMsg);
			public int Count => _outer.Count;
			public IEnumerator<TKey> GetEnumerator()
			{
				foreach (var item in _outer)
					yield return item.Key;
			}
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
				=> GetEnumerator();
			public void CopyTo(TKey[] array, int index)
			{
				var i = _outer.Count;
				if (i > array.Length)
					throw new ArgumentException("The array is not big enough to hold the dictionary keys.", nameof(array));
				if (0 > index || i > array.Length + index)
					throw new ArgumentOutOfRangeException(nameof(index));
				i = 0;
				foreach (var item in _outer)
				{
					array[i + index] = item.Key;
					++i;
				}
			}
		}
		#endregion

		#region _ValuesCollection
		private sealed class _ValuesCollection<TKey,TValue> : ICollection<TValue>
		{
			const string _readOnlyMsg = "The collection is read-only.";
			IDictionary<TKey, TValue> _outer;
			public _ValuesCollection(IDictionary<TKey, TValue> outer)
			{
				_outer = outer;
			}
			public bool Contains(TValue value)
			{
				foreach (var item in _outer)
					if (Equals(item.Value, value))
						return true;
				return false;
			}
			public bool IsReadOnly => true;
			public void Add(TValue key)
				=> throw new InvalidOperationException(_readOnlyMsg);
			public bool Remove(TValue key)
				=> throw new InvalidOperationException(_readOnlyMsg);
			public void Clear()
				=> throw new InvalidOperationException(_readOnlyMsg);
			public int Count => _outer.Count;
			public IEnumerator<TValue> GetEnumerator()
			{
				foreach (var item in _outer)
					yield return item.Value;
			}
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
				=> GetEnumerator();
			public void CopyTo(TValue[] array, int index)
			{
				var i = _outer.Count;
				if (i > array.Length)
					throw new ArgumentException("The array is not big enough to hold the dictionary values.", nameof(array));
				if (0 > index || i > array.Length + index)
					throw new ArgumentOutOfRangeException(nameof(index));
				i = 0;
				foreach (var item in _outer)
				{
					array[i + index] = item.Value;
					++i;
				}
			}
		}
		#endregion
	}
}
