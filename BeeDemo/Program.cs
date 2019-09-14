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
		static void Main()
		{
			_TestPerf();

		}
		
		static void _TestPerf()
		{
			var d = new Dictionary<int, string>();
			var sd = new SortedDictionary<int, string>();
			var sbtd = new SortedBTreeDictionary<int, string>(5);
			var satd = new SortedAvlTreeDictionary<int, string>();
			var sstd = new SortedSplayTreeDictionary<int, string>();
			Stopwatch s = new Stopwatch();
			var it = _iterationStep;
			while (it <= _maxIterations)
			{
				Console.WriteLine("*** Sequential Access - {0} items ***",it);
				_AddToTargetSeq(d, s, it);
				_AddToTargetSeq(sd, s, it);
				_AddToTargetSeq(sbtd, s, it);
				_AddToTargetSeq(satd, s, it);
				//if (it <= 5000)
					_AddToTargetSeq(sstd, s, it);
				Console.WriteLine();
				_SearchTargetSeq(d, s, it);
				_SearchTargetSeq(sd, s, it);
				_SearchTargetSeq(sbtd, s, it);
				_SearchTargetSeq(satd, s, it);
				//if (it <= 5000)
					_SearchTargetSeq(sstd, s, it);
				Console.WriteLine();
				_RemoveItemsTarget(d, s);
				_RemoveItemsTarget(sd, s);
				_RemoveItemsTarget(sbtd, s);
				_RemoveItemsTarget(satd, s);
				//if (it <= 5000)
					_RemoveItemsTarget(sstd, s);
				Console.WriteLine();
				Console.WriteLine("*** Random Access - {0} items ***",it);
				var rnd = _FillRandom(it, s);
				_AddToTargetRnd(d, s, rnd);
				_AddToTargetRnd(sd, s, rnd);
				_AddToTargetRnd(sbtd, s, rnd);
				_AddToTargetRnd(satd, s, rnd);
				//if (rnd.Length <= 5000)
					_AddToTargetRnd(sstd, s, rnd);
				Console.WriteLine();
				_SearchTargetRnd(d, s, rnd);
				_SearchTargetRnd(sd, s, rnd);
				_SearchTargetRnd(sbtd, s, rnd);
				_SearchTargetRnd(satd, s, rnd);
				//if (rnd.Length <= 5000)
					_SearchTargetRnd(sstd, s, rnd);
				Console.WriteLine();
				_RemoveItemsTarget(d, s);
				_RemoveItemsTarget(sd, s);
				_RemoveItemsTarget(sbtd, s);
				_RemoveItemsTarget(satd, s);
				//if (rnd.Length <= 5000)
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
