using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bee;
namespace BeeDemo
{
	class Program
	{
		// options
		const bool _testRemoves = false;
		const int _maxIterations = 1000000;
		const int _iterationStep = 10;
		const bool _testBestCaseSeq = true;
		
		static void Main()
		{
			
			_TestPerf();

		}
		
		static void _TestPerf()
		{
			var dicts = new IDictionary<int, string>[] {
				new Dictionary<int, string>(),
				new SortedDictionary<int, string>(),
				new SortedBTreeDictionary<int, string>(),
				new SortedBPlusTreeDictionary<int, string>(),
				//new SortedAvlTreeDictionary<int, string>(),
				//new SortedSplayTreeDictionary<int, string>()
			};
			Stopwatch s = new Stopwatch();
			var it = _iterationStep;
			while (it <= _maxIterations)
			{
				for (var i = 0; i < dicts.Length; i++)
					dicts[i].Clear();
				Console.WriteLine("*** Sequential Access - {0} items ***",it);
				Console.WriteLine();
				for (var i = 0; i < dicts.Length; i++)
					_AddToTargetSeq(dicts[i], s, it);
				Console.WriteLine();
				for (var i = 0; i < dicts.Length; i++)
					_SearchTargetSeq(dicts[i], s, it);
				Console.WriteLine();
				for (var i = 0; i < dicts.Length; i++)
					_RemoveItemsTarget(dicts[i], s);
				Console.WriteLine();
				Console.WriteLine("*** Random Access - {0} items ***",it);
				Console.WriteLine();
				var rnd = _FillRandom(it, s);
				for (var i = 0; i < dicts.Length; i++)
					_AddToTargetRnd(dicts[i], s, rnd);
				Console.WriteLine();
				for (var i = 0; i < dicts.Length; i++)
					_SearchTargetRnd(dicts[i], s, rnd);
				Console.WriteLine();
				Console.WriteLine("*** Sequential Scan - {0} items ***", it);
				Console.WriteLine();
				for (var i = 0; i < dicts.Length; i++)
					_ScanTarget( dicts[i], s,it);
				Console.WriteLine();
				Console.WriteLine();
				it *= _iterationStep;
			}
		}

		private static void _RemoveItemsTarget(IDictionary<int, string> d, Stopwatch s)
		{
			if(!_testRemoves)
			{
				d.Clear();
				return;
			}
			try
			{
				s.Reset();
				while (0 != d.Count)
				{
					int first = -1;
					// grab the first element
					foreach (var item in d)
					{
						first = item.Key;
						break;
					}
					s.Start();
					d.Remove(first);
					s.Stop();
				}
				Console.WriteLine(_GetName(d) + " removes: " + s.Elapsed.TotalMilliseconds + "ms");
			}
			catch(Exception ex)
			{
				Console.WriteLine(_GetName(d) + " threw: " +ex.GetType().Name+": "+ex.Message);
			}
		}
		static int _GetHeight(IDictionary<int,string> d)
		{
			var t = d.GetType();
			try
			{
				//d.ContainsKey(d.Count / 2); // force a splay on the splay tree
				PropertyInfo pi = t.GetProperty("Height");
				if (null != pi)
					return (int)pi.GetValue(d);
			}
			catch { }
			return -1;
		}
		private static void _SearchTargetSeq(IDictionary<int, string> d, Stopwatch s, int iterations)
		{
			try
			{
				s.Reset();
				for (int i = 0; i < iterations; ++i)
				{
					string v;
					s.Start();
					d.TryGetValue(i, out v);
					s.Stop();
				}
				Console.Write(_GetName(d) + " searches: " + s.Elapsed.TotalMilliseconds + "ms");
				var h = _GetHeight(d);
				if (0 > h)
					Console.WriteLine();
				else
					Console.WriteLine(", Height = {0}", h);
			}
			catch(Exception ex)
			{
				Console.WriteLine(_GetName(d) + " threw: " +ex.GetType().Name+": "+ex.Message);
			}
		}
		private static void _ScanTarget(IDictionary<int, string> d, Stopwatch s, int iterations)
		{
			try
			{
				s.Reset();
				using (var e = d.GetEnumerator())
				{
					for (int i = 0; i < iterations; ++i)
					{
						s.Start();
						e.MoveNext();
						s.Stop();
					}
				}
				Console.Write(_GetName(d) + " scan: " + s.Elapsed.TotalMilliseconds + "ms");
				var h = _GetHeight(d);
				if (0 > h)
					Console.WriteLine();
				else
					Console.WriteLine(", Height = {0}", h);
			}
			catch (Exception ex)
			{
				Console.WriteLine(_GetName(d) + " threw: " + ex.GetType().Name + ": " + ex.Message);
			}
		}

		private static void _AddToTargetSeq(IDictionary<int, string> d, Stopwatch s,int iterations)
		{
			try
			{
				s.Reset();
				if (!_testBestCaseSeq || iterations < 3)
				{
					for (int i = 0; i < iterations; ++i)
					{
						s.Start();
						d.Add(i, i.ToString());
						s.Stop();
					}
				}
				else
				{

					var ic = iterations / 2;
					var jc = ic + 1;
					var added = true;
					while (added)
					{
						added = false;
						if (0 <= ic)
						{
							added = true;
							s.Start();
							d.Add(ic, ic.ToString());
							s.Stop();
							--ic;
						}
						if (jc < iterations)
						{
							added = true;
							s.Start();
							d.Add(jc, jc.ToString());
							s.Stop();
							++jc;
						}
					}
				}
				Console.Write(_GetName(d) + " adds: " + s.Elapsed.TotalMilliseconds + "ms");
				// for the splay tree
				d.ContainsKey(d.Count / 2);

				var h = _GetHeight(d);
				if (0 > h)
					Console.WriteLine();
				else
					Console.WriteLine(", Height = {0}", h);
			}
			catch (Exception ex)
			{
				Console.WriteLine(_GetName(d) + " threw: " + ex.GetType().Name + ": " + ex.Message);
			}
		}
		static string _GetName<TKey,TValue>(IDictionary<TKey,TValue> d)
		{
			var s = d.GetType().Name;
			var i = s.IndexOf('`');
			if (1 > i)
				return s;
			return s.Substring(0, i);
		}
		static int[] _FillRandom(int iterations,Stopwatch s)
		{
			var seen = new HashSet<int>(iterations);
			while(seen.Count< iterations)
			{
				var rnd = new Random(unchecked((int)s.ElapsedTicks)^seen.Count).Next();
				seen.Add(rnd);
			}
			
			return seen.ToArray();
		}
		
		private static void _SearchTargetRnd(IDictionary<int, string> d, Stopwatch s, int[] rnd)
		{
			try
			{
				s.Reset();
				for (var i = 0; i < rnd.Length; i++)
				{
					string v;
					var j = rnd[i];
					s.Start();
					d.TryGetValue(j, out v);
					s.Stop();
				}
				Console.Write(_GetName(d) + " searches: " + s.Elapsed.TotalMilliseconds + "ms");
				var h = _GetHeight(d);
				if (0 > h)
					Console.WriteLine();
				else
					Console.WriteLine(", Height = {0}", h);
			}
			catch (Exception ex)
			{
				Console.WriteLine(_GetName(d) + " threw: " + ex.GetType().Name + ": " + ex.Message);
			}
		}

		private static void _AddToTargetRnd(IDictionary<int, string> d, Stopwatch s, int[] rnd)
		{
			try
			{
				s.Reset();
				for (var i = 0; i < rnd.Length; i++)
				{
					var j = rnd[i];
					s.Start();
					d.Add(j, j.ToString());
					s.Stop();
				}
				Console.Write(_GetName(d) + " adds: " + s.Elapsed.TotalMilliseconds + "ms");
				var h = _GetHeight(d);
				if (0 > h)
					Console.WriteLine();
				else
					Console.WriteLine(", Height = {0}", h);
			}
			catch (Exception ex)
			{
				Console.WriteLine(_GetName(d) + " threw: " + ex.GetType().Name + ": " + ex.Message);
			}
		}
	}
}
