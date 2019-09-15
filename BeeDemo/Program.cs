using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
		const bool _testDictionary = true;
		const bool _testSortedDictionary = true;
		const bool _testSortedBTreeDictionary = true;
		const bool _testSortedBTreePlusDictionary = true;
		const bool _testSortedAvlTreeDictionary = true;
		const bool _testSortedSplayTreeDictionary = true;
		static void Main()
		{
			_TestPerf();

		}
		
		static void _TestPerf()
		{
			var d = new Dictionary<int, string>();
			var sd = new SortedDictionary<int, string>();
			var sbtd = new SortedBTreeDictionary<int, string>();
			var sbptd = new SortedBPlusTreeDictionary<int, string>();
			var satd = new SortedAvlTreeDictionary<int, string>();
			var sstd = new SortedSplayTreeDictionary<int, string>();
			Stopwatch s = new Stopwatch();
			var it = _iterationStep;
			while (it <= _maxIterations)
			{
				d.Clear();
				sd.Clear();
				sbtd.Clear();
				sbptd.Clear();
				satd.Clear();
				sstd.Clear();
				Console.WriteLine("*** Sequential Access - {0} items ***",it);
				Console.WriteLine();
				if(_testDictionary)
					_AddToTargetSeq(d, s, it);
				if (_testSortedDictionary)
					_AddToTargetSeq(sd, s, it);
				if(_testSortedBTreeDictionary)
					_AddToTargetSeq(sbtd, s, it);
				if(_testSortedBTreePlusDictionary)
					_AddToTargetSeq(sbptd, s, it);
				if(_testSortedAvlTreeDictionary)
					_AddToTargetSeq(satd, s, it);
				if(_testSortedSplayTreeDictionary)
					_AddToTargetSeq(sstd, s, it);
				Console.WriteLine();
				if(_testDictionary)
					_SearchTargetSeq(d, s, it);
				if (_testSortedDictionary)
					_SearchTargetSeq(sd, s, it);
				if(_testSortedBTreeDictionary)
					_SearchTargetSeq(sbtd, s, it);
				if(_testSortedBTreePlusDictionary)
					_SearchTargetSeq(sbptd, s, it);
				if(_testSortedAvlTreeDictionary)
					_SearchTargetSeq(satd, s, it);
				if(_testSortedSplayTreeDictionary)
					_SearchTargetSeq(sstd, s, it);
				Console.WriteLine();
				if(_testDictionary)
					_RemoveItemsTarget(d, s);
				if (_testSortedDictionary)
					_RemoveItemsTarget(sd, s);
				if(_testSortedBTreeDictionary)
					_RemoveItemsTarget(sbtd, s);
				if(_testSortedBTreePlusDictionary)
					_RemoveItemsTarget(sbptd, s);
				if(_testSortedAvlTreeDictionary)
					_RemoveItemsTarget(satd, s);
				if(_testSortedSplayTreeDictionary)
					_RemoveItemsTarget(sstd, s);
				Console.WriteLine();
				Console.WriteLine("*** Random Access - {0} items ***",it);
				Console.WriteLine();
				var rnd = _FillRandom(it, s);
				if(_testDictionary)
					_AddToTargetRnd(d, s, rnd);
				if (_testSortedDictionary)
					_AddToTargetRnd(sd, s, rnd);
				if(_testSortedBTreeDictionary)
					_AddToTargetRnd(sbtd, s, rnd);
				if(_testSortedBTreePlusDictionary)
					_AddToTargetRnd(sbptd, s, rnd);
				if(_testSortedAvlTreeDictionary)
					_AddToTargetRnd(satd, s, rnd);
				if(_testSortedSplayTreeDictionary)
					_AddToTargetRnd(sstd, s, rnd);
				Console.WriteLine();
				if(_testDictionary)
					_SearchTargetRnd(d, s, rnd);
				if (_testSortedDictionary)
					_SearchTargetRnd(sd, s, rnd);
				if(_testSortedBTreeDictionary)
					_SearchTargetRnd(sbtd, s, rnd);
				if(_testSortedBTreePlusDictionary)
					_SearchTargetRnd(sbptd, s, rnd);
				if(_testSortedAvlTreeDictionary)
					_SearchTargetRnd(satd, s, rnd);
				if (_testSortedSplayTreeDictionary)
					_SearchTargetRnd(sstd, s, rnd);
				Console.WriteLine();
				if (_testDictionary)
					_RemoveItemsTarget(d, s);
				if (_testSortedDictionary)
					_RemoveItemsTarget(sd, s);
				if (_testSortedBTreeDictionary)
					_RemoveItemsTarget(sbtd, s);
				if (_testSortedBTreePlusDictionary)
					_RemoveItemsTarget(sbptd, s);
				if (_testSortedAvlTreeDictionary)
					_RemoveItemsTarget(satd, s);
				if (_testSortedSplayTreeDictionary)
					_RemoveItemsTarget(sstd, s);
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
				Console.WriteLine(_GetName(d) + " searches: " + s.Elapsed.TotalMilliseconds + "ms");
			}
			catch(Exception ex)
			{
				Console.WriteLine(_GetName(d) + " threw: " +ex.GetType().Name+": "+ex.Message);
			}
		}

		private static void _AddToTargetSeq(IDictionary<int, string> d, Stopwatch s,int iterations)
		{
			try
			{
				s.Reset();
				for (int i = 0; i < iterations; ++i)
				{
					s.Start();
					d.Add(i, i.ToString());
					s.Stop();
				}
				Console.WriteLine(_GetName(d) + " adds: " + s.Elapsed.TotalMilliseconds + "ms");
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
				Console.WriteLine(_GetName(d) + " searches: " + s.Elapsed.TotalMilliseconds + "ms");
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
				Console.WriteLine(_GetName(d) + " adds: " + s.Elapsed.TotalMilliseconds + "ms");
			}
			catch (Exception ex)
			{
				Console.WriteLine(_GetName(d) + " threw: " + ex.GetType().Name + ": " + ex.Message);
			}
		}
	}
}
