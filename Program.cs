/*
 * Created by: Zilog8
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ScratchPad1
{
	class Euler
	{
		public static void run()
		{
			
		}
	}
	
	class Program
	{
		public static void Main(string[] args)
		{
			Euler282.run();
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);	
		}
	}
	
	class Euler282
	{
		public static void run()
		{
			int split = 1475789056;
			calcCache<point,zint> cc = new standardCalcCache<point,zint>(new ackermannCCOBJ());
			zint total = new zint(0);
			point[] pp = new point[]{new point(1,0), new point(2,2), new point(3,4)}; 
			foreach(point p in pp)
			{
				Console.Write(p + " = ");
				Console.WriteLine(cc.getVal(p));
			}
			for(int i=0; i<7; i++)
				total+=cc.getVal(new point(i,i));
			Console.WriteLine(total);
		}
		
		public struct point : IComparable<point>
		{
			public zint m;
			public zint n;
			
			public point (int m, int n)
			{
				this.m = new zint(m);
				this.n = new zint(n);
			}
			
			public point(zint m, zint n)
			{
				this.m = m;
				this.n = n;
			}
			
			public override bool Equals(object obj)
			{
				point o = (point)obj;
				return (this.m==o.m&&this.n==o.n);
			}
			
			public override int GetHashCode()
			{
				return m.GetHashCode()^n.GetHashCode();
			}
			
			public override string ToString()
			{
				return "(M,N): (" + m.ToString() + "," + n.ToString() + ")";
			}
			#region IComparable<Euler282.point> implementation
			public int CompareTo (point other)
			{
				int retVal = other.m.CompareTo(this.m);
				if(retVal==0)
					retVal = other.n.CompareTo(this.n);
				return retVal;
			}
			
			#endregion
		}
		
		public class ackermannCCOBJ : ccObj<point,zint>
		{
			public zint getVal(point x, calcCache<point,zint> c)
			{
				if(c.cacheSize()>200000000)
					c.partialTrim();
				if(x.m==0)
					return x.n+1;
				else if(x.n==0)
					return c.getVal(new point(x.m-1,new zint(1)));
				else
					return c.getVal(new point(x.m-1, c.getVal(new point(x.m,x.n-1))));
			}
		}
	
	}
	
	class Euler261
	{
		static int MaxM = 2000;
		
		public static void run()
		{
			//This is the stuff that has to be loaded from disk on startup for resume
			state now = lib.fileCache<state>("Euler261.state", state.initial);
			
			if(now.m==0)
			{
				now.m++;
				Console.WriteLine("Starting from scratch, m = 1");
				now.pivots = firstRun();
				lib.fileCacheSave<state>("Euler261.state", now);
				printPivotSum(now.pivots);
			}
			
			while(now.m<MaxM)
			{
				now.m++;
				Console.WriteLine("Starting m = " + now.m.ToString());
				subsequentRun(now);
				lib.fileCacheSave<state>("Euler261.state", now);
				printPivotSum(now.pivots);
			}
		}
		
		public static void subsequentRun(state now)
		{
			SqrSum rightSide = new SqrSum(now.m);
			SqrSum leftSide = new SqrSum(now.m+1);
			
			while(leftSide.currIndex<10000000000ul)
			{
				BigInt2 leftVal = leftSide.getNext();
				BigInt2 rightVal = rightSide.currVal;
				while(leftVal>rightVal)
					rightVal = rightSide.getNext();
				if(leftVal==rightVal && leftSide.currIndex<rightSide.currIndex-(ulong)(now.m-1))
					now.pivots.Add(leftSide.currIndex);
			}
		}
		// k^2 + (k+1)^2 + ... (k+m)^2 == (n+1)^2 + ... (n+m)^2
		public static HashSet<ulong> firstRun()
		{
			HashSet<ulong> pivots = new HashSet<ulong>();
			
			lib.SqrGen2 rightSide = new lib.SqrGen2(1);
			SqrSum leftSide = new SqrSum(2);
			while(leftSide.currIndex<10000000000ul)
			{
				BigInt2 leftVal = leftSide.getNext();
				BigInt2 rightVal = rightSide.currValue;
				while(leftVal>rightVal)
					rightVal = rightSide.getNext();
				if(leftVal==rightVal)
					pivots.Add(leftSide.currIndex);
			}
			
			return pivots;
		}
		
		public class SqrSum
		{
			BigInt2[] cache;
			int cacheIndex;
			
			lib.SqrGen2 genIndex;
			public BigInt2 currVal;
			
			public ulong currIndex
			{
				get{ return genIndex.currIndex;}
			}
			
			public SqrSum(int m)
			{
				cache = new BigInt2[m];
				cacheIndex = 0;
				genIndex = new lib.SqrGen2();
				currVal = new BigInt2();
				for(int i=0;i<m-1;i++)
					getNext();
			}
			
			public BigInt2 getNext()
			{
				currVal-=cache[cacheIndex];
				cache[cacheIndex] = genIndex.getNext();
				currVal+=cache[cacheIndex];
				cacheIndex++;
				cacheIndex %= cache.Length;
				return currVal;
			}
		}
		
		public static void printPivotSum(HashSet<ulong> pivots)
		{
			BigInt2 totalSum = new BigInt2(0);
			foreach(ulong l in pivots)
			{
				totalSum+=l;
			}
			Console.WriteLine("Total = " + totalSum.ToString());
		}
		
		public static void printPivots(HashSet<ulong> pivots)
		{
			BigInt2 totalSum = new BigInt2(0);
			foreach(ulong l in pivots)
			{
				Console.Write(l.ToString()+",");
				totalSum+=l;
			}
			Console.WriteLine();
			Console.WriteLine("Total = " + totalSum.ToString());
		}
						
		[Serializable]
		public class state
		{
			public int m;
			public HashSet<ulong> pivots;
			
			public static state initial()
			{
				state r = new state();
				r.m = 0;
				r.pivots = new HashSet<ulong>();
				return r;
			}
		}
	}
	
	class Euler261new
	{
		public static void run()
		{
			int chunkSize = 10;
			ulong maxIndex = 1000;
			
			Unit.all = new Unit[chunkSize];
			Unit.results = new HashSet<ulong>();
			Unit.offset = 0;
			
			Unit.all[0].FirstSet();
		}
		
		private struct Unit
		{
			public static ulong offset;
			public static Unit[] all;
			public static HashSet<ulong> results;
			
			lib.SqrGen2 lowEnd;
			lib.SqrGen2 highEnd;
			
			public BigInt2 currVal;
			
			public void FirstSet()
			{
				lowEnd = new ScratchPad1.lib.SqrGen2(0);
			}
			
			public void SetNext(int nextIndex)
			{
				
			}
		}
	}
	
	class Euler249
	{
		//strategy:
		//Think of it as a recursive knap-sack problem
		//maxPossible = sum of all primes 2..4999
		//for each prime P up to maxPossible:
		// getNumSolutions(P, 1 + math.min(index of P, index of 4999))   //getNumSolutions(spaceLeftInSack,index of cealing prime value)
		// this function calls itself somehow to get its answer:
			//	val += getNumSolutions(arg[0]-prime[arg[1]-1], arg[1]-1);
			//  val += getNumSolutions(arg[0], arg[1]-1);
		//Possible speedups: short-circuit if spaceLeftInSack>sum of all primes up to prime[celingindex-1]
		
		public static void run()
		{
			runProb();
		}
		
		public static void runProb()
		{
			int[] pA = lib.allPrimesBelow(5000);
			int[] pAsums = new int[pA.Length];
			int total = 0;
			for(int i=0; i<pA.Length; i++)
			{
				total+=pA[i];
				pAsums[i] = total;
			}
			
			recursiveKnapSack.primesArray = pA;
			//recursiveKnapSack.primesSet = new HashSet<int>(pA);
			recursiveKnapSack.primeSums = pAsums;
			
			//TODO: Implement a weakReference-based cache.
			//Idea: a dictionary of the strong, plus a costum hashmap (using the hash of the key) of 10-1000 weakReferenced dictionaries.
			var cc = new weakReferenceCache<tuple,limitedzint>(new recursiveKnapSack());
			
			int[] sacks = lib.allPrimesBelow(total+1);
			limitedzint result = new limitedzint(0,2);
			for(int i=0; i<sacks.Length;i++)
			{
				if((i<<25)==0)
				{
					Console.Clear();
					Console.WriteLine(i.ToString() + " / " + sacks.Length.ToString());
				}
				int sackSize = sacks[i];
				result.addTo(cc.getVal(new tuple(sackSize, 1 + Math.Min(i,pA.Length-1))));
			}
			Console.WriteLine(result);
		}
		
		public static void runTest()
		{
			int[] pA = lib.allPrimesBelow(5000);
			int[] pAsums = new int[pA.Length];
			int total = 0;
			for(int i=0; i<pA.Length; i++)
			{
				total+=pA[i];
				pAsums[i] = total;
			}
			
			recursiveKnapSack.primesArray = pA;
			//recursiveKnapSack.primesSet = new HashSet<int>(pA);
			recursiveKnapSack.primeSums = pAsums;
			
			var cc = new standardCalcCache<tuple,limitedzint>(new recursiveKnapSack());
			
			int[] sacks = lib.allPrimesBelow(total+1);
			limitedzint result = new limitedzint(0,2);
			
				result.addTo(cc.getVal(new tuple(21, 1 + Math.Min(18,pA.Length-1))));
				
			Console.WriteLine(result);
		}
		
		class recursiveKnapSack : ccObj<tuple, limitedzint>
		{
//			public static HashSet<int> primesSet;
			public static int[] primesArray;
			public static int[] primeSums;
			
			public limitedzint getVal(Euler249.tuple x, calcCache<Euler249.tuple, limitedzint> c)
			{
				limitedzint res = new limitedzint(0,2);
				
				int nextIndex = x.indexOfCeiling-1;	
				if(x.spaceLeft < primeSums[nextIndex] && nextIndex>0)
					res.addTo(c.getVal(new tuple(x.spaceLeft, nextIndex)));
				
				int calc = x.spaceLeft-primesArray[nextIndex];
				int nextPsum = int.MinValue;
				if(nextIndex>0)
					nextPsum = primeSums[nextIndex-1];
				if(calc==0 || calc == nextPsum)
					res.increment();
				else if(calc>0 && calc < nextPsum)
					res.addTo(c.getVal(new tuple(calc, nextIndex)));
				return res;
			}
		}
		
		private static void calcNumberOfSets()
		{
			Factorial.pfc = lib.primeFactorCountForAllNumsBelow(5000);
			
			Console.WriteLine(Euler093.nCk(9,5));
			Console.WriteLine(nCk(9,5));
			
			int[] pA = lib.allPrimesBelow(5000);
			zint b = new zint(0);
			for(int i=1; i<=pA.Length; i++)
				b+=nCk(pA.Length,i);
			Console.WriteLine(b);
		}
		
		private struct tuple : IComparable<tuple>
		{
			public int spaceLeft;
			public int indexOfCeiling;
			
			public tuple(int a, int b)
			{
				this.spaceLeft = a;
				this.indexOfCeiling = b;
			}
			
			public override bool Equals(object obj)
			{
				tuple o = (tuple)obj;
				return (this.spaceLeft==o.spaceLeft&&this.indexOfCeiling==o.indexOfCeiling);
			}
			
			public override int GetHashCode()
			{
				return (spaceLeft<<10)^indexOfCeiling;;
			}
			
			public int CompareTo(tuple other)
			{
				return other.indexOfCeiling-this.indexOfCeiling;
			}
		}
		
		public static zint nCk(int n, int k)
		{
			return (((new Factorial(n))/(new Factorial(k)))/(new Factorial(n-k))).getValue();
		}
		
		public class Factorial
		{
			public static int[][] pfc;
			int[] factorCount;
			
			private Factorial()
			{}
			
			public Factorial(int n)
			{
				if(n<2)
					factorCount = new int[]{};
				else
				{
					List<int> l = new List<int>();
					for(int i=2;i<=n; i++)
					{
						while(l.Count<pfc[i].Length)
							l.Add(0);
						for(int ii=2; ii<pfc[i].Length; ii++)
							l[ii]+=pfc[i][ii];
					}
					while(l[l.Count-1]==0)
						l.RemoveAt(l.Count-1);
					factorCount = l.ToArray();
				}
			}
			
			public static Factorial operator / (Factorial a, Factorial b)
			{
				Factorial retVal = new Factorial();
				List<int> l = new List<int>(a.factorCount.Length);
				l.Add(0); l.Add(0);
				for(int i=2; i<a.factorCount.Length;i++)
				{
					l.Add(a.factorCount[i]);
					if(i<b.factorCount.Length)
					{
						l[i]-=b.factorCount[i];
					}
				}
				while(l.Count>0 && l[l.Count-1]==0)
					l.RemoveAt(l.Count-1);
				retVal.factorCount = l.ToArray();
				return retVal;
			}
			
			public zint getValue()
			{
				zint b = new zint(1);
				for(int i=2; i<factorCount.Length; i++)
				{
					for(int x = factorCount[i];x>0;x--)
						b*=i;
				}
				return b;
			}
		}
	}	
	
	class Euler244
	{
		public static void run()
		{
			HashSet<int> olderGen = new HashSet<int>();
			List<state> prevGen = new List<state>(new state[]{state.start});
			
			long result = 0;
			while(result==0)
				result = calc(olderGen, ref prevGen);
			Console.WriteLine(result);
		}
				
		public static long calc(HashSet<int> olderGen, ref List<state> prevGen)
		{
			List<state> newGen = new List<state>();
			HashSet<int> newGenH = new HashSet<int>();
			long tot = 0;
			
			foreach(state s in prevGen)
			{
				if(s.canGoDown)
				{
					state sd = s.Down;
					int sdh = sd.GetHashCode();
					if(!olderGen.Contains(sdh))
					{
						if(sdh==23130)
							tot+=sd.checksum;
						else
						{
							newGen.Add(sd);
							newGenH.Add(sdh);
						}
					}
				}
				if(s.canGoUp)
				{
					state sd = s.Up;
					int sdh = sd.GetHashCode();
					if(!olderGen.Contains(sdh))
					{
						if(sdh==23130)
							tot+=sd.checksum;
						else
						{
							newGen.Add(sd);
							newGenH.Add(sdh);
						}
					}
				}
				if(s.canGoLeft)
				{
					state sd = s.Left;
					int sdh = sd.GetHashCode();
					if(!olderGen.Contains(sdh))
					{
						if(sdh==23130)
							tot+=sd.checksum;
						else
						{
							newGen.Add(sd);
							newGenH.Add(sdh);
						}
					}
				}
				if(s.canGoRight)
				{
					state sd = s.Right;
					int sdh = sd.GetHashCode();
					if(!olderGen.Contains(sdh))
					{
						if(sdh==23130)
							tot+=sd.checksum;
						else
						{
							newGen.Add(sd);
							newGenH.Add(sdh);
						}
					}
				}
			}
			olderGen.UnionWith(newGenH);
			prevGen = newGen;
			return tot;
		}
		
		public class state : IEquatable<state>
		{					//index = x + y*4
			BitArray board; //false if red, true if blue, empty is false
			int slider;  //coordinates of the empty slot, in x+y*4
			public int checksum;
			byte last;
			
			public static state start
			{
				get{
					state s = new state();
					s.checksum = 0;
					s.slider = 0;
					s.last = 255;
					s.board = new BitArray(16);
					for(int x = 2; x<4; x++)
						for(int y = 0; y<4; y++)
							s.board[x+4*y] = true;
					return s;
				}
			}
			
			public static state end
			{
				get{
					state s = new state();
					s.checksum = 0;
					s.slider = 0;
					s.last = 255;
					s.board = new BitArray(16);
					for(int x = 0; x<4; x++)
						for(int y = 0; y<4; y++)
							s.board[x+4*y] = (x%2==1)^(y%2==1);
					return s;
				}
			}
						
			private static int calcChecksum(byte move, int lastChecksum)
			{
				long l = Math.BigMul(lastChecksum, 243);
				l+=move;
				return (int)(l%100000007);
			}
			
			public bool canGoLeft
			{
				get{
					if(slider%4==3 || last == 82)
						return false;
					else
						return true;
				}
			}
			
			public state Left
			{
				get{					
					BitArray b = new BitArray(board);
					int slid = slider+1;
					b[slider] = b[slid];
					b[slid] = false;
					
					state mool = new state();
					mool.board = b;
					mool.slider = slid;
					mool.last = 76;
					mool.checksum = calcChecksum(76, checksum);
					
					return mool;
				}
			}
			
			public bool canGoRight
			{
				get{
					if(slider%4==0 || last == 76)
						return false;
					else
						return true;
				}
			}
			
			public state Right
			{
				get{					
					BitArray b = new BitArray(board);
					int slid = slider-1;
					b[slider] = b[slid];
					b[slid] = false;
					
					state mool = new state();
					mool.board = b;
					mool.slider = slid;
					mool.last = 82;
					mool.checksum = calcChecksum(82, checksum);
					
					return mool;
				}
			}
			
			public bool canGoUp
			{
				get{
					if(slider/4==3 || last == 68)
						return false;
					else
						return true;
				}
			}
			
			public state Up
			{
				get{					
					BitArray b = new BitArray(board);
					int slid = slider+4;
					b[slider] = b[slid];
					b[slid] = false;
					
					state mool = new state();
					mool.board = b;
					mool.slider = slid;
					mool.last = 85;
					mool.checksum = calcChecksum(85, checksum);
					
					return mool;
				}
			}
			
			public bool canGoDown
			{
				get{
					if(slider/4==0 || last == 85)
						return false;
					else
						return true;
				}
			}
			
			public state Down
			{
				get{					
					BitArray b = new BitArray(board);
					int slid = slider-4;
					b[slider] = b[slid];
					b[slid] = false;
					
					state mool = new state();
					mool.board = b;
					mool.slider = slid;
					mool.last = 68;
					mool.checksum = calcChecksum(68, checksum);
					
					return mool;
				}
			}
			
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				int y = slider/4;
				int x = slider-y*4;
				sb.AppendLine("Checksum: " + checksum.ToString() + "; Last: " + last.ToString()
				              + "; Slider: (" + x.ToString() + "," + y.ToString() + ") ; Hash: " + 
				              GetHashCode().ToString()); //Convert.ToString(GetHashCode(),2));
				for(int iy=0; iy<16; iy+=4)
				{
					for(int ix=0; ix<4; ix++)
					{
						int val = ix+iy;
						if(val==slider)
							sb.Append(" ");
						else if(board[val])
							sb.Append("B");
						else
							sb.Append("R");
					}
					sb.AppendLine();
				}
				return sb.ToString();
			}
			
			public override int GetHashCode()
			{
				int hash = slider;
				int i=16;
				while(i>0)
				{
					hash<<=1;
					if(board[--i])
						hash++;
				}
				return hash;
			}
			
			public static bool operator == (state a, state b)
			{
				if(a.slider!=b.slider)
					return false;
				int i = a.board.Length;
				while(i>0)
					if(a.board[--i]!=b.board[i])
						return false;
				return true;
			}
			
			public static bool operator != (state a, state b)
			{
				return !(a==b);
			}
			
			public override bool Equals(object obj)
			{
				return this==(state)obj;
			}
			
			public bool Equals(state other)
			{
				return this==other;
			}
		}
	}
	
	class Euler233
	{
		//Summary: Math.Sqrt(double) and isInteger(double) dont have enough precision for the higher N's
		// gotta do something else, me-thinks
		public static void run1()
		{
			Console.WriteLine(CalcfN(6, 90));
			Console.WriteLine(CalcfN(10000, 90));
		}
		
		public static void run()
		{
			long sum = 0;
			long maxN = 100000000000;
			long minN = 1;
			for(long x=maxN; x>=minN; x--)
			{
				if(CalcfN(x, 420)==420)
				{
					sum+=x;
				}
			}
			Console.WriteLine(sum);
		}
		
		private static int[] precomputedTable = new int[]{0,0,2,4,8};
		
		public static int CalcfN(double N, int max)
		{			
			int total = 4;        //Add in 4, for the (0,0), (0,N), (N,0), (N,N). 
			double center = N/2;
			double centerSqr = center*center;
			int isXint = 0;
			
			//this iterates through 1/8th of the circle, but it is enough to derive the whole circle, (using the precomputedTable).
			//the center positions:(center,center*(1-sqrt(2))), (center,center*(1+sqrt(2))),
			//(center*(1-sqrt(2)), center), (center*(1+sqrt(2)), center) do not need to be considered since for an integer (rational?) N,
			//they will never be integers.
			for(double x = .5; x< center; x+=.5)
			{
				double NX = N-x;
				int iX = isInteger(NX) + isXint;
				if(iX>0)
				{
					//derived from the circle formula: (x-n/2)^2 + (y+n/2)^2 = (n*sqrt(2)/2)^2
					double y = center - Math.Sqrt( NX*x + centerSqr);
					int iY =  isInteger(y) + isInteger(N-y);
					total+=precomputedTable[iX*iY];
					if(total>max)
						return -1;
				}
				
				isXint++;
				isXint%=2;
			}
			return total;
		}
		
		/// <summary>
		/// Returns 1 if its an integer value, 0 if not.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		private static int isInteger(double n)
		{
			double roundedN = Math.Round(n);
			if( Math.Abs(roundedN-n) < .000000000001d)
				return 1;
			else
				return 0;
		}
	}
	
	class Euler206
	{
		static string pattern = "1_2_3_4_5_6_7_8_9_0";
		public static void run()
		{
			long min = 1020304050607080900;
			long max = 1929394959697989990;
			
			int minInt = (int)(Math.Sqrt(((double)min)))-2;
			int maxInt = (int)(Math.Sqrt(((double)max)))+2;
			
			Console.WriteLine("MinInt = " + minInt.ToString());
			Console.WriteLine("MaxInt = " + maxInt.ToString());
			
			int currInt = 0;
			long result = 0;
			for(currInt = minInt; !isSpecial(result); currInt++)
				result = Math.BigMul(currInt, currInt);
			currInt--;
			Console.WriteLine("Integer = " + currInt.ToString() + " Sqr = " + result.ToString());
		}
		
		public static bool isSpecial(long a)
		{
			string s = a.ToString();
			for(int i=0; i<s.Length; i+=2)
			{
				if(s[i]!=pattern[i])
					return false;
			}
			return true;
		}
	}
	
	class Euler205
	{
		public static void run2()
		{
			Random r  = new Random();
			Roller Colin = new Roller(r,6);
			Roller Pete = new Roller(r,4);
			long ratio =0;
			long wins = 0;
			long total = 0;
			for(int i=0; true; i++)
			{
				if(Pete.getNext(9)>Colin.getNext(6))
				{
					wins++;
				}
				total++;
				if(i>100000)
				{
					i=0;
					long nratio = (wins*100000000)/(total);
					if(ratio!=nratio)
					{
						Console.WriteLine(nratio);
						ratio=nratio;
					}
				}
			}			
		}
		
		public class Roller
		{
			Random r;
			int max;
			
			public Roller(Random r, int max)
			{
				this.r = r;
				this.max = max+1;
			}
			
			public int getNext()
			{
				return r.Next(1,max);
			}
			
			public int getNext(int numRolls)
			{
				int total = 0;
				for(int i=numRolls; i>0; i--)
					total+=r.Next(1,max);
				return total;
			}
		}
		
		public static void run()
		{
			List<int> colin = new List<int>();
			recurser(colin, 6,6,0);
			Console.WriteLine("Calc'd Colin");
			List<int> pete = new List<int>();
			recurser(pete, 4,9,0);
			Console.WriteLine("Prog'd Pete");
			
			long peteWin = 0;
			long total = 0;
			long finish = ((long)pete.Count)*((long)colin.Count);
			foreach(int c in colin)
			{
				foreach(int p in pete)
				{
					total++;
					if(p>c)
						peteWin++;
				}
				//Console.WriteLine(finish-total);
			}
			Console.WriteLine(peteWin.ToString() + " / " + total.ToString());
			Console.WriteLine(((double)peteWin)/((double)total));
		}
		
		public static void recurser(List<int> list, int dice, int level, int currSum)
		{
			if(level==0)
				list.Add(currSum);
			else
			{
				for(int i=1; i<=dice; i++)
				{
					recurser(list, dice, level-1,currSum+i);
				}
			}
		}
	}
	
	class Euler204
	{
		public static void run()
		{
			int finalMax = 1000000000;
			
			//Console.WriteLine(pr(finalMax/10,5));
			Console.WriteLine(pr(finalMax,100));
		}
		
		public static int pr(int max, int n)
		{
			BitArray isNotPrime = new BitArray(max+1);
			BitArray isHammond = new BitArray(max+1,true); isHammond[0] = false;
			
			for(int curr=2; curr<=n; curr++)
			{
				if(!isNotPrime[curr]) //FALSE is a prime, thus we negate
				{
					for( int i = curr*2; i<=max; i+=curr)
					{
						isNotPrime[i] = true; //this aint a prime, so TRUE it.
					}
				}
			}
			
			for(int curr=n+1; curr<=max; curr++)
			{
				if(!isNotPrime[curr]) //FALSE is a prime, thus we negate
				{
					isHammond[curr] = false;
					for( int i = curr*2; i<=max; i+=curr)
					{
						isNotPrime[i] = true; //this aint a prime, so TRUE it.
						isHammond[i] = false;
					}
				}
			}
			
			int total = 0;
			foreach(bool b in isHammond)
				if(b)
					total++;
			return total;
		}
	}
	
	class Euler203
	{
		
		public class primesquare : lib.ngonalIntObject<long>
		{
			lib.InfiniteArrayOfPrimes2 iap;
			
			public primesquare(lib.InfiniteArrayOfPrimes2 iap)
			{
				this.iap = iap;
			}
			
			public long gen(int x)
			{
				int prime = iap[x-1];
				return Math.BigMul(prime,prime);
			}
		}
		
		public static void run()
		{
			long[][] pascalTriangle = getPT(51);
			HashSet<long> cache = new HashSet<long>();
			foreach(long[] level in pascalTriangle)
			{
				foreach(long along in level)
				{
					cache.Add(along);
				}
			}
			
			zint total = new zint(0);
			long rfrf = 126410606437752; rfrf/=2; rfrf = (long) (1.1*Math.Sqrt(rfrf));
			var primesqrGen = new lib.nGonalInt<long>(new primesquare(new lib.InfiniteArrayOfPrimes2((int)rfrf)));
			
			foreach(long along in cache)
			{
				if(isSquareFree(along, primesqrGen))
					total.addTo(along);
			}
			Console.WriteLine(total);
		}
		
		private static bool isSquareFree(long x, lib.nGonalInt<long> primesqrGen)
		{
			long sqr = 1;
			for(int i=1;sqr<x;i++)
			{
				sqr = primesqrGen.getAtI(i);
				if(x%sqr==0)
					return false;
			}
			return true;
		}
		
		public static long[][] getPT(long x)
		{
			long[][] retVal = new long[x+1][];
			retVal[0] = new long[]{};
			for(long i=1; i<=x; i++)
			{
				long[] rr = new long[i];
				rr[rr.Length-1] = 1; rr[0] = 1;
				long[] prev = retVal[i-1];
				for(long g = 1; g<rr.Length-1; g++)
				{
					rr[g] = prev[g-1] + prev[g];
				}
				retVal[i] = rr;
			}
			return retVal;
		}
	}
		
	class Euler187
	{
		public static void run()
		{
			Console.WriteLine(primeFactors(30));
			Console.WriteLine(primeFactors(100000000));
		}
		
		
		public static long primeFactors(int x)
		{
			long count = 0;
			Dictionary<int, int[]> all = new Dictionary<int, int[]>();
			BitArray buff = lib.isPrimeForAllBelow(x);
			
			for(int curr=2; curr<buff.Length; curr++)
			{
				if(buff[curr]) //if curr is a prime
				{
					for( int i = curr*2; i<x; i+=curr)
					{
						if(!all.ContainsKey(i))
						{
							int val = i/curr;
							if(buff[val])		//and the second value is a prime
								all[i] = new int[]{curr, val};							
						}
						else
						{
							if(all[i]!=null && all[i][1]!=curr)
								all[i] = null;
						}						
					}
				}
				else //curr isn't a prime
				{
					if(all.ContainsKey(curr))
					{
						if(all[curr]!= null)
							count++;
						all.Remove(curr);
					}
				}
			}
			
			return count;
		}		
		
		
	}
	
	class Euler179
	{
		public static void run()
		{
			int max = 10000000;
			//int max = 15;
			
			List<int>[] pants = new List<int>[max+1];
			for(int i=2; i<pants.Length; i++)
				pants[i] = new List<int>();
			
			long total = 0;
			
			for(int i=2; i<pants.Length-1; i++)
			{
				for(int curr = i*2; curr<pants.Length; curr+=i)
				{
					pants[curr].Add(i);
				}
				if(pants[i].Count==pants[i+1].Count)
				{
					total++;
					//Console.WriteLine(i.ToString());
				}
				pants[i] = null;
			}
			Console.WriteLine(total);
		}
	}
	
	class Euler172
	{
		static int MaxCount = 3;
		static int Levels = 18;
		
		public static void run()
		{
			byte[] allZero = new byte[10];
			
			calcCache<state, long> cc = new standardCalcCache<state, long>(new recurser());			
			long total = 9* cc.getVal(new state(allZero,Levels-1, 0));
			
			Console.WriteLine(total);
		}
		
		public class state : IComparable<state>
		{
			public byte[] count;
			public int level;
			int HashCode;
			
			public state(byte[] prevCount, int level, int incrementIndex)
			{
				count = new byte[prevCount.Length];
				Array.Copy(prevCount, count, prevCount.Length);
				count[incrementIndex]++;
				Array.Sort(count);
				this.level = level;
				
				#region Generate HashCode
				HashCode = level*100000000;
				for(int i=0; i<count.Length; i++)
					HashCode+= ((int)count[i])*((int)Math.Pow(10,i));
				#endregion
			}
					
			public override int GetHashCode()
			{
				return HashCode;				
			}
			
			public override bool Equals(object obj)
			{
				state other = (state)obj;
				if(level!=other.level)
					return false;
				for(int i=0;i<count.Length;i++)
				{
					if(count[i]!=other.count[i])
						return false;
				}
				return true;
			}
			
			public int CompareTo(state other)
			{
				return 0;
			}
		}
		
		public class recurser : ccObj<state, long>
		{			
			public long getVal(state x, calcCache<state, long> c)
			{
				if(x.level==0) return 1;
				else
				{
					long retVal = 0;
					for(int i=0; i<x.count.Length; i++)
					{
						if(x.count[i]<MaxCount)
						{
							retVal+= c.getVal(new state(x.count,x.level-1, i));
						}
					}
					return retVal;
				}
			}
		}
	}
	
	class Euler164
	{
		static int Levels = 20;
		
		public static void run()
		{
			byte[] allZero = new byte[2];
			
			calcCache<state, long> cc = new standardCalcCache<state, long>(new recurser());
			long total = 0;
			for(byte i=1;i<10;i++)
				total += cc.getVal(new state(allZero,Levels-1, i));
			
			Console.WriteLine(total);
		}
		
		public class state : IComparable<state>
		{
			public byte[] prev2;
			public int level;
			int HashCode;
			
			public state(byte[] oldPrev2, int level, byte newDigit)
			{
				prev2 = new byte[2];
				prev2[0] = oldPrev2[1];
				prev2[1] = newDigit;
				this.level = level;
				
				#region Generate HashCode
				HashCode = level*1000000 + prev2[1]*1000 + prev2[0];
				#endregion
			}
			
			public override int GetHashCode()
			{
				return HashCode;				
			}
			
			public override bool Equals(object obj)
			{
				state other = (state)obj;
				if(level!=other.level)
					return false;
				for(int i=0;i<prev2.Length;i++)
				{
					if(prev2[i]!=other.prev2[i])
						return false;
				}
				return true;
			}
			
			public int CompareTo(state other)
			{
				return 0;
			}
		}
		
		public class recurser : ccObj<state, long>
		{			
			public long getVal(state x, calcCache<state, long> c)
			{
				if(x.level==0) return 1;
				else
				{
					long retVal = 0;
					
					int max = 9 - (x.prev2[0] + x.prev2[1]);
					while(max>=0)
					{
						retVal+=c.getVal(new state(x.prev2,x.level-1, (byte)(max--)));
					}
					return retVal;
				}
			}
		}
	}
	
	class Euler148
	{
		public static void run()
		{
			int[][] pascalTriangle = getPT(51);
		}
		
		public static int[][] getPT(int x)
		{
			int[][] retVal = new int[x+1][];
			retVal[0] = new int[]{};
			for(int i=1; i<=x; i++)
			{
				int[] rr = new int[i];
				rr[rr.Length-1] = 1; rr[0] = 1;
				int[] prev = retVal[i-1];
				for(int g = 1; g<rr.Length-1; g++)
				{
					rr[g] = prev[g-1] + prev[g];
				}
				retVal[i] = rr;
			}
			return retVal;
		}
	}
	
	class Euler145
	{
		public static void run()
		{
			//int max = 1000;
			int max = 1000000000;
			int count =0;
			for(int i = 1; i<max; i++)
			{
				if(isReversable(i))
					Console.WriteLine(i.ToString() + ":" + (++count).ToString());
			}
			Console.WriteLine(count);
		}
		
		public static bool isReversable(int a)
		{
			string sA = a.ToString();
			if(sA.EndsWith("0"))
				return false;
			char[] cA = sA.ToCharArray();
			Array.Reverse(cA);
			string nSA = new string(cA);
			int b = int.Parse(nSA);
			return !hasEven(a+b);
		}
		
		public static char[] evens = new char[]{'0','2','4','6','8'};
		
		public static bool hasEven(int a)
		{
			string s = a.ToString();
			foreach(char c in evens)
				for(int i=0; i<s.Length; i++)
					if(s[i]==c)
						return true;
			return false;
		}
	}
	
	class Euler132
	{
		public static void run()
		{
			lib.InfiniteArrayOfPrimes2 iap = new ScratchPad1.lib.InfiniteArrayOfPrimes2(10000000);
			HashSet<int> primeFactors = new HashSet<int>();
			for(int i=0; primeFactors.Count<40; i++)
			{
				int prime = iap[i];
				if(isFactor(prime))
				{
					primeFactors.Add(prime);
					Console.WriteLine(primeFactors.Count.ToString() + ": " + prime.ToString());
				}
			}
			printSum(primeFactors);
		}
		
		//strategy:
		//think of it as a case of long division (but adding 1's instead of 0's when you need more digits)
		//for each prime:
		//divide until you get a repeat in remainder or remainder == 0
		//if remainder repeats, discard prime and move onto the next one
		//if remainder == 0, add prime to the prime-factors list
		//MEMO: e.g. 111 isnt divisible by 11, so look into how to account for that
		//about above, its because 111 == R(3), and 10^9 % 3 != 0
		private static bool isFactor(int prime)
		{
			int n = 1;
			int rem = 1;
			HashSet<int> remainders = new HashSet<int>();
			while(rem!=0)
			{
				if(remainders.Contains(rem))
					return false;
				remainders.Add(rem);
				while(rem<prime)
				{
					rem*=10;
					rem++;
					n++;
				}
				rem %= prime;
			}
			
			return 1000000000%n==0;
		}
		
		public static void printSum(HashSet<int> primeFactors)
		{
			long totalSum = 0;
			foreach(int i in primeFactors)
			{
				totalSum+=i;
			}
			Console.WriteLine("Total = " + totalSum.ToString());
		}
	}
	
	class Euler125
	{
		public static void run()
		{
			int max = 100000000; //1000;
			
			SumSqrer ss = new SumSqrer(max);
			int limit = (int)(Math.Sqrt(max/2))+1;
			//List<int> palindromes = new List<int>();
			HashSet<int> palindromes = new HashSet<int>();
			while(ss.curr<=limit)
			{
				foreach(int p in ss.iterate())
				{
					if(!palindromes.Contains(p))
						palindromes.Add(p);
				}
			}
			
			long total = 0;
			foreach(int p in palindromes)
			{
				total+=p;
			}
			//Console.WriteLine(palindromes.Count);
			Console.WriteLine(total);
		}
		
		public class SumSqrer
		{
			public List<int> sumsqrs;
			public int curr;
			int max;
			
			public SumSqrer(int max)
			{
				sumsqrs = new List<int>();
				curr = 0;
				this.max = max;
			}
			
			public List<int> iterate()
			{
				curr++;
				int currsqr = curr*curr;
				List<int> newsmsq = new List<int>(sumsqrs.Count+1);
				List<int> pals = new List<int>();
				foreach(int f in sumsqrs)
				{
					int val = f + currsqr;
					if(val<max)
					{
						newsmsq.Add(val);
						if(lib.isPalindrome(val.ToString()))
							pals.Add(val);
					}
				}				
				newsmsq.Add(currsqr);
				sumsqrs = newsmsq;
				return pals;
			}
		}
	}
	
	class Euler124
	{
		public static void run()
		{
			SortedList<long, int> sl = new SortedList<long, int>();
			int[][] bb = lib.primeFactorsForAllNumsBelow(100001);
			for(int i = 2; i<bb.Length; i++)
			{
				long rad = 1;
				foreach(int b in bb[i])
					rad*=b;
				rad*= 1000000;
				rad+=i;
				sl.Add(rad,i);
			}
			
				int ii = 9998;
				long key = sl.Keys[ii];
				int val = sl[key];
				Console.WriteLine("At k = " + (ii+2).ToString() + ": " + val.ToString() + " gives sort string: " + key.ToString());
		}
	}
	
	class Euler123
	{
		public static lib.InfiniteArrayOfPrimes2 iap;
		
		public static long[] multi(long[] a, long[] b)
		{
			long[] result = new long[a.Length+b.Length-1];
			for(int i=0; i<result.Length; i++)
				result[i] = 0;
			for(int ia=0; ia<a.Length;ia++)
			{
				for(int ib=0; ib<b.Length;ib++)
				{
					int location = ia+ib;
					long val = a[ia]*b[ib];
					result[location]+=val;
				}
			}
			return result;
		}
				
		public class matrix
		{
			long[] instance;
			public long[] current;
			
			public matrix(long[] inp)
			{
				instance = inp;
				current = inp;
			}
			
			public long[] getNext()
			{
				current = multi(instance,current);
				current = new long[]{current[0],current[1]};
				return current;
			}
		}
		
		public static void run()
		{
			long max = 10000000000;
			iap = new lib.InfiniteArrayOfPrimes2(10*(int)Math.Sqrt(max));
			
			//index == power of x;  ie, x^2 + 2x - 1  == {-1,2,1}
			matrix a = new matrix(new long[]{-1,1});
			matrix b = new matrix(new long[]{1,1});
			
			int count = 2;
			long val = calc(a.getNext(),b.getNext(),iap[count]);
			while(val<=max)
			{
				count++;
				int prime = iap[count-1]; //iap starts indexing from 0, therefore have to adjust.
				val = calc(a.getNext(),b.getNext(),prime);
				long pp = Math.BigMul(prime, prime);
				while(val>=pp)
					val-=pp;
				while(val<0)
					val+=pp;				
			}
			Console.WriteLine(count);
		}
		
		public static long calc(long[] a, long[] b, long prime)
		{
			return (a[1]+b[1])*prime + a[0]+b[0];
		}
	}
	
	class Euler120
	{
		public static void run()
		{
			int[] rems = new int[1001];
			zint[] pows= new zint[1002];
			
			for(int i=2; i<1002; i++)
			{
				pows[i] = new zint(i);
			}
			
			int n = 1;
			while(true)
			{
				n++;
				pows[2].multWith(2);
				pows[3].multWith(3);
				for(int i=3;i<=1000;i++)
				{
					zint ta = pows[i-1];
					zint tb = pows[i+1];
					tb.multWith(i+1);
					zint temp = ta.duplicate();
					temp.addTo(tb);
					int r = temp.remainder(i*i);
					rems[i] = Math.Max(r,rems[i]);
				}
				zint Sum = new zint(0);
				foreach(int i in rems)
					Sum.addTo(i);
				Console.WriteLine("current:" + Sum.ToString());
			}
		}
		
		private static int calcR(int a)
		{
			zint ta = new zint(a-1);
					ta.power(1000);
			zint tb = new zint(a+1);
					tb.power(1000);
			int maxR = 0;
			int asqr = a*a;
			for(int n=1001; n<=000;n++)
			{
				ta.multWith(a-1);
				tb.multWith(a+1);
				
				zint temp = ta.duplicate();
				temp.addTo(tb);
				
				int m = temp.remainder(asqr);
				maxR = Math.Max(maxR,m);
			}
			return maxR;
		}
	}
	
	class Euler119
	{
		public static void run()
		{
			List<zint> pizza = new List<zint>();
			for(int i =0;i<10; i++)
				pizza.Add(new zint(i));
			
			List<zint> res = new List<zint>();
			while(res.Count<30)
				iter(pizza,res);
			
			zint tres = null;
			while(100>pizza.Count)
			{
				iter(pizza,res);
				if(tres!=res[29])
				{
					tres = res[29];
					Console.WriteLine("Possible: " + tres.ToString());
				}
			}
			Console.WriteLine("Final: " + tres.ToString());
		}
		
		public static void iter(List<zint> pizza, List<zint> results)
		{
			bool mustSort = false;
			
			//Add another item to the list (equal to 1, since the mult is yet to come)
			pizza.Add(new zint(1));
			
			//Mult all the items in the list by their index, and check digitsum
			for(int i = pizza.Count-1; i>1; i--)
			{
				zint tL = pizza[i]*i;
				pizza[i] = tL;
				if(lib.DigitSum(tL.ToString())==i)
				{
					results.Add(tL);
					mustSort = true;
					//Console.WriteLine(results.Count.ToString() + " " + tL.ToString());
				}
			}
			
			//Do I have to sort? if so do so .
			if(mustSort)
				results.Sort();
		}
	}
	
	class Euler118
	{
		public static void run()
		{
			HashSet<int> rprimes = lib.fileCache<HashSet<int>>("Euler118.cache", HashSetAllRelevantPrimes);
			HashSet<int>[] pants = lib.fileCache<HashSet<int>,HashSet<int>[]>("Euler118.cache2", notHave,rprimes);
			pants[0] = rprimes;
			
			recursed r = new recursed(pants);
			calcCache<state, long> cc = new standardCalcCache<state, long>(r);
			r.addCC(cc);
			
			long lo = cc.getVal(new state(new char[]{}, 0)); //'4','5','6','7','8','9'
			Console.WriteLine(lo);
		}
				
		public struct state : IComparable<state>
		{
			public char[] x;
			public int last;
			
			public state(char[] x, int last)
			{
				this.x = x;
				this.last = last;
			}
			
			public int CompareTo(state other)
			{
				return 0;
			}
		}
		
		public class recursed : ccObj<state, long>
		{
			HashSet<int>[] primes;
			calcCache<state, long> cc;
			
			public recursed(HashSet<int>[] primes)
			{
				this.primes = primes;
			}
			
			public void addCC(calcCache<state, long> cc)
			{
				this.cc = cc;
			}
			
			public long getVal(state sx)
			{
				char[] x = sx.x;
				int last = sx.last;
				
				long retVal = 0;
				HashSet<int> finalSet = new HashSet<int>(primes[0]);
				
				foreach(char ch in x)
				{
					int index = ((int)ch)-48;
					finalSet.IntersectWith(primes[index]);
				}
				
				foreach(int bb in finalSet)
				{
					if(bb>last)
					{
						char[] bbc = lib.ArrayUnion<char>(x,bb.ToString().ToCharArray());
						Array.Sort(bbc);
						retVal+=cc.getVal(new state(bbc, bb));
					}
				}
				return retVal;
			}
			
			public long getVal(state sx, calcCache<state, long> c)
			{
				if(sx.x.Length==9)
					return 1;
				
//				string file = "Euler118.cache3." + new string(sx.x) + "." + sx.last.ToString();
//				return lib.fileCache<state,long>(file,getVal,sx);
				return getVal(sx);
			}
		}
		
		public static HashSet<int> HashSetAllRelevantPrimes()
		{
			int x  = 987654322;
			HashSet<int> pants = new HashSet<int>();
			lib.largeBitArray buff = new lib.largeBitArray(x);
			for(int curr=2; curr<x; curr++)
			{
				if(!buff[curr]) //FALSE is a prime, thus we negate
				{
					string s = curr.ToString();
					if(!s.Contains("0") && lib.doesntRepeatChars(s))
						pants.Add(curr);
					for( int i = curr*2; i<x; i+=curr)
						buff[i] = true; //this aint a prime, so TRUE it.
				}
			}
			return pants;
		}
		
		public static HashSet<int>[] notHave(HashSet<int> rp)
		{
			var pants = new HashSet<int>[10];
			for(int i=1; i<10; i++)
				pants[i] = new HashSet<int>();
			
			string[] istring = new string[]{"","1","2","3","4","5","6","7","8","9"};
			foreach(int bb in rp)
			{
				string sb = bb.ToString();
				for(int i=1; i<10; i++)
					if(!sb.Contains(istring[i]))
						pants[i].Add(bb);
			}
			return pants;
		}
	}
	
	class Euler117
	{
		public static void run()
		{
			int num = 50;
			calcCache<int,zint> cc = new standardCalcCache<int, zint>(new Euler116.recursed(new int[]{1,2,3,4}));
			Console.WriteLine(cc.getVal(num));
		}
	}
	
	class Euler116
	{
		public static void run()
		{
			int num = 50;
			zint tot = new zint(0);
			int[] arr = new int[]{1,1};
			for(int i=2;i<5;i++)
			{
				arr[1] = i;
				calcCache<int,zint> cc = new standardCalcCache<int, zint>(new recursed(arr));
				zint retVal = cc.getVal(num);
				retVal.substract(new zint(1));
				tot.addTo(retVal);
			}
			Console.WriteLine(tot);
		}
		
		public class recursed : ccObj<int, zint>
		{
			int[] options;
			
			public recursed(int[] options)
			{
				this.options=options;
			}
			
			public zint getVal(int remain, calcCache<int, zint> c)
			{
				if(remain==0)
					return new zint(1);
				else
				{
					zint z = new zint(0);
					foreach(int i in options)
					{
						if(remain>=i)
							z.addTo(c.getVal(remain-i));
					}
					return z;
				}
			}			
		}		
	}
	
	class Euler115
	{
		public static void run()
		{
			int min = 50;
			calcCache<int,zint> cc = new standardCalcCache<int, zint>(new recursed(min));
			zint minZ = new zint(1000001);
			int size = min;
			for(zint tot = cc.getVal(++size);
			    minZ>tot;
			    tot = cc.getVal(++size))
				;
			Console.WriteLine(size);
		}
		
		public class recursed : ccObj<int, zint>
		{
			int min;
			
			public recursed(int min)
			{
				this.min=min;
			}
			
			public zint getVal(int remain, calcCache<int, zint> c)
			{
				if(remain==0)
					return new zint(1);
				else
				{
					zint z = new zint(0);
					if(remain<0) //can't be a red
					{
						remain = -remain;
						z.addTo(c.getVal(remain-1)); //adding a black, so no need to make negative
					}
					else
					{
						z.addTo(c.getVal(remain-1)); //adding a black, so no need to make negative
						for(int i=min; i<=remain; i++)
							z.addTo(c.getVal(-(remain-i))); //make it negative to indicate you can't add another red right away.
					}
					return z;
				}
			}			
		}		
	}
	
	class Euler114
	{
		public static void run()
		{
			int min = 3;
			calcCache<int,zint> cc = new standardCalcCache<int, zint>(new Euler115.recursed(min));
			zint tot = cc.getVal(50);
			Console.WriteLine(tot);
		}
	}
	
	class Euler113
	{
		static calcCache<tuple, BigInt> nbc;
		public static void run()
		{
//			long result = 0;
//			for(int i=1; i<1000000; i++)
//				if( Euler112.isntBouncy(i))
//					result +=1;
//			Console.WriteLine(result);
			run1();
		}
		public static void run1()
		{
			nbc = new standardCalcCache<Euler113.tuple, BigInt>(new nonBouncyCounter());
			
			int length = 100;
			BigInt result = new BigInt(0);
			for(int currlength = 2; currlength<=length; currlength++)
				for(int i=1; i<10; i++)
				{
					result += nbc.getVal(new tuple(currlength-1,i, 0));	
				}
			result+=9; //adjust to include the first 9 natural numbers. Could fix the code to include them, but not worth the trouble.
			Console.WriteLine(result);
			Console.WriteLine(result.components());
		}
		
		public class nonBouncyCounter : ccObj<tuple, BigInt>
		{			
			public BigInt getVal(Euler113.tuple x, calcCache<Euler113.tuple, BigInt> c)
			{
				BigInt res = new BigInt(0);
				if(x.digitsLeft==1)
				{
					if(x.delta==0)
						res+=10;
					else if(x.delta==1)
					{
						res+=10-x.lastDigit;
					}
					else
					{
						res+=x.lastDigit+1;
					}
				}
				else
				{
					if(x.delta==0)
					{
						res+= c.getVal(new tuple(x.digitsLeft-1, x.lastDigit, 0));
						for(int i=0; i<x.lastDigit; i++)
							res+=c.getVal(new tuple(x.digitsLeft-1, i, -1));
						for(int i=x.lastDigit+1; i<10; i++)
							res+=c.getVal(new tuple(x.digitsLeft-1, i, 1));							
					}
					else if(x.delta==1)
					{
						for(int i=x.lastDigit; i<10; i++)
							res+=c.getVal(new tuple(x.digitsLeft-1, i, 1));	
					}
					else
					{
						for(int i=0; i<=x.lastDigit; i++)
							res+=c.getVal(new tuple(x.digitsLeft-1, i, -1));
					}
				}
				return res;
			}
		}
		
		public struct tuple : IComparable<tuple>
		{
			public int digitsLeft;
			public int lastDigit;
			public int delta;
			
			public tuple(int digitsLeft, int lastDigit, int delta)
			{
				this.digitsLeft = digitsLeft;
				this.lastDigit = lastDigit;
				this.delta = delta;
			}
			
			public override string ToString()
			{
				return "(digitsLeft,lastDigit,delta): (" + digitsLeft + "," + lastDigit + "," + delta + ")";
			}
			
			public override bool Equals(object obj)
			{
				tuple oth = (tuple)obj;
				return ((digitsLeft==oth.digitsLeft) && (lastDigit==oth.lastDigit) && (delta==oth.delta));
			}
			
			public override int GetHashCode()
			{
				return delta*10000 + lastDigit*1000 + digitsLeft;
			}
			
			public int CompareTo(tuple other)
			{
				return 0;
			}
		}
	}
	
	class Euler112
	{
		public static void run()
		{
			int percentage = 99;
			int rate = 100/(100-percentage);
			
			int totalCount = 1;
			int ratedCount = rate;
			while(totalCount!=ratedCount)
			{
				if(isntBouncy(++totalCount))
					ratedCount+=rate;
			}
			Console.WriteLine(totalCount);
		}
		
		public static bool isntBouncy(int i)
		{
			char[] s = i.ToString().ToCharArray();
			return (isDecreasing(s) || isIncreasing(s));
		}
		
		public static bool isDecreasing(char[] s)
		{
			for(int i=1; i<s.Length; i++)
				if(s[i-1]<s[i])
					return false;
			return true;
		}
				
		public static bool isIncreasing(char[] s)
		{
			for(int i=1; i<s.Length; i++)
				if(s[i-1]>s[i])
					return false;
			return true;
		}
	}
	
	class Euler110c
	{
		public static void run()
		{
			//Asumming: x<=y
			//Then:
			// n < x <=2*n			therefore, for posible X's to be greater than 1000, n>1000
			// 2*n <= y
			
			int[] p = lib.PrimeFactors(1260,true);
			foreach(int d in p)
				Console.WriteLine(d);
			
//			decimal minn = d;
			int minn = 4088000;
//			int maxn = 1 + int.MaxValue/2;
			while(true)
			{
//				for(int i=0; i<1000; i++)
//				{
//					if(result(--maxn)>4000000)
//						Console.WriteLine("Possible Answer found at " + maxn.ToString());
//				}
//				Console.WriteLine("Max At: " + maxn.ToString());
				for(int i=0; i<1000; i++)
				{
					if(result(++minn)>4000000)
					{
						Console.WriteLine("Answer Found at " + minn.ToString());
						return;
					}
				}
				Console.WriteLine("Min At: " + minn.ToString());
			}
		}
//		
//		public static int result(zint n)
//		{
//			int total = 0;
//			zint n2 = n.duplicate();
//			n2 = n2.multWith(2);
//			
//			zint xn = n.duplicate();
//			xn.power(2);
//			for(zint x = n.duplicate(); 0>=x.CompareTo(n2);)
//			{
//				x.addTo(1);
////				int remainder;
////				Math.DivRem(n*x,x-n,out remainder);
////				if(remainder==0)
////					total++;
//				xn.addTo(n);
//				if(xn.remainder(
//				if(xn%(x-n)==0)
//					total++;
//				
//			}
//			return total;
//		}
		
		public static int result(int n)
		{
			int total = 0;
			int n2 = 2*n;
			
			long xn = Math.BigMul(n,n);
			for(int x = n+1; x<=(n2); x++)
			{
//				int remainder;
//				Math.DivRem(n*x,x-n,out remainder);
//				if(remainder==0)
//					total++;
				
				xn+=n;
				if(xn%(x-n)==0)
					total++;
				
			}
			return total;
		}
	}
	
	class Euler110b
	{
		public static void run()
		{
			//Asumming: x<=y
			//Then:
			// n < x <=2*n			therefore, for posible X's to be greater than 1000, n>1000
			// 2*n <= y
			
			int res = 0;
			int n=4000000;
			while(res<4000000)
			{
				res = result(++n);
			}
			Console.WriteLine(n);
		}
		
		public static int result(int n)
		{
			int total = 0;
			int n2 = 2*n;
			
			long xn = Math.BigMul(n,n);
			for(int x = n+1; x<=(n2); x++)
			{
//				int remainder;
//				Math.DivRem(n*x,x-n,out remainder);
//				if(remainder==0)
//					total++;
				
				xn+=n;
				if(xn%(x-n)==0)
					total++;
				
			}
			return total;
		}
	}
	
	class Euler110
	{
		public static void largeBAtest()
		{
			int length = 200000;
			BitArray ba = new BitArray(length);
			lib.largeBitArray lba = new lib.largeBitArray(length);
			Random r = new Random();
			for(int i=0; i<length;i++)
			{
				bool b = 128>r.Next(0,256);
				ba[i] = b;
				lba[i] = b;
			}
			for(int i=0; i<length;i++)
			{
				if(ba[i]!=lba[i])
					Console.WriteLine("Error");
			}
		}
		
		public static void run()
		{
			// (1/x) + (1/y) = (1/n) = (x+y)/xy
			// nx + ny = xy
			//(n^2 + i*n) = xy-ny = y(x-n) = i*y
			// (n^2)/i  = y-n
			//Asumming: x<=y
			// n < x <=2*n		2*n <= y
			// let x = i+n; therefore  0<i<=n
			//
			
			long min =    1332500000; //1072500000;
			while(true)
			{
				Console.WriteLine("Up to: " + min.ToString());
				long max = min+2500000;
				factors[] fA = primeFactorsForAllBetween(min,max);
				long curr = min;
				foreach(factors f in fA)
				{
					factors f2 = doubledCount(f);
					int num = 1 + recursed(f2,1, curr,0);
					if(num>4000000)
					{
						Console.WriteLine("Number: " + curr.ToString());
						return;
					}
					curr++;
				}
				min = max;
			}
		}		
		
		public static factors doubledCount(factors f)
		{
			int[] cs = new int[f.counts.Length];
			for(int i=0; i<cs.Length;i++)
			{
				cs[i] = 2*f.counts[i];
			}
			return new factors(f.facts, cs);
		}
		
		public class factors
		{
			public int[] facts;
			public int[] counts;
			
			public factors(int[] f, int[] c)
			{
				facts = f;
				counts = c;
			}
			
			public void Add(int factor, int count)
			{
				facts = addTo(facts, factor);
				counts = addTo(counts, count);
			}
			
			private static int[] addTo(int[] a, int b)
			{
				int[] newA = new int[a.Length+1];
				Array.Copy(a,newA, a.Length);
				newA[a.Length] = b;
				return newA;
			}
			
			public factors minusOne(long i)
			{
				int[] newC = (int[])counts.Clone();
				newC[i]--;
				return new factors(facts,newC);
			}
			
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Factors: ");
				foreach(int i in facts)
					sb.Append(i.ToString() + ",");
				sb.Remove(sb.Length-1,1);
				sb.Append(" Frequencies: ");
				foreach(int i in counts)
					sb.Append(i.ToString() + ",");
				sb.Remove(sb.Length-1,1);
				return sb.ToString();
			}
		}
		
		public static int count(long num, long factor)
		{
			int count = 0;
			while(num%factor==0)
			{
				num/=factor;
				count++;
			}
			return count;
		}
		
		public static factors[] primeFactorsForAllBetween(long min, long max)
		{
			factors[] all = new factors[(max+1)-min];
			//BitArray buff = new BitArray(max+1);
			lib.largeBitArray buff = new lib.largeBitArray(max+1);

			for(int curr=2; curr<buff.Length; curr++)
			{
				if(!buff[curr]) //FALSE is a prime, thus we negate
				{
					for( long i = curr; i<=max; i+=curr)
					{
						buff[i] = true; //this aint a prime, so TRUE it.
						if(i>=min)
						{
							factors f = all[i-min];
							if(f==null)
							{
								all[i-min] = new factors(new int[]{curr}, new int[]{count(i,curr)});
							}
							else
							{
								f.Add(curr, count(i,curr));
							}	
						}
					}
				}
			}
			
			return all;
		}		
		
		public static int recursed(factors f, long multi, long n, long curr)
		{
			int retval = 0;
			for(long i=curr; i<f.facts.Length; i++)
			{
				if(f.counts[i]>0)
				{
					long c = multi*((long)f.facts[i]);
					if(c==n)
						retval++;
					else if(c<n)
					{
						factors newF = f.minusOne(i);
						retval += 1 + recursed(newF,c,n,i);
					}
				}
			}
			return retval;
		}
	}
	
	class Euler108
	{
		public static void run()
		{
			//Asumming: x<=y
			//Then:
			// n < x <=2*n			therefore, for posible X's to be greater than 1000, n>1000
			// 2*n <= y
			
			int res = 0;
			int n=156001;
			while(res<1000)
			{
				res = result(++n);
			}
			Console.WriteLine(n);
		}
		
		public static int result(int n)
		{
			int total = 0;
			int n2 = 2*n;
			
			long xn = Math.BigMul(n,n);
			for(int x = n+1; x<=(n2); x++)
			{
//				int remainder;
//				Math.DivRem(n*x,x-n,out remainder);
//				if(remainder==0)
//					total++;
				
				xn+=n;
				if(xn%(x-n)==0)
					total++;
				
			}
			return total;
		}
	}
	
	class Euler107
	{
		static edge[] allEdges;
		static custumCache cc;
		
		private static void testload()
		{
			allEdges = new edge[]{
				new edge(0,1,16),
				new edge(0,2,12),
				new edge(0,3,21),
				new edge(1,3,17),
				new edge(1,4,20),
				new edge(2,3,28),
				new edge(2,5,31),
				new edge(3,4,18),
				new edge(3,5,19),
				new edge(3,6,23),
				new edge(4,6,11),
				new edge(5,6,27)				
			};
		}
		
		private static void fixEdges()
		{
			foreach(edge ed in allEdges)
			{
				if(ed.vertex1>ed.vertex2)
				{
					int temp = ed.vertex2;
					ed.vertex2 = ed.vertex1;
					ed.vertex1 = temp;
				}
			}
		}
		
		private static void load()
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\network.txt");
			
			List<edge> temp2 = new List<Euler107.edge>();
			
			for(int v1 = 0; v1<lines.Length; v1++)
			{
				int[] line = Array.ConvertAll<string,int>(lines[v1].Split(new char[]{','}),hyperparse);
				for(int v2 = 0; v2<line.Length; v2++)
				{
					int weight = line[v2];
					if(weight!=-1 && v1<v2)
						temp2.Add(new edge(v1,v2,weight));
				}
			}
			
			allEdges = temp2.ToArray();
		}
		
		private static int hyperparse(string s)
		{
			if(s[0]=='-')
				return -1;
			else
				return int.Parse(s);
		}
		
		public static void run()
		{
			//testload();
			load();
			fixEdges();
			cc = new custumCache(new connectedSearch());
			int[] network = new int[allEdges.Length];
			for(int i=0; i<network.Length; i++)
				network[i]=i;
			
			int maxWeight = sumWeights(network);
			int minWeight = minWeightRecursur(network,0);
			Console.WriteLine(maxWeight-minWeight);
		}
		
		private class state : IComparable<state>
		{
			public HashSet<int> network;
			public int vertex1;
			public int vertex2;
			public int vertHash;
			
			public state(int[] network, int vertex1, int vertex2)
			{
				this.network = new HashSet<int>(network);
				if(vertex1>vertex2)
				{
					int temp = vertex2;
					vertex2 = vertex1;
					vertex1 = temp;
				}
				this.vertex2 = vertex2;
				this.vertex1 = vertex1;
				this.vertHash = vertex1*100 + vertex2;
			}
			
			public state(HashSet<int> network, int vertex1, int vertex2)
			{
				this.network = network;
				if(vertex1>vertex2)
				{
					int temp = vertex2;
					vertex2 = vertex1;
					vertex1 = temp;
				}
				this.vertex2 = vertex2;
				this.vertex1 = vertex1;
				vertHash = vertex1*100 + vertex2;
			}
			
			public override int GetHashCode()
			{
				int retVal = vertHash;
				foreach(int i in network)
					retVal^=i;
				return retVal;
			}
			
			public override bool Equals(object obj)
			{
				state other = (state)obj;
				return (vertHash==other.vertHash && network.Count==other.network.Count && network.SetEquals(other.network));
			}
			
			public int CompareTo(Euler107.state other)
			{
				return network.Count.CompareTo(other.network.Count);
			}
		}
		
		private class connectedSearch : ccObj<state, bool>
		{
			public bool getVal(state x, calcCache<state, bool> c)
			{
				HashSet<int> network = x.network;
				foreach(int edID in network)
				{
					edge ed = allEdges[edID];
					if(ed.vertex1==x.vertex1)
					{
						if(ed.vertex2==x.vertex2)
							return true;
						else
						{
							if(c.getVal(new state(newNet(network,edID),ed.vertex2, x.vertex2)))
								return true;
						}
					}
					else if(ed.vertex2==x.vertex1 && c.getVal(new state(newNet(network,edID),ed.vertex1, x.vertex2)))
					{
						return true;
					}
				}
				return false;
			}
		}
		
		private static HashSet<int> newNet(HashSet<int> oldNet, int RemoveID)
		{
			HashSet<int> newNet = new HashSet<int>(oldNet);
			newNet.Remove(RemoveID);
			return newNet;
		}
		
		private static int[] newNet(int[] oldNet, int RemoveAt)
		{
			int[] newNet = new int[oldNet.Length-1];
			Array.Copy(oldNet, newNet,RemoveAt);
			Array.ConstrainedCopy(oldNet,RemoveAt+1,newNet,RemoveAt, newNet.Length-RemoveAt);
			return newNet;
		}
		
		private static int minWeightRecursur(int[] network, int lastRemoved)
		{
			int minWeight = sumWeights(network);
			for(int i=lastRemoved; i<network.Length; i++)
			{
				int[] newNetwork = newNet(network, i);
				edge removedEdge = allEdges[network[i]];
				if(cc.getVal(new state(newNetwork, removedEdge.vertex1,removedEdge.vertex2)))
				{
					minWeight = Math.Min(minWeight, minWeightRecursur(newNetwork,i));
				}
			}
			return minWeight;
		}
		
		private static int sumWeights(int[] network)
		{
			int tot = 0;
			foreach(int ed in network)
			{
				tot += allEdges[ed].weight;
			}
			return tot;
		}
		
		private class edge
		{
			public int vertex1;
			public int vertex2;
			public int weight;
			
			public edge (int vertex1, int vertex2, int weight)
			{
				this.vertex1 = vertex1;
				this.vertex2 = vertex2;
				this.weight = weight;
			}
		}
				
		private class custumCache : calcCache<state, bool>
		{
			private Dictionary<int,List<state>> cache;
			private HashSet<state> impossibles;
			private ccObj<state,bool> cco;
			
			public custumCache(ccObj<state,bool> cco)
			{
				cache = new Dictionary<int,List<state>>();
				impossibles = new HashSet<Euler107.state>();
				this.cco = cco;
			}
					
			public bool getVal(state x)
			{
				if(impossibles.Contains(x))
					return false;
				
				bool retVal;
				int hash = x.vertHash;
				List<state> tList;
				
				if(!cache.TryGetValue(hash,out tList))
				{
					tList = new List<Euler107.state>();
					cache.Add(hash,tList);
				}
				
				foreach(state st in tList)
				{
					if(x.network.IsSupersetOf(st.network))
						return true;						
				}
				
				retVal = cco.getVal(x,this);
				
				if(retVal)
				{
					for(int i=0; i<tList.Count; i++)
					{
						if(x.network.IsSubsetOf(tList[i].network))
							tList.RemoveAt(i--);
					}
					tList.Add(x);
				}
				else
				{
					if(impossibles.Count>600000)
						impossibles.Clear();
					else
						impossibles.Add(x);
				}
				
				if(tList.Count>511) //=512 ;
				{
					tList.Sort();
					tList.RemoveRange(260,252); //range(x, 512-x)
				}
				
				return retVal;
			}
			
			public void partialTrim()
			{
				cache.Clear();
			}
			
			public void clearCache()
			{
				cache.Clear();
			}
			
			public int cacheSize()
			{
				return cache.Count;
			}
		}
	
	}
	
	class Euler104
	{
		public static void run()
		{
			zint prev = new zint(1);
			long currIndex = 2;
			zint curr = new zint(1);
			while(!test(curr))
			{
				prev.addTo(curr);
				zint newCurr = prev;
				prev = curr;
				curr = newCurr;
				currIndex++;
				//Console.WriteLine(currIndex.ToString() + ": " + curr.ToString());
			}
			Console.WriteLine("Answer: " + currIndex.ToString() ); //+ ": " + curr.ToString());
		}
		
		public static bool test(zint z)
		{			
			string sZ = z.ToString();
			if(sZ.Length>8 && test2(sZ.Substring(0,9)) && test2(sZ.Substring(sZ.Length-9,9)))
			{
				return true;
			}
			else
				return false;
		}
		
		public static bool test2(string z)
		{
			if(!z.Contains("0") && lib.doesntRepeatChars(z))
				return true;
			else
				return false;
		}
	}	
	
	class Euler102
	{
		public static void run()
		{
			int count =0;
			Point[][] p = load();
			foreach(Point[] t in p)
			{
				if(ContainsOrigin(t))
					Console.WriteLine((++count).ToString());
			}
			Console.WriteLine("Total: " + count.ToString());
		}
		
		public static Point[][] load()
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\triangles.txt");
			return Array.ConvertAll<string, Point[]>(lines,hyperparse);
		}
		private static Point[] hyperparse(string s)
		{
			string[] sts = s.Split(new char[]{','});
			int[] ints = Array.ConvertAll<string, int>(sts,int.Parse);
			int i=0;
			return new Point[]{
				new Point(ints[i++], ints[i++]),
				new Point(ints[i++], ints[i++]),
				new Point(ints[i++], ints[i++])				
			};
		}
		
		public static bool ContainsOrigin(Point[] t)
		{
			double antiAngleA = Angle.Fix(Math.Atan2(-t[0].Y,-t[0].X));
			double antiAngleB = Angle.Fix(Math.Atan2(-t[1].Y,-t[1].X));
			double angleC = Angle.Fix(Math.Atan2(t[2].Y,t[2].X));
			
			double limit1 = Math.Max(antiAngleA,antiAngleB);
			double limit2 = Math.Min(antiAngleA,antiAngleB);
			
			double straightDiff = limit1 - limit2;
			double aroundDiff = limit2 + ((2*Math.PI)-limit1);
			
			if(straightDiff<aroundDiff)		
				return ((angleC<=limit1) && (angleC>=limit2));
			else
				return ((angleC>=limit1) || (angleC<=limit2));
		}
		
		public class Angle
		{
			public double val;
			public Angle(double d)
			{
				val = Fix(d);
			}
			public Angle(int y, int x)
			{
				val = Fix(Math.Atan2(y,x));
			}
			public static double Fix(double d)
			{
				while(d<0)
					d+=(2*Math.PI);
				while(d>2*Math.PI)
					d-=(2*Math.PI);
				return d;
			}
		}
	}
	
	class Euler100
	{
//		x   x-1    1
//		- * ---  = -
//		y   y-1    2
//		
//		2x*(x-1) = y*(y-1)
		
		public static void run()
		{
			long min = 1000000000000;
//			long min = 20;
			long y = min+1; //1064424281028
			long x = ((long)(((double)y)/(Math.Sqrt(2)))); //752661627175
			decimal xside = 2*x;
			
			xside*= x-1;
			decimal yside = y;
			yside*= y-1;			
			
			while(xside!=yside)
			{
				if(xside>yside)
				{
					yside+=y*2;
					y++;
				}
				else
				{
					xside+=x*4;
					x++;
				}
			}
			Console.WriteLine(x);
		}
	}
	
	class Euler099
	{		
		public static void run()
		{
			SortedList<double, Euler063.duple> final = new SortedList<double,Euler063.duple>();
			Euler063.duple[] start = load();
			Euler063.duple[] post = weedOut(start); //check for absolute advantages
			double minExp = (double)getMinExp(post);	//get minimum exponent
			foreach(Euler063.duple dd in post)
			{
				Console.Write(".");
				double rat = ((double)(dd.b))/minExp;
				double estimate = Math.Pow(((double)(dd.a)),rat); //don't need the value, if the exponents equal. if x>y, then x^2 > y^2
				final.Add(estimate, dd);
				Console.WriteLine(final[final.Keys[final.Count-1]]);
			}
			int count = final.Count;
			Console.WriteLine("Final List:");
			for(int i=0; i<10; i++)
				Console.WriteLine(final[final.Keys[count-(1+i)]]);
		}
		
		private static void PrintAll(Euler063.duple[] moo)
		{
			for(int i=0; i<moo.Length;i++)
			{
				Console.WriteLine(moo[i]);
			}
		}
		
		private static Euler063.duple[] weedOut(Euler063.duple[] pre)
		{
			List<Euler063.duple> inter = new List<ScratchPad1.Euler063.duple>();
			foreach(Euler063.duple dd in pre)
			{
				MaybeAdd(dd,inter,pre);
			}
			return inter.ToArray();
		}
		
		private static void MaybeAdd(Euler063.duple dd, List<Euler063.duple> inter, Euler063.duple[] pre)
		{
			foreach(Euler063.duple tD in pre)
				if(tD.a>dd.a && tD.b>dd.b)
					return;
			inter.Add(dd);
		}
		
		private static int getMinExp(Euler063.duple[] data)
		{
			int min = int.MaxValue;
			foreach(Euler063.duple dd in data)
			{
				min = Math.Min(dd.b,min);
			}
			return min;
		}
		
		public static Euler063.duple[] load()
		{
			List<Euler063.duple> tot= new List<ScratchPad1.Euler063.duple>();
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\base_exp.txt");
			foreach(string line in lines)
			{
				string[] pair = line.Split(new char[]{','});
				tot.Add(new Euler063.duple(int.Parse(pair[0]),int.Parse(pair[1])));
			}
			return tot.ToArray();
		}
	}
	
	class Euler097
	{
		public static void run()
		{
			limitedzint zTot = new limitedzint(2,3);
			zTot.power(7830457);
			zTot.multWith(28433);
			zTot.addTo(new limitedzint(1,3));
			Console.WriteLine(zTot);
		}
	}
	
	class Euler096
	{
		public static void run()
		{
			List<string> puzzles = load();
			
			zint Sum = new zint(0);
			foreach(string puzz in puzzles)
			{
				Sum.addTo(getVal(solve(puzz)));
			}
			Console.WriteLine(Sum);
		}
		
		public static List<string> load()
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\sudoku.txt");
			List<string> puzzles = new List<string>();
			for(int i=0;i<lines.Length;)
			{
				i++;
				StringBuilder sb = new StringBuilder();
				for(int c=9;c>0;c--)
					sb.Append(lines[i++]);
				
				puzzles.Add(sb.ToString());
			}
			return puzzles;
		}
		
		public static int getVal(string solved)
		{
			return int.Parse(solved.Substring(0,3));
		}
		
		public static string solve(string puzz)
		{
			sudokuBoard sb = new sudokuBoard(puzz);
			while(!sb.Solve());
			string res = sb.ToString();
			if(res.Contains("0"))
				Console.WriteLine("Error");
			return res;
		}
		
		
            //General Sudoku Philosophy:
            //There are two models for solving cell values:
            //1) There are no other values that this cell can have (based on eliminiating values assigned to other cells in its column/row/subsquare
            //2) There are no other cells in this column/row/subsquare that can have this specific value
            //
            //This Implementation:
            //The first model is worked at through keeping track of the possible values for each cell.
            //Each time a cell is assigned a value, all cells in the same row, column, or subsquare have that value's possibility set to false.
            //If a cell is left with only one possibility set to true, then the corresponding value is assigned to it.
            //All this is done when cell values are loaded into the puzzle.
            //
            //If the first model hasnt solved the puzzle, the The second model is worked at.
            //The puzzle is traversed and a queue of unsolved cells is generated.
            //The second model is a loop, in which for each cell we check if one of its possible values is unique among its column, row, or subsquare.
            //If it is, then the cell is assigned that value, and model 1 solving occurs for the affected cells (very efficient).
            //This loop is repeated until the queue length reaches zero, or a whole run through the queue is done without solving a cell.
            //These correspond to solving the puzzle, or the puzzle being unsolvable.
            //(the reason for continual looping is that a cell that was not solvable by model 2 may become so once a later cell in the queue is solved.)
            //
            //Evolutionary Theory:
            //It may be possible that this could be done more efficiently by working the two models concurrently.
            //In my opinion, by keeping track of the number of cells that could have each value for each row, column, and subsquare.
            //ie, Column[i].Possibles[j] == 4; in Column i, the number of cells that could possibly have value j is 4
            //when a value of j is given to a cell in that column, then Column[i].Possibles[j] = 0;
            //if the value j is removed as a possibility for a cell in that column, then Column[i].Possibles[j]--;
            //if the Column[i].Possibles[j] reaches 1; then look through the cells in that column till you find the one
            //that has j as a possibility, and set its value to j.
            //
            //Revolutionary Theory:
            //Could it be possible that by having a list of unassigned values for each column, row, and subsquare
            //a puzzle could be solved by checking the overlapping values for each specific cell?
            //Something like this:
            //the board is represented by an int[,] of values (with 0 being the "unknown value" value)
            //Columns[], with a Column object for each column in the board
            //struct Column
            //{
            //    int[] of values that have not been assigned to any cells in the column
            //    OR a bitvector[1<<i] saying if value i has been assigned to any cell in the column
            //}
            //Do the same as above for the rows and subsquares
            //now, to calculate values by checking the overlap in unassigned values of Column, Row, and Subsquare for each unknown cell.
            //if for any cell, there is only one value in common between Column, Row and Subsquare, then that is the cell's value.

            public class sudokuBoard
            {
                private sudokuCell[,] board;
                public int Side; //The length of one side of the board. For the typical 9x9 sudoku puzzle, this value is 9.
                private int sqrootSide; //The square root of the length of a side of the board. For the typical 9x9 sudoku puzzle, this value is 3.
                public int StartVector; //Starting value for BitVector32. All possibilities for values > Side must be set to false

                /// <summary>
                /// Makes a blank sudoku board.
                /// </summary>
                /// <param name="Side">The length of a side of the desired sudoku board.</param>
                public sudokuBoard(int Side)
                {
                    this.Side = Side;

                    initBoard();

                    for (int i = 0; i < Side * Side; i++)
                    {
                        int ii = i / Side;
                        board[i - (ii * Side), ii] = new sudokuCell(false, this);
                    }

                    FillLinks();
                }

                public sudokuBoard(string puzzle)
                {
                    Side = (int)Math.Sqrt(puzzle.Length);
                    if (Side * Side != puzzle.Length)
                        throw new Exception("Puzzle not a square number!: " + puzzle.Length.ToString());

                    initBoard();
                    
                    int[] iA = new int[Side * Side];
                    for (int i = 0; i < Side * Side; i++)
                    {
                        iA[i] = int.Parse(puzzle.Substring(i, 1));
                        int ii = i / Side;
                        board[i - (ii * Side), ii] = new sudokuCell(!0.Equals(iA[i]), this);
                    }

                    FillLinks();

                    for (int i = 0; i < Side * Side; i++)
                    {
                        int ii = i / Side;
                        if (iA[i] != 0)
                            board[i - (ii * Side), ii].Value = iA[i];
                    }
                }

                private void initBoard()
                {
                    sqrootSide = (int)Math.Sqrt(Side);
                    if (sqrootSide * sqrootSide != Side)
                        throw new Exception("Puzzle Side not a square number!: " + Side.ToString());

                    StartVector = (2 << Side) - 2; //Starting value for BitVector32. All possibilities for values > Side must be set to false

                    board = new sudokuCell[Side, Side];
                }

                /// <summary>
                /// Creates the links between the sudokuCells and their neighbors.
                /// </summary>
                private void FillLinks()
                {
                    for (int y = 0; y < Side; y++)
                    {
                        for (int x = 0; x < Side; x++)
                        {
                            sudokuCell sC = board[x, y];
							
                            //let it know where it is
                            sC.positX = x;
                            sC.positY = y;
                            
                            //filling in the row array
                            int i = 0;
                            for (int tX = 0; tX < Side; tX++)
                                if (tX != x)
                                    sC.row[i++] = board[tX, y];

                            //filling in the column array
                            i = 0;
                            for (int tY = 0; tY < Side; tY++)
                                if (tY != y)
                                    sC.column[i++] = board[x, tY];

                            //determining sub-square
                            int minX = sqrootSide * (x / sqrootSide);
                            int minY = sqrootSide * (y / sqrootSide);
                            int maxX = minX + sqrootSide - 1;
                            int maxY = minY + sqrootSide - 1;

                            //filling in the sub-square array                        
                            i = 0;
                            for (int tX = minX; tX <= maxX; tX++)
                                for (int tY = minY; tY <= maxY; tY++)
                                {
                                    if (tX != x || tY != y)
                                        sC.subsquare[i++] = board[tX, tY];
                                }
                        }
                    }
                }

                /// <summary>
                /// Tries to solve. Returns true if solved. False if you should try again.
                /// </summary>
                /// <returns></returns>
                public bool Solve()
                {
                    List<sudokuCell> q = new List<sudokuCell>();
                    for (int x = 0; x < Side; x++)
                        for (int y = 0; y < Side; y++)
                            if (board[x, y].Value == 0)
                                q.Add(board[x, y]);

                    if (q.Count.Equals(0))
                        return true; //already solved
                    else
                    {
                        int count = q.Count + 1; //creates an offset, allowing entrance into the while loop
                        while (count > q.Count)    //keep running as long as each iteration of the loop deacreases the number of unknown nodes
                        {
                            count = q.Count;
                            for (int i = 0; i < q.Count; i++)
                            {
                                sudokuCell sC = q[i];
                                if (sC.Value != 0 || sC.DeriveNodeValueFromGroupPossibilities())
                                {
                                    q.RemoveAt(i);
                                    i--;            //adjust index since we just shrunk Queue q
                                }
                            }
                        }
                    }
                    
                     if (q.Count.Equals(0))
                        return true; //already solved
                    else
                    {
                    	string b = this.ToString();
                    	int x = q[0].positX;
                    	int y = q[0].positY;
                    	BitVector32 bv = new BitVector32(q[0].GetPossibilities());
                    	List<string> solutions = new List<string>();
                    	for(int i=1; i<=9; i++)
                    	{
                    		if(bv[1<<i])
                    		{
                    			int index = (y*9 + x);
                    			try
                    			{
                    				string nS = b.Remove(index,1);
                    				nS = nS.Insert(index,i.ToString());
                    				sudokuBoard sb = new sudokuBoard(nS);
                    				while(!sb.Solve());
                    				solutions.Add(sb.ToString());
                    			}
                    			catch
                    			{
                    				q[0].RemovePossibility(i);
                    				return false;
                    			}
                    		}
                    	}
                    	if(solutions.Count==1)
                    	{
                    		sudokuBoard sb = new sudokuBoard(solutions[0]);
                    		this.board = sb.board;
                    		return true;
                    	}
                    	else
                    	{
                    		Console.WriteLine("Error");
                    	}
                    	return false;
                    }
                }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    for (int y = 0; y < Side; y++)
                    {
                        for (int x = 0; x < Side; x++)
                        {
                            sb.Append(board[x, y].Value);
                        }
                    }
                    return sb.ToString();
                }

                public int[] getCellData(int x, int y)
                {
                    sudokuCell sC = board[x, y];
                    int[] iA = new int[2];
                    iA[0] = sC.Value;
                    iA[1] = sC.GetPossibilities();
                    return iA;
                }

                public sudokuCell getCell(int x, int y)
                {
                    return board[x, y];
                }
            }

            /// <summary>
            /// Represents one cell in a sudoku board.
            /// </summary>
            public class sudokuCell
            {
                private BitVector32 possibilities;  //limits max square to 25x25, since it only supports up to 31 possible values.
                //possibilities[1] == True if cell is known. False if cell is unknown.
                //possibilities[1<<i] == True if i is a possible value. False if i is not a possible value.
                public sudokuCell[] row;    //all the sudokuCells in the same row
                public sudokuCell[] column; //all the sudokuCells in the same column
                public sudokuCell[] subsquare; //all the sudokuCells in the same subsquare.
                private int cellValue; //if this cell is known, contains its value. if this cell is not known, == the number of possibilities + Side.
                private int Side;
                public int positX;
                public int positY;
                
                private sudokuBoard Parent;

                public sudokuCell(bool valueKnown, sudokuBoard parent)
                {
                    this.Parent = parent;
                    Side = Parent.Side;
                    row = new sudokuCell[Side - 1];
                    column = new sudokuCell[Side - 1];
                    subsquare = new sudokuCell[Side - 1];
                    if (valueKnown)
                    {
                        cellValue = -1;
                        possibilities = new BitVector32(1);
                    }
                    else
                    {
                        cellValue = 2 * Side;
                        possibilities = new BitVector32(Parent.StartVector);
                    }
                }

                public int Value
                {
                    get
                    {
                        if (possibilities[1])
                            return cellValue;
                        else return 0;
                    }

                    set
                    {
                        cellValue = value;
                        possibilities[1] = true;
                        foreach (sudokuCell sC in row)
                            sC.RemovePossibility(cellValue);
                        foreach (sudokuCell sC in column)
                            sC.RemovePossibility(cellValue);
                        foreach (sudokuCell sC in subsquare)
                            sC.RemovePossibility(cellValue);
                    }
                }

                public int GetPossibilities()
                {
                    return possibilities.Data;
                }

                public void RemovePossibility(int Value)
                {
                    if (cellValue == Value)
                        throw new Exception("Solution not Possible!");
                    if (!possibilities[1] && possibilities[1 << Value])
                    {
                        possibilities[1 << Value] = false;
                        cellValue--;
                        if (cellValue == Side + 1)
                        {
                            possibilities[1] = true;
                            for (int i = 1; i <= Side; i++)
                                if (possibilities[1 << i])
                                {
                                    this.Value = i;
                                    return;
                                }
                        }
                    }
                }

                /// <summary>
                /// Checks if this cell is the only one in its row/column/subsquare that can possibly have a particular value.
                /// If this is the case, then it is automatically set to that value
                /// </summary>
                /// <returns>True if it was the case (ie Value has been discovered through this method), False otherwise (or already known).</returns>
                public bool DeriveNodeValueFromGroupPossibilities()
                {
                    if (possibilities[1]) return false;

                    List<int> cl = new List<int>();
                    List<int> rl = new List<int>();
                    List<int> sl = new List<int>();
                    for (int i = 1; i <= Side; i++)
                        if (possibilities[1 << i])
                        {
                            cl.Add(i); rl.Add(i); sl.Add(i);
                        }


                    //check subsquare
                    for (int i = 0; i < Side - 1; i++)
                        if (subsquare[i].Value == 0)
                            for (int ii = 1; ii <= Side; ii++)
                                if (subsquare[i].possibilities[1 << ii])
                                    sl.Remove(ii);
                    if (sl.Count == 1)
                    {
                        this.Value = sl[0];
                        return true;
                    }

                    //check column
                    for (int i = 0; i < Side - 1; i++)
                        if (column[i].Value == 0)
                            for (int ii = 1; ii <= Side; ii++)
                                if (column[i].possibilities[1 << ii])
                                    cl.Remove(ii);
                    if (cl.Count == 1)
                    {
                        Value = cl[0];
                        return true;
                    }

                    //check row
                    for (int i = 0; i < Side - 1; i++)
                        if (row[i].Value == 0)
                            for (int ii = 1; ii <= Side; ii++)
                                if (row[i].possibilities[1 << ii])
                                    rl.Remove(ii);
                    if (rl.Count == 1)
                    {
                        Value = rl[0];
                        return true;
                    }

                    return false;
                }

            }
	}
	
	class Euler095
	{
		public static void run()
		{
			int max = 1000000;
			int[][] res = lib.FactorsForAllNumsBelow(max);
			int[] facsum = Array.ConvertAll<int[],int>(res, summer);
			
			int longestLength=1;
			for(int i=0; i<max; i++)
			{
				List<int> chain = new List<int>();
				for(int r = i; r<max && !chain.Contains(r); r = facsum[r])
					chain.Add(r);
				if(chain[0]==facsum[chain[chain.Count-1]] && chain.Count>longestLength)
				{
					longestLength = chain.Count;
					Console.WriteLine("new: " + i.ToString() + " -> " + longestLength.ToString());
				}
			}
			Console.WriteLine("Done!");
		}
		
		public static int summer(int[] res)
		{
			int retval = 1;
			foreach(int i in res)
				retval+=i;
			return retval;
		}
		
		public class chain : ccObj<int, int>
		{			
			public int getVal(int x, calcCache<int, int> c)
			{
				if(x==89)
					return 89;
				if(x==1)
					return 1;
				string xS = x.ToString();
				int sqsum = 0;
				for(int i=0;i<xS.Length;i++)
				{
					int s = (int.Parse(xS.Substring(i,1)));
					sqsum+= s*s;
				}
				return c.getVal(sqsum);
			}
		}
	}
	
	class Euler094
	{
		public class sqrCalc
		{
			public sqrCalc(int max)
			{
				
			}
			
			public bool tryGetRoot(long square, out int root)
			{
				int newRoot = (int)Math.Sqrt(square);
				long newSquare = Math.BigMul(newRoot,newRoot);
				if(newSquare>square)
				{
					while(newSquare>square)
					{
						newSquare-=newRoot;
						newRoot--;
						newSquare-=newRoot;
					}
				}
				else
				{
					while(newSquare<square)
					{
						newSquare+=newRoot;
						newRoot++;
						newSquare+=newRoot;
					}
				}
				
				root = newRoot;
				return newSquare==square;
			}
			
			public long getSquare(int val)
			{
				return Math.BigMul(val,val);
			}
		}
		
		static sqrCalc sc;
		
		public static void run()
		{
			int sideCeiling = 333333333;
			sc = new sqrCalc(sideCeiling);
			
			long totalPerim = checkLow(sideCeiling); //check if 333333333,333333333,333333332 is valid, and if so put its value here
			for(int i=1;i<sideCeiling;i+=2) //diffside must be even, thus equiside must be odd.
			{
				totalPerim+=checkThing(i);
			}
			Console.WriteLine(totalPerim);
		}
		//area = .5 *(.5*diffSide*height)
		//height = sqrt(equiside^2 - (.5*diffSide)^2)
		//		equiside^2 = (.5*diffSide)^2 + height^2
		//Conclusions: diffside must have a minimum of 1 factors of 2 (must be even)
		//diffside and height must have a minimum of 2 factors of 2 between the two. (from the area equation)
		//			(diffside*height)%4==0 , (halfdiffside*height)%2==0
		//height must be a whole integer number
		//	thus: equiside^2 - (.5*diffSide)^2 == a squaredNumber
		//		
		static long checkThing(int equiSide)
		{
			long totalPerim = 0;
			
			long equiSqr = sc.getSquare(equiSide);
			int halfDiffSide = equiSide/2;	//divide rounds down, and equiside is odd, thus equiside/2 == (equiside-1)/2;
			long diffSqr = sc.getSquare(halfDiffSide);
			int height;
			if(sc.tryGetRoot(equiSqr-diffSqr,out height) && halfDiffSide!=0 && height!=0 && (halfDiffSide%2==0 || height%2==0))
			{
				totalPerim+=equiSide*3 - 1;
			}
			
			diffSqr += halfDiffSide;
			halfDiffSide++;
			diffSqr += halfDiffSide;
			
			if(sc.tryGetRoot(equiSqr-diffSqr,out height) && halfDiffSide!=0 && height!=0 && (halfDiffSide%2==0 || height%2==0))
			{
				totalPerim+=equiSide*3 + 1;
			}
			
			return totalPerim;
		}
		
		static long checkLow(int equiSide)
		{
			if(equiSide%2==0)
				return 0;
			
			long totalPerim = 0;
			
			long equiSqr = sc.getSquare(equiSide);
			int halfDiffSide = equiSide/2;	//divide rounds down, and equiside is odd, thus equiside/2 == (equiside-1)/2;
			long diffSqr = sc.getSquare(halfDiffSide);
			int height;
			if(sc.tryGetRoot(equiSqr-diffSqr,out height) && halfDiffSide!=0 && height!=0 && (halfDiffSide%2==0 || height%2==0))
			{
				totalPerim+=equiSide*3 - 1;
			}
			return totalPerim;			
		}
	}
	
	class Euler093
	{
		public static void run()
		{
			int[] ns = new int[2];
			int[][] digs = new int[2][];
			
			foreach(int[] digitSet in nCkSets.iterator(9,4))
			{
				HashSet<int> hs = new HashSet<int>();
				foreach(int[] digits in Permutation.iterator(digitSet))
				{
					foreach(int[] ops in Dice.allPossibleRolls(3,4))
					{
						List<decimal> expression = new List<decimal>();
						for(int i=0; i<ops.Length; i++)
						{
							expression.Add(digits[i]);
							expression.Add(ops[i]);
						}
						expression.Add(digits[ops.Length]);
						
						hs.UnionWith(eval(expression));
					}
				}
				
				int n = 0;
				while( hs.Contains(n+1) )
					n++;
				
				if(n>ns[0])
				{
					ns[0] = n;
					digs[0] = digitSet;
					Array.Sort(ns, digs);
				}
			}
			
			for(int i = ns.Length-1; i>=0; i--)
			{
				int[] d = digs[i];
				Console.WriteLine(ns[i].ToString() + ": " + (1000*d[0]+100*d[1]+10*d[2]+d[3]).ToString());
			}
		}
		
		private static List<int> eval(List<decimal> expression)
		{
			List<int> hs = new List<int>();
			if(expression.Count==1)
			{
				var currVal = expression[0];
				var roundedCurr = decimal.Round(currVal);
				if(Math.Abs(roundedCurr-currVal)>.0000000000001m)
					hs.Add(0);
				else
					hs.Add((int)currVal);
			}
			
			for(int i=1; i<expression.Count; i+=2)
			{
				decimal result = eval(expression[i-1],(int)expression[i],expression[i+1]);
				
				if(result!=decimal.MaxValue)
				{
					var expressionClone = new List<decimal>(expression);
					expressionClone[i-1] = result;
					expressionClone.RemoveAt(i);
					expressionClone.RemoveAt(i);
					hs.AddRange(eval(expressionClone));
				}
			}
			
			return hs;
		}
		
		private static decimal eval(decimal a, int op, decimal b)
		{
			switch (op)
			{
			    case 1:
					a += b;
			        break;
			    case 2:
			        a -= b;
			        break;
			    case 3:
					a *= b;
			        break;
			    default:
			        if(b==0)
			        	a = decimal.MaxValue;
			        else
				        a /= b;
			        break;
			}
			return a;
		}
		
		public class Dice
		{
			int numDice;
			int maxDiceValue;
			int[] curr;
			bool done;
			
			public Dice(int numDice, int maxDiceValue)
			{
				this.numDice = numDice;
				this.maxDiceValue = maxDiceValue;
				curr = new int[numDice];
				for(int i=0; i<curr.Length; i++)
					curr[i] = 1;
			}
			
			public void increment()
			{
				int incrementIndex = 0;
				while(incrementIndex<curr.Length && curr[incrementIndex]==maxDiceValue)
				{
					curr[incrementIndex]=1;
					incrementIndex++;
				}
				if(incrementIndex==curr.Length)
					done = true;
				else
					curr[incrementIndex]++;
			}
			
			public int[] getCurr()
			{
				int[] curr2 = new int[curr.Length];
				Array.Copy(curr,curr2,curr.Length);
				return curr2;
			}
			
			public bool isDone()
			{
				return done;
			}
			
			public static IEnumerable<int[]> allPossibleRolls(int numDice, int maxDiceValue)
			{
				Dice d = new Dice(numDice,maxDiceValue);
				for(int[] i = d.getCurr(); !d.isDone(); d.increment())
					yield return d.getCurr();
			}
		}
		
		public class nCkSets
		{
			int[] curr;
			int n;
			bool done;
			
			public nCkSets(int n, int k)
			{
				if(n<=k || k==0)
					done = true;
				else
					done = false;
				
				this.n = n;
				curr = new int[k];
				
				fillEnumerated(curr, Math.Min(k,n));
			}
			
			private static void fillEnumerated(int[] iA, int length)
			{
				for(int i=0; i<length;i++)
					iA[i]=i+1;
			}
			
			public int[] getCurr()
			{
				int[] curr2 = new int[curr.Length];
				Array.Copy(curr,curr2,curr.Length);
				return curr2;
			}
			
			public bool isDone()
			{
				return done;
			}
			
			public void increment()
			{
				if(done) return;
				
				int incrementIndex = 0;
				while(incrementIndex<curr.Length-1)
				{
					if(curr[incrementIndex] +1 < curr[incrementIndex+1])
						break;
					else
						incrementIndex++;
				}
							
				curr[incrementIndex]++;
				
				fillEnumerated(curr, incrementIndex);
				
				//if true means we're done permutin',
				//cause our search for an index to increment has reached the last,
				//and it's value has gone beyond n
				done = (curr[incrementIndex]>n);
			}
			
			public static IEnumerable<int[]> iterator(int n, int k)
			{
				nCkSets nck = new nCkSets(n,k);
				for(int[] i = nck.getCurr(); !nck.isDone(); nck.increment())
					yield return nck.getCurr();
			}
		}		
		
		public class Permutation
		{
			int[] current;
			int[] end;
			bool done;
			
			public Permutation(int numberItems)
			{
				current = new int[numberItems];
				end = new int[numberItems];
				for(int i=0;i<numberItems;i++)
				{
					current[i] = i;
					end[i] = numberItems-(i+1);
				}
				done = false;
			}
			
			public int[] getCurr()
			{
				int[] curr2 = new int[current.Length];
				Array.Copy(current,curr2,current.Length);
				return curr2;
			}
			
			public bool isDone()
			{
				return done;
			}
			
			public void increment()
			{
				if(lib.ArrayEqual<int>(current,end))
					done = true;
				if(done) return;
				
				int t;
                if (current[current.Length - 1] > current[current.Length - 2])
                {	//Console.WriteLine("next is quick");
                    t = current[current.Length - 1];
                    current[current.Length - 1] = current[current.Length - 2];
                    current[current.Length - 2] = t;
                }
                else
                {	//Console.WriteLine("next is slow");
                    int i = current.Length - 2;
                    while (current[i] > current[i + 1]) i--;
                    int j = newIntI(current, i);
                    t = current[i];
                    current[i] = current[j];
                    current[j] = t;

                    int[] buffer = new int[current.Length];
                    for (int tt = 0; tt < buffer.Length; tt++)
                        buffer[tt] = -1;
                    int bufferI = 0;
                    for (t = i + 1; t < current.Length; t++)
                        buffer[bufferI++] = current[t];
                    for (j = current.Length - 1; j > i; j--)
                    {
                        int maxBuffI = 0;
                        for (t = 1; t < bufferI; t++)
                            if (buffer[maxBuffI] < buffer[t]) maxBuffI = t;
                        current[j] = buffer[maxBuffI];
                        buffer[maxBuffI] = -1;
                    }
                }
			}
			
			private static int newIntI(int[] a, int startI)
            {
                int res = startI + 1;
                for (int j = startI + 2; j < a.Length; j++)
                    if (a[j] > a[startI] && a[j] < a[res]) res = j;
                return res;
            }
			
			public static IEnumerable<T[]> iterator<T>(T[] n)
			{				
				for(Permutation nck = new Permutation(n.Length); !nck.isDone(); nck.increment())
				{
					int[] perm = nck.getCurr();
					T[] permed = new T[n.Length];
					for(int i=0; i<n.Length; i++)
						permed[i] = n[perm[i]];
					
					yield return permed;
				}
			}
			
			public static IEnumerable<int[]> iterator(int n)
			{
				for(Permutation nck = new Permutation(n); !nck.isDone(); nck.increment())
					yield return nck.getCurr();
			}
		}
				
		public static long nCk(int n, int k)
		{
			return (factorial(n)/factorial(k))/factorial(n-k);
		}
		
		public static long factorial(int x)
		{
			long val = 1;
			for(int i=1; i<=x; i++)
				val*=i;
			return val;
		}
	}
	
	class Euler092
	{
		public static void run()
		{
			calcCache<int,int> cc = new standardCalcCache<int, int>(new chain());
			int count = 0;
			for(int i=1; i<10000000; i++)
				if(cc.getVal(i)==89)
					count++;
			Console.WriteLine(count);
		}
		
		public class chain : ccObj<int, int>
		{			
			public int getVal(int x, calcCache<int, int> c)
			{
				if(x==89)
					return 89;
				if(x==1)
					return 1;
				string xS = x.ToString();
				int sqsum = 0;
				for(int i=0;i<xS.Length;i++)
				{
					int s = (int.Parse(xS.Substring(i,1)));
					sqsum+= s*s;
				}
				return c.getVal(sqsum);
			}
		}
	}
	
	class Euler091
	{
		public static void run()
		{
			int size = 50;
			calcCache<point,int> cc = new standardCalcCache<point, int>(new commonDiv(size));
			
			long total = 0;
			point p1 = new point(0,0);
			for(p1.a=0; p1.a<=size; p1.a++)
			{
				for(p1.b=0;p1.b<=size;p1.b++)
				{
					if(p1.a==0 && p1.b==0)
					{
						total+=size*size;
					}
					else if(p1.a==0)
					{
						total+=size;
					}
					else if(p1.b==0)
					{
						total+=size;
					}
					else
					{
						int cd = cc.getVal(p1);
						point invSlope = new point(p1.b/cd, -p1.a/cd);
						for(point p2 = p1-invSlope; p2.a>=0 && p2.b>=0 && p2.a<=size && p2.b<=size; p2-=invSlope)
						{
							total++;
						}
						for(point p2 = p1+invSlope; p2.a>=0 && p2.b>=0 && p2.a<=size && p2.b<=size; p2+=invSlope)
						{
							total++;
						}
					}
				}
			}
			Console.WriteLine(total);
		}
				
		public class commonDiv : ccObj<point,int>
		{
			HashSet<int>[] primeFactors;
			public commonDiv(int max)
			{
				int[][] pFs = lib.primeFactorsForAllNumsBelow(max+1);
				primeFactors = Array.ConvertAll<int[],HashSet<int>>(pFs, tohashset);
			}
			
			private HashSet<int> tohashset(int[] iA)
			{
				return new HashSet<int>(iA);			
			}
			
			public int getVal(point x, calcCache<point, int> c)
			{
				int retval = 1;
				HashSet<int> tH = new HashSet<int>(primeFactors[x.a]);
				tH.IntersectWith(primeFactors[x.b]);
				if(tH.Count!=0)
				{
					foreach(int fact in tH)
						retval*=fact;
					retval *= c.getVal(new point(x.a/retval, x.b/retval)) ;
				}
				return retval;
			}
		}
	}
	
	class Euler088
	{
		static int upto = 12000;
		public static void run()
		{
			int[] results = new int[upto+1];
			for(int i =2; i<results.Length; i++)
				results[i] = int.MaxValue;
			
			List<mss> cache = new List<mss>();
			bool keepGoing = true;
			for(int currNum = 2; keepGoing; currNum++)
			{
				List<mss> forNum = forAnum(currNum, results);
				List<mss> combined = combine(cache, forNum, results);
				
				keepGoing = check(results);
				if(keepGoing)
				{
					cache.AddRange(forNum);
					cache.AddRange(combined);
				}
			}
			cache.Clear();
			
			HashSet<int> pants = new HashSet<int>(results);
			long total = 0;
			foreach(int ii in pants)
				total += ii;
				
			Console.WriteLine(total);
		}
		
		private static List<mss> forAnum(int num, int[] results)
		{
			List<mss> retVal = new List<mss>();
			mss origin = new mss(num,num, 1);
			for(int z = origin.calcEquivalentSetSize(); z<=upto; z = origin.calcEquivalentSetSize())
			{
				retVal.Add(origin);
				if(origin.mult<results[z])
					results[z] = origin.mult;
				origin += num;
			}
			return retVal;
		}
		
		private static List<mss> combine(List<mss> cache, List<mss> numList, int[] results)
		{
			List<mss> retVal = new List<mss>();
			
			foreach(mss val in cache)
			{
				int currIndex = 0;
				for(int z = -1; z<=upto && currIndex<numList.Count; currIndex++)
				{
					mss combed = val + numList[currIndex];	
					z = combed.calcEquivalentSetSize();
					if(z<=upto)
					{
						retVal.Add(combed);
						if(combed.mult<results[z])
							results[z] = combed.mult;
					}
				}
			}
			return retVal;
		}
		
		private static bool check(int[] results)
		{
			foreach(int a in results)
			{
				if(a==int.MaxValue)
					return true;
			}
				
			return false;
		}
			    
		private struct mss
		{
			public int mult;
			int sum;
			int setSize;
			
			public mss (int mult, int sum, int setSize)
			{
				this.mult = mult;
				this.sum = sum;
				this.setSize = setSize;
			}
			
			public static mss operator + (mss a, mss b)
			{
				a.mult*= b.mult;
				a.sum+=b.sum;
				a.setSize+=b.setSize;
				return a;
			}
			
			public static mss operator + (mss a, int b)
			{
				a.mult*= b;
				a.sum+=b;
				a.setSize+=1;
				return a;
			}
			
			public int calcEquivalentSetSize()
			{
				return (mult-sum) + setSize;
			}
			
			public override string ToString ()
			{
				return "Mult: " + mult.ToString() + " Sum: " + sum.ToString() + " SetSize: " + setSize.ToString();
			}
		}
	}
	
	class Euler087
	{
		public static void run()
		{
			Dictionary<int, bool> numbers = new Dictionary<int, bool>();
			int limit = 50;
			limit *= 1000000;
			int sqrtLimit = 1+(int)(Math.Sqrt(limit));
			var iap = lib.allPrimesBelow(sqrtLimit+200);
			
			List<int> squares = new List<int>();
			List<int> cubes = new List<int>();
			List<int> tetras = new List<int>();
			
			int prime = 0; int i=0;
			while(prime<sqrtLimit)
			{
				prime = iap[i++];
				long sq = prime*prime;
				squares.Add((int)sq);
				
				sq*= prime;
				if(sq<limit)
				{
					cubes.Add((int)sq);
					sq*= prime;
					if(sq<limit)
					{
						tetras.Add((int)sq);
					}
				}
			}
			
			foreach(int sq in squares)
			{
				foreach(int cu in cubes)
				{
					int preCurr = cu + sq;
					foreach(int tet in tetras)
					{
						int curr = preCurr + tet;
						if(curr<limit)
						{
							if(!numbers.ContainsKey(curr))
							{
								numbers.Add(curr,true);
							}
						}
						else
							break;
					}
				}
			}
			
			Console.WriteLine(numbers.Count);
		}
	}
	
	class Euler085
	{
		public static void run()
		{
			int goal = 2000000;
			qManager q = new qManager(100);
			calcCache<duple, int> cc = new standardCalcCache<duple, int>(new squareCount());
			int closestVal = 0;
			duple closest = new duple(0,0);
			bool trip = true;
			while(trip)
			{
				trip = false;
				for(int reps = 0; reps<3; reps++)
					for(int i=q.total+1; i>0; i--)
					{
						duple val = q.getNext();
						int res = cc.getVal(val);
						if(Math.Abs(goal-res) < Math.Abs(goal-closestVal))
						{
							closestVal = res;
							closest = val;
							Console.WriteLine(val);
							trip = true;
						}
					}
			}
			Console.WriteLine("Answer: " + (closest.a*closest.b).ToString());
		}
		
		public class squareCount : ccObj<duple, int>
		{
			public int getVal(duple d, calcCache<duple, int> c)
			{
				int x = d.a;
				int y = d.b;
				
				if(x<=0) return 0;
			
				int Total = c.getVal(new duple(x-1,y));
				for(int i = y+1 ; i>0; i--)
				{
					Total+= (i-1)*(x);
				}
				return Total;
			}
		}
		
		public static int count2(int x, int y)
		{
			if(x<0) return 0;
			
			int Total = count2(x-1,y);
			for(int i = y+1 ; i>0; i--)
			{
				Total+= (i-1)*(x);
			}
			return Total;
		}
		
		public static int count(int x, int y)
		{
			x++; y++; //this method counts on the lines, and lines = squares + 1;
			
			int total = 0;
			for(int xC = 0; xC<x; xC++)
			{
				for(int yC = 0; yC<y; yC++)
				{
					total += (x-(xC+1))*(y-(yC+1));
				}
			}
			return total;
		}
		
		public class qManager
		{
			public int total = 1;
			Queue<duple> q;
			public qManager()
			{
				 q = new Queue<duple>();
			}
			public qManager(int total)
			{
				 this.total = total;
				 q = new Queue<duple>();
			}
			
			public duple getNext()
			{
				while(q.Count==0)
					genMore();
				return q.Dequeue();
			}
			
			private void genMore()
			{
				total++;
				for(int a = 1; a<=total/2; a++)
				{
					int b = total-a;
					q.Enqueue(new duple(a,b));
				}
			}
		}
	
		public struct duple : IComparable<duple>
		{
			public int a;
			public int b;
			
			public duple(int a, int b)
			{
				this.a = a;
				this.b = b;
			}
			
			public override string ToString()
			{
				return "(A,B): (" + a.ToString() + "," + b.ToString() + ")";
			}
			
			public int CompareTo(duple other)
			{
				return 0;
			}
		}
	}
	
	class Euler083
	{
		public static void run()
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\matrix.txt");
			int[][] matrix = Array.ConvertAll<string, int[]>(lines,hyperparse);
			
			int maxInt = int.Parse(run81(matrix).ToString());
			
			recursed83 cc = new recursed83(matrix, maxInt);
			cc.recurse(new point(0,0), 0);
			Console.WriteLine(cc.minValMatrix[matrix.Length-1][matrix.Length-1]);
		}
		
		private class recursed83
		{
			static int Max;
			
			int[][] matrix;
			public int[][] minValMatrix;
			
			public recursed83(int[][] matrix, int max)
			{
				Max = max;
				this.matrix = matrix;
				int size = matrix.Length;
				minValMatrix = Array.ConvertAll<int[],int[]>(matrix,maxArrayConv);
			}
			
			private int[] maxArrayConv(int[] item)
			{
				return Array.ConvertAll<int,int>(item, maxIntConv);
			}
			private int maxIntConv(int item)
			{
				return int.MaxValue;
			}
			
			public void recurse(point p, int currentVal)
			{
				int x = p.a;
				int y = p.b;
				
				currentVal+=matrix[x][y];
				
				if(currentVal<Max && currentVal<minValMatrix[x][y])
				{
					int size = matrix.Length-1;
					minValMatrix[x][y] = currentVal;
					if(x>0)
						recurse(new point(x-1,y), currentVal);
					if(y>0)
						recurse(new point(x,y-1), currentVal);
					if(x<size)
						recurse(new point(x+1,y), currentVal);
					if(y<size)
						recurse(new point(x,y+1), currentVal);
				}
			}			
		}
		
		public static zint run81(int[][] matrix)
		{
			calcCache<point,zint> cc = new standardCalcCache<point, zint>(new recursed81(matrix));
			return cc.getVal(new point(0,0));
		}
		
		private static int[] hyperparse(string s)
		{
			string[] sts = s.Split(new char[]{','});
			int[] ints = Array.ConvertAll<string, int>(sts,int.Parse);
			return ints;
		}
		
		private class recursed81 : ccObj<point, zint>
		{
			int[][] matrix;
			
			public recursed81(int[][] matrix)
			{
				this.matrix = matrix;
			}
			
			public zint getVal(point p, calcCache<point, zint> c)
			{
				int x = p.a;
				int y = p.b;
				int size = matrix.Length-1;
				if(x==size && y==size)
					return new zint(matrix[x][y]);
				else
				{
					zint z = null;
					if(x<size)
						z = c.getVal(new point(x+1, y));
					if(y<size)
					{
						if(z==null)
							z = c.getVal(new point(x, y+1));
						else
						{
							zint t = c.getVal(new point(x, y+1));
							if(z.CompareTo(t)>0)
								z = t;
						}
					}
					z = z.duplicate();
					z.addTo(matrix[x][y]);
					return z;
				}
			}
			
		}
	}
	
	class Euler082
	{
		public static zint MAX;
		
		public static void run()
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\matrix.txt");
			int[][] matrix = Array.ConvertAll<string, int[]>(lines,hyperparse);
			calcCache<point,zint> cc = new standardCalcCache<point, zint>(new recursed(matrix));
			MAX = new zint(long.MaxValue);
			MAX.multWith(int.MaxValue);
			zint mini = MAX;
			for(int i=0; i<matrix.Length; i++)
			{
				//Console.WriteLine(matrix[i][0]);
				zint tm = cc.getVal(new point(i,0));
				if(mini.CompareTo(tm)>0)
				{
					mini = tm;
				}
			}
			Console.WriteLine(mini);
		}
		
		private static int[] hyperparse(string s)
		{
			string[] sts = s.Split(new char[]{','});
			int[] ints = Array.ConvertAll<string, int>(sts,int.Parse);
			return ints;
		}
		
		private class recursed : ccObj<point, zint>
		{
			int[][] matrix;
			
			public recursed(int[][] matrix)
			{
				this.matrix = matrix;
			}
			
			public zint getVal(point p, calcCache<point, zint> c)
			{
				int x = p.a;
				int y = p.b;
				int size = matrix.Length-1;
				if(y==size)
					return new zint(matrix[x][y]);
				else
				{
					zint mini = c.getVal(new point(x,y+1)).duplicate();
					
					long curr = 0;
					for(int ix = 1; ix<=x;ix++)
					{
						curr+=matrix[x-ix][y];
						zint tz = c.getVal(new point(x-ix,y+1)).duplicate();
						tz.addTo(curr);
						if(mini.CompareTo(tz)>0)
								mini = tz;
					}
					
					curr = 0;					
					for(int ix = 1; ix+x<=size;ix++)
					{
						curr+=matrix[x+ix][y];
						zint tz = c.getVal(new point(x+ix,y+1)).duplicate();
						tz.addTo(curr);
						if(mini.CompareTo(tz)>0)
								mini = tz;
					}
					
					mini.addTo(matrix[x][y]);
					return mini;
				}
			}
			
		}
	}
	
	class Euler081
	{
		public static void run()
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\matrix.txt");
			int[][] matrix = Array.ConvertAll<string, int[]>(lines,hyperparse);
			calcCache<point,zint> cc = new standardCalcCache<point, zint>(new recursed(matrix));
			Console.WriteLine(cc.getVal(new point(0,0)));
		}
		
		private static int[] hyperparse(string s)
		{
			string[] sts = s.Split(new char[]{','});
			int[] ints = Array.ConvertAll<string, int>(sts,int.Parse);
			return ints;
		}
		
		private class recursed : ccObj<point, zint>
		{
			int[][] matrix;
			
			public recursed(int[][] matrix)
			{
				this.matrix = matrix;
			}
			
			public zint getVal(point p, calcCache<point, zint> c)
			{
				int x = p.a;
				int y = p.b;
				int size = matrix.Length-1;
				if(x==size && y==size)
					return new zint(matrix[x][y]);
				else
				{
					zint z = null;
					if(x<size)
						z = c.getVal(new point(x+1, y));
					if(y<size)
					{
						if(z==null)
							z = c.getVal(new point(x, y+1));
						else
						{
							zint t = c.getVal(new point(x, y+1));
							if(z.CompareTo(t)>0)
								z = t;
						}
					}
					z = z.duplicate();
					z.addTo(matrix[x][y]);
					return z;
				}
			}
			
		}
	}
	
	class Euler080
	{
		public static void run3()
		{
			string s = "";
			for(int i=1; i<100; i++)
				s+= i.ToString();
			double dd = double.Parse(s);
			zint zz = zint.Parse(s);
			double estm = zz.approx();
			Console.WriteLine();
		}
		
		public static void run2()
		{
			var q = new ScratchPad1.Euler063.qManager(7000);
			while(true)
			{
				var d = q.getNext();
				
				for(int ai=0; ai<16; ai++)				
				for(int bi=0; bi<16; bi++)
				{
					long test = ((long)Math.Pow(10,bi))*((long)d.b);
					long t = ((long)Math.Pow(10,ai))*((long)d.a);
					test /= t;	
					
					zint zb = new zint(d.b);
					zb.ee(bi);
					zint za = new zint(d.a);
					za.ee(ai);
					zb.DivideBy(za,0);
					if(!test.ToString().Equals(zb.ToString()))
					{
						Console.WriteLine("error");
					}
				}
			}
		}
		
		public static void run()
		{
			List<int> squares = new List<int>();
			for(int i=2; i<12; i++)
				squares.Add(i*i);
			
			List<int> q = new List<int>();
			for(int i=2; i<100;i++)
			{
				if(!squares.Contains(i))
					q.Add(i);
			}
			
			long total = 0;
			foreach(int n in q)
			{
				zint z = sqrt(n);
				string zs = z.ToString().Substring(0, 100);
				total+=lib.DigitSum(zs);
			}
			Console.WriteLine(total);
		}
		
		public static zint sqrt(int n)
		{
			zint num = new zint(n);
			num.ee(300);
			
			double g = Math.Sqrt(n);
			g*= (double)1E150;
			zint guess = new zint(g);
			zint oldGuess = new zint(1);
			while(guess!=oldGuess)
			{
				oldGuess = guess;
				zint t = num.duplicate();
				t.DivideBy(guess, 0);
				t.addTo(guess);
				t.DivideBy(2);
				guess = t;
			}
			return guess;
		}
	}
	
	class Euler079
	{
		public static void run()
		{
			
			Gamut g = new Gamut();
			
			int last = 0;			
			for(int i=0; !g.passGamut(i) ;i++)
			{
				last = i;
			}
			
			Console.WriteLine(last+1);
		}
		
		public class Gamut
		{
			char[][] gamut;
			
			public Gamut()
			{
				gamut = new char[50][];
				string[] lines = System.IO.File.ReadAllLines(@"C:\temp\keylog.txt");
				int gamutIndex = 0;
				foreach(string line in lines)
				{
					gamut[gamutIndex++] = line.ToCharArray();
				}
			}
			
			public Gamut(bool test)
			{
				//531278
				gamut = new char[][]{
									new char[]{'5','3','1'},new char[]{'5','1','8'},new char[]{'3','1','7'},new char[]{'1','7','8'},
									new char[]{'2','7','8'},new char[]{'3','2','8'},new char[]{'5','2','7'},new char[]{'1','2','8'}};
			}
			
			public bool passGamut(int x)
			{
				string xS = x.ToString();
				foreach(char[] test in gamut)
				{
					int index = -1;
					for(int i=0; i<test.Length;i++)
					{
						index = xS.IndexOf(test[i], index+1);
						if(index<0)
							return false;
					}
				}
				return true;
			}
		}
	}
	
	class Euler078
	{
		public static void run1()
		{
			int num = 5;
			calcCache<duple,int> cc = new standardCalcCache<duple, int>(new recursed());
			Console.WriteLine(cc.getVal(new duple(num,num)));
		}
		
		public static void run()
		{
			int num = 5;//2350; //min value so far...
			calcCache<duple,int> cc = new standardCalcCache<duple, int>(new recursed());
			while(cc.getVal(new duple(num,num))!=0)
				num++;
			Console.WriteLine(num);
		}
		
		private class recursed : ccObj<duple, int>
		{
			public int getVal(duple p, calcCache<duple, int> c)
			{
				int prev = p.a;
				int remain = p.b;
								
				int z = 0;
				for(int i=1; i<=prev && i<=remain; i++)
				{
					int newRem = remain-i;
					if(newRem==0)
						z++;
					else
						z+=c.getVal(new duple(i, newRem));
				}
				z%= 1000000;					
				return z;
			}
			
		}
		
		public struct duple : IComparable<duple>
		{
			public int a;
			public int b;
			
			public duple(int a, int b)
			{
				this.a = a;
				this.b = b;
			}
			
			public override int GetHashCode()
			{
				return a^b;
			}
			
			public override string ToString()
			{
				return "(A,B): (" + a.ToString() + "," + b.ToString() + ")";
			}
			
			public int CompareTo(duple other)
			{
				return 0;
			}
		}
	}
	
	class Euler077
	{
		public static void run()
		{
			var r = new Recurser(new lib.InfiniteArrayOfPrimes2(10000));
			int i = 10;
			for(int res = r.recurse(i); res<5000; res = r.recurse(++i))
				Console.WriteLine(i.ToString() + " : " + res.ToString());
			Console.WriteLine("Answer = " + i.ToString());
		}
		
		public class Recurser : ccObj<point, int>
		{
			IList<int> iap;
			calcCache<point, int> cc;
			public Recurser(IList<int> iap)
			{
				this.iap = iap;
				cc = new standardCalcCache<point, int>(this);
			}
			
			public int recurse(int val)
			{
				int index = iap.IndexOf(val);
				if(index<0)
					index = ~ index;
				return cc.getVal(new point(val, index));
			}
			
			public int getVal(point x, calcCache<point, int> c)
			{
				int val = x.a;
				int lastPrimeIndex = x.b;
				if(val==0)
					return 1;
				else if(lastPrimeIndex<0)
					return 0;
				else
				{					
					if(iap[lastPrimeIndex]>val)
						return c.getVal(new point(val,lastPrimeIndex-1));
					else
					{
						int tot = 0;
						for(int i=lastPrimeIndex; i>=0; i--)
						{
							tot+= c.getVal(new point(val - iap[i], i));
						}
						return tot;
					}
				}
			}
		}
	}
	
	class Euler076
	{
		public static void run()
		{
			int num = 100;
			calcCache<point,zint> cc = new standardCalcCache<point, zint>(new recursed());
			Console.WriteLine(decimal.Parse(cc.getVal(new point(num,num)).ToString())-1);
		}
		
		private class recursed : ccObj<point, zint>
		{
			public zint getVal(point p, calcCache<point, zint> c)
			{
				int prev = p.a;
				int remain = p.b;
				
				if(remain==0)
					return new zint(1);
				else
				{
					zint z = new zint(0);
					for(int i=1; i<=prev && i<=remain; i++)
					{
						z.addTo(c.getVal(new point(i, remain-i)));
					}
					return z;
				}
			}
			
		}
	}
	
	class Euler075
	{
		public static void run()
		{
			HashSet<int> answers = new HashSet<int>();
			HashSet<int> dupes = new HashSet<int>();
			Dictionary<long,int> sqrt = new Dictionary<long, int>();
			const int max = 1500000;
			for(int a=(max*2)/3; a>0;a--)
			{
				sqrt.Add(Math.BigMul(a,a),a);
				for(int b = a; b>0; b--)
				{
					long sum = Math.BigMul(a,a) + Math.BigMul(b,b);
					int hyp;
					if(sqrt.TryGetValue(sum,out hyp))
					{
						int circum = a + b + hyp;
						if(circum<=max && !dupes.Contains(circum))
						{
							if(!answers.Contains(circum))
							{
								answers.Add(circum);
								Console.WriteLine(a);
							}
							else
							{
								dupes.Add(circum);
								answers.Remove(circum);
							}
						}
					}
				}
			}
			
			Console.WriteLine("Answers = " + answers.Count.ToString());
		}
		
		
		public class qManager
		{
			public int total = 1;
			Queue<duple> q;
			public qManager()
			{
				 q = new Queue<duple>();
			}
			public qManager(int total)
			{
				 this.total = total;
				 q = new Queue<duple>();
			}
			
			public duple getNext()
			{
				while(q.Count==0)
					genMore();
				return q.Dequeue();
			}
			
			private void genMore()
			{
				total++;
				for(int a = 1; a<=total/2; a++)
				{
					int b = total-a;
					if(b!=a)
						q.Enqueue(new duple(a,b));
				}
			}
		}
	
		public struct duple
		{
			public int a;
			public int b;
			
			public duple(int a, int b)
			{
				this.a = a;
				this.b = b;
			}
			
			public override string ToString()
			{
				return "(A,B): (" + a.ToString() + "," + b.ToString() + ")";
			}
		}
	}
	
	class Euler074
	{
		public static void run()
		{
			int chainLength = 61;
			calcCache<long,long> cc = new standardCalcCache<long, long>(new digitFactSum());
			List<long> buffer = new List<long>();
			int count = 0;
			for(int i=999999; i>1; i--)
			{
				if(isSpecial(buffer, cc, i, chainLength))
					count++;
			}
			Console.WriteLine(count);
		}
		
		private static bool isSpecial(List<long> buffer, calcCache<long,long> cc, int i, int chainLength)
		{
			buffer.Clear();
			buffer.Add(i);
			for(int x = 0; x<chainLength-1;x++)
			{
				long res = cc.getVal(buffer[buffer.Count-1]);
				if(buffer.Contains(res))
					return false;
				else
					buffer.Add(res);
			}
			long res2 = cc.getVal(buffer[buffer.Count-1]);
			if(buffer.Contains(res2))
				return true;
			else
				return false;
		}
		
		private class digitFactSum : ccObj<long, long>
		{
			int[] factorials;
				
			public digitFactSum()
			{
				factorials = new int[10];
				factorials[0] = 1;
				for(int i=1; i<factorials.Length; i++)
				{
					factorials[i] = i*factorials[i-1];
				}
			}
			
			public long getVal(long x, calcCache<long, long> c)
			{
				long retVal = 0;
				char[] cA = x.ToString().ToCharArray();
				foreach(char ch in cA)
				{
					retVal+= factorials[(((int)ch)-48)];
				}
				return retVal;
			}
		}
	}
	
	class Euler073
	{
		public static void run()
		{
			int max = 12000;
			
			int[][] facts = lib.primeFactorsForAllNumsBelow(max+1);
			long count = 0;
			Fraction Max = new Fraction(1,2);
			Fraction Min = new Fraction(1,3);
			for(int den = max; den>1; den--)
			{
				for(int num = 1; num<den; num++)
				{
					if(isCoprime(facts[num], facts[den]))
					{
						Fraction tF = new Fraction(num, den);
						if(Max.CompareTo(tF)>0) //if Max is greater than the new value
						{
							if(Min.CompareTo(tF)<0) //if Min is smaller than, cool beans.
							{
								count++;
							}
						}
						else			//if Max is smaller or equal, we've gone too far
							break;
					}
				}
			}
			Console.WriteLine(count);
		}
				
		private static bool isCoprime(int[] factorsA, int[] factorsB)
		{
			foreach(int a in factorsA)
			{
				if(0<=Array.BinarySearch<int>(factorsB,a))
					return false;
			}
			return true;
		}
		
		public class Fraction :  System.IComparable<Fraction>
		{
			public int numerator;
			public int denominator;
			private decimal val;			
			
			public Fraction(int numerator, int denominator)
			{
				this.numerator = numerator;
				this.denominator = denominator;
				val = numerator;
				val/=denominator;
			}
			
			public override string ToString()
			{
				return numerator.ToString() + "/" + denominator.ToString();
			}
			
			public int CompareTo(Fraction other)
			{
				return val.CompareTo(other.val);
			}
		}
	}
	
	class Euler072
	{		
		public static void run()
		{
			int max = 1000000;
			long count = 0;
			var factors = lib.primeFactorsForAllNumsBelow(max+1);
			for(int den = max; den>1; den--)
			{
				count+=Euler070.phi(den, factors[den]);
			}
			Console.WriteLine(count);
		}
	}
	
	class Euler071
	{
		public static void run()
		{
			int max = 1000000;
			
			Fraction currMax = new Fraction(1,max);
			Fraction comp = new Fraction(3,7);
			for(int den = max; den>1; den--)
			{
				for(int num = 1; num<den; num++)
				{
					Fraction tF = new Fraction(num, den);
					if(comp.CompareTo(tF)>0)
					{
						if(tF.CompareTo(currMax)>=0)
						{
							currMax = tF;
						}
					}
					else
						break;
				}
			}
			currMax.Simplify();
			Console.WriteLine(currMax);
		}
		
		public class Fraction :  System.IComparable<Fraction>
		{
			public int numerator;
			public int denominator;
			private decimal val;			
			
			public Fraction(int numerator, int denominator)
			{
				this.numerator = numerator;
				this.denominator = denominator;
				val = numerator;
				val/=denominator;
			}
			
			public override string ToString()
			{
				return numerator.ToString() + "/" + denominator.ToString();
			}
			
			public int CompareTo(Fraction other)
			{
				return val.CompareTo(other.val);
			}
			
			public void Simplify()
			{
				;
			}
		}
	}
	
	class Euler070
	{
		public static void run()
		{
			int until = 10000000;			
			
			int[][] factors = lib.primeFactorsForAllNumsBelow(until);
			
			int n = 0;
			double nratio = 10;
			
			for(int a = until-1; a>1; a--)
			{
				int phiN = phi(a, factors[a]);
				double ratio = ((double)a)/((double)phiN);
				if(ratio<nratio && isPerm(a.ToString(), phiN.ToString()))
				{
					n = a;
					nratio = ratio;
					Console.WriteLine(n);
				}
			}
			Console.WriteLine("Done!");
		}
		
		public static int phi(int n, int[] factors)
		{
			long num = n;
			long den = 1;
			foreach(int f in factors)
			{
				num*=f-1;
				den*=f;
			}
			return (int)(num/den);
		}
		
		public static bool isPerm(string a, string b)
		{
			if(a.Length!=b.Length)
				return false;
			char[] cA = a.ToCharArray();
			char[] cB = b.ToCharArray();
			Array.Sort(cA);
			Array.Sort(cB);
			return lib.ArrayEqual<char>(cA,cB);
		}
		
		private static bool isCoprime(int[] factorsA, int[] factorsB)
		{
			foreach(int a in factorsA)
			{
				if(0<=Array.BinarySearch<int>(factorsB,a))
					return false;
			}
			return true;
		}
	}
	
	class Euler069
	{
		public static void run()
		{
			int until = 1000000;			
			
			calcCache<int, int[]> c = new standardCalcCache<int, int[]>(new ccPF(new lib.InfiniteArrayOfPrimes(until)));
			
			int maxA = 0;
			float maxVal = 0.0f;
			
			for(int a = 2; a<=until; a++)
			{
				long aCount = 0;
				int[] factA = c.getVal(a);
				for(int b=1; b<a;b++)
				{
					int[] factB = c.getVal(b);
					if(isCoprime(factB,factA))
						aCount++;
				}
				float result = ((((float)a)/(float)(aCount)));
				if(result >maxVal)
				{
					maxA = a;
					maxVal = result;
				}
			}
			
			Console.WriteLine("Max Value given by: " + maxA.ToString() + "   Equal to: " + maxVal.ToString());
		}
		
		private class ccPF : ccObj<int, int[]>
		{
			private IList<int> iap;
			
			public ccPF(IList<int> iap)
			{
				this.iap = iap;
			}
			
			public int[] getVal(int x, calcCache<int, int[]> c)
			{
				return lib.PrimeFactors(x,iap,false);
			}
		}
		
		private static bool isCoprime(int[] factorsA, int[] factorsB)
		{
			foreach(int a in factorsA)
			{
				if(0<=Array.BinarySearch<int>(factorsB,a))
					return false;
			}
			return true;
		}
	}
	
	class Euler068
	{
		public static void run()
		{
			long giant = -1;
			Euler024.lexPerm lp = new Euler024.lexPerm(10);
			int[] p = lp.getNext(0);
			while(p!=null)
			{
				giant = Math.Max(isMagical(p),giant);
				p = lp.getNext(1);
			}
			Console.WriteLine(giant);
		}
				
		public static long isMagical(int[] ii)
		{
			if(ii[0]<ii[3] && ii[0]<ii[5] &&
			   ii[0]<ii[7] && ii[0]<ii[9])
			{
				int sum = ii[0] + ii[1] + ii[2];
				if( sum == (ii[3] +  ii[2] + ii[4]) &&
				    sum == (ii[6] +  ii[4] + ii[5]) &&
				    sum == (ii[8] +  ii[6] + ii[7]) &&
				    sum == (ii[1] +  ii[8] + ii[9]) )
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(ii[0]+1); sb.Append(ii[1]+1); sb.Append(ii[2]+1);
					sb.Append(ii[3]+1); sb.Append(ii[2]+1); sb.Append(ii[4]+1);
					sb.Append(ii[5]+1); sb.Append(ii[4]+1); sb.Append(ii[6]+1);
					sb.Append(ii[7]+1); sb.Append(ii[6]+1); sb.Append(ii[8]+1);
					sb.Append(ii[9]+1); sb.Append(ii[8]+1); sb.Append(ii[1]+1);
					if(sb.Length==16)
						return long.Parse(sb.ToString());
					else
						return -1;
				}
			}
			return -1;
		}		
		
	}
	
	class Euler066
	{
		public static HashSet<int> allSquaresBelow(int x)
		{
			HashSet<int> squares = new HashSet<int>();
			int sqr = 4;
			int i = 2;
			while(sqr<x)
			{
				squares.Add(sqr);
				i++;
				sqr = i*i;
			}
			return squares;
		}
		
		private static BigInt one;
		private static HashSet<int> squares;
		
		public static void run()
		{
			wrapper[] results = new wrapper[1000];
			for(int i=0;i<results.Length; i++)
				results[i] = new wrapper(i);
						
			System.Threading.Thread[] threads = new System.Threading.Thread[1000];
			for(int i=0; i<threads.Length; i++)
			{				
				threads[i] = new System.Threading.Thread(new System.Threading.ThreadStart(results[i].CalcX));
			}
			
			
			int thread1 = 0;
			int thread2 = 1;
			threads[thread1].Start();
			threads[thread2].Start();
			
			while(notDone(results))
			{
				System.Threading.Thread.Sleep(5000);
				threads[thread1].Suspend();
				threads[thread1].Suspend();
				
			}
		}
		
		private static bool notDone(wrapper[] results)
		{
			foreach(wrapper l in results)
				if(l.result==-1)
					return true;
			return false;
		}
		
//		public static void run1()
//		{
//			squares = allSquaresBelow(1005);
//			one = new BigInt(1);
//			
//			int maxXD = 0;						
//			long maxX = 0;
//			
//			int Dceiling = 981;
//			int Dfloor = 188;
//			
//			while(Dceiling>=Dfloor)
//			{
//				long c = CalcX(Dceiling);
//				long f = CalcX(Dfloor);
//				
//				int D = 0;
//				long x = 0;
//				
//				if(c>f)
//				{
//					D = Dceiling;
//					x = c;
//				}
//				else
//				{
//					D = Dfloor;
//					x = f;
//				}
//				
//				if(x>maxX)
//				{
//					maxX = x;
//					maxXD = D;
//					Console.WriteLine(maxXD.ToString() + " : " + maxX.ToString());
//				}
//				
//				Dceiling--;
//				Dfloor++;
//			}
//			Console.WriteLine("Answer: " + maxXD.ToString());
//		}
		
		[Serializable]
		private class wrapper
		{
			int D;
			public long result;
			lib.SqrGen x;
			lib.SqrGen y;			
			
			public wrapper(int D)
			{
				lib.SqrGen x = new lib.SqrGen(0);
				lib.SqrGen y = new lib.SqrGen(1);
				this.D = D;
				if(D==0 || D==1 || squares.Contains(D))
					result = 0;
				else
					result = -1;
			}
			
			public void CalcX()
			{
				if(result==-1)
				{					
					BigInt calc = new BigInt(0);
					while(calc!=one)
					{
						if(calc<one)
							x.getNext();
						else
							y.getNext();
						calc = (x.currValue) - ((y.currValue)*D);
					}
					result = x.currIndex;
				}
			}
		}
	}	
	
	class Euler065
	{
		public static void run()
		{
			//Console.WriteLine(ith(10));
			Console.WriteLine(ith(100));			
		}
		
		public static long ith(int num)
		{
			int count = 1;
			List<int> lr = new List<int>();
			while(count<num)
			{
				lr.Add(1); count++;
				if(count<num)
				{
					int b = ((count/3)+1)*2;
					lr.Add(b); count++;
				}
				if(count<num)
				{
					lr.Add(1); count++;
				}
			}
			Euler057.Fraction f = new Euler057.Fraction(0,1);
			lr.Reverse();
			foreach(int i in lr)
			{
				f.SumTo(i);
				f.Invert();
			}
			f.SumTo(2);
			//return long.Parse(f.numerator.ToString());
			return lib.DigitSum(f.numerator.ToString());
		}
	}
	
	class Euler063
	{
		public static void run()
		{
			qManager q = new qManager();
			long count = 0;
			while(true)
			{
				duple d = q.getNext();
				zint z = new zint(d.b);
				z.power(d.a);
				if(z.ToString().Length==d.a)
				{
					Console.WriteLine(
						(++count).ToString() + ": " + 
						d.b.ToString() + "^" + d.a.ToString() + 
						" = " + z.ToString());
				}
			}
		}
		
		public class qManager
		{
			int total = 1;
			Queue<duple> q;
			public qManager()
			{
				 q = new Queue<duple>();
			}
			public qManager(int total)
			{
				 this.total = total;
				 q = new Queue<duple>();
			}
			
			public duple getNext()
			{
				while(q.Count==0)
					genMore();
				return q.Dequeue();
			}
			
			private void genMore()
			{
				total++;
				for(int a = 1; a<=total; a++)
				{
					int b = total-a;
					q.Enqueue(new duple(a,b));
				}
			}
		}
	
		public struct duple
		{
			public int a;
			public int b;
			
			public duple(int a, int b)
			{
				this.a = a;
				this.b = b;
			}
			
			public override string ToString()
			{
				return "(A,B): (" + a.ToString() + "," + b.ToString() + ")";
			}
		}
			
	}
	
	class Euler062
	{
		public static void run()
		{
			foreach(int[] iA in search())
			{
				foreach(int i in iA)
				{
					Console.WriteLine(i.ToString() + ",");
				}
				Console.WriteLine();
			}
		}
		
		private static List<int[]> search()
		{
			List<int[]> results = new List<int[]>();
			int num = 5;
			Dictionary<string, List<int>> superCache = new Dictionary<string, List<int>>();
			lib.nGonalInt<long> g = new ScratchPad1.lib.nGonalInt<long>(new cubeNgonal());
			
			int maxDigits = int.MaxValue;
			int n = 1;
			for(char[] cC = (g.getAtI(n)).ToString().ToCharArray();
			    	cC.Length<=maxDigits;
			    	cC= (g.getAtI(++n)).ToString().ToCharArray())
			{
				Array.Sort(cC);
				string key = new string(cC);
				
				List<int> pants;
				if(!superCache.TryGetValue(key, out pants))
				{
					pants = new List<int>();
				}
				pants.Add(n);
				superCache[key] = pants;
				
				if(pants.Count == num)
				{
					if(results.Count==0)
						maxDigits = key.Length;
					results.Add(pants.ToArray());
				}
			}
			return results;
		}
		
		public class cubeNgonal : lib.ngonalIntObject<long>
		{			
			public long gen(int x)
			{
				long b = (long) x;
				return b*b*b;
			}
		}
	}
	
	class Euler061
	{		
		public static void run1()
		{
			Console.WriteLine(GenTest((f=>((f*(f+1))/2))));
			Console.WriteLine(GenTest((f=>(f*f))));
			Console.WriteLine(GenTest((f=>((f*(3*f-1))/2))));
			Console.WriteLine(GenTest((f=>((f*(2*f-1))))));
			Console.WriteLine(GenTest((f=>((f*(5*f-3))/2))));
			Console.WriteLine(GenTest((f=>((f*(3*f-2))))));
		} 
		
		public static string GenTest(Func<int, int> f)
		{
			StringBuilder sb = new StringBuilder();
			for(int i=1; i<6; i++)
			{
				sb.Append(f(i));
				sb.Append(',');
			}
			return sb.ToString();
		}
		
		public static void run()
		{
			List<pear>[] dB = new List<pear>[100];
			for (int i=0; i<dB.Length; i++ )
			{
				dB[i] = new List<pear>();
			}
			
			Gen4digits((f=>((f*(f+1))/2)),0,dB);
			Gen4digits((f=>(f*f)),1,dB);
			Gen4digits((f=>((f*(3*f-1))/2)),2,dB);
			Gen4digits((f=>((f*(2*f-1)))),3,dB);
			Gen4digits((f=>((f*(5*f-3))/2)),4,dB);
			List<pear> oct = Gen4digits((f=>((f*(3*f-2)))),5);
			
			foreach(pear o in oct)
			{
				pear[] p = new pear[6];
				p[0] = o;
				recursor(0, p, new bool[]{true,true,true,true,true}, dB);
			}
		}
		
		public static void recursor(int i, pear[] currSet, bool[] poly, List<pear>[] dB)
		{
			if(i==5 && currSet[0].front==currSet[5].back)
			{
				int tot = 0;
				foreach(pear p in currSet)
				{
					tot+= p.front*100 + p.back;
				}
				Console.WriteLine(tot);
			}
			else
			{
				List<pear> possibs = dB[currSet[i].back];
				i++;
				foreach(pear p in possibs)
				{
					if(poly[p.polygon]) //is possible
					{
						pear[] newSet = new pear[6];
						Array.Copy(currSet,newSet,i);
						newSet[i] = p;
						
						bool[] newPoly = new bool[5];
						Array.Copy(poly,newPoly,5);
						newPoly[p.polygon] = false;
						
						recursor(i, newSet, newPoly, dB);
					}
				}
			}
		}
		
		
		public static List<pear> Gen4digits(Func<int, int> f, int polygon)
		{
			List<pear> list = new List<pear>();
			
			int i = 1;
			int res = f(i);
			while(1000>res)
				res = f(++i);
			
			while(res<10000)
			{
				int front = res/100;
				int back = res-(100*front);
				list.Add(new pear(front,back,polygon));
				res = f(++i);
			}
			
			return list;
		}
		
		public static void Gen4digits(Func<int, int> f, int polygon, List<pear>[] dB)
		{
			int i = 1;
			int res = f(i);
			while(1000>res)
				res = f(++i);
			
			while(res<10000)
			{
				int front = res/100;
				int back = res-(100*front);
				dB[front].Add(new pear(front,back,polygon));
				res = f(++i);
			}
		}
		
		public class pear
		{
			public int front;
			public int back;
			public int polygon;
			
			public pear (int front, int back, int polygon)
			{
				this.front = front;
				this.back = back;
				this.polygon = polygon;
			}
		}
	}
	
	class Euler060
	{
		static IList<int> iap;
		static calcCache<duple, bool> cc;
		
		public static void run1()
		{
			iap = new ScratchPad1.lib.InfiniteArrayOfPrimes2(int.MaxValue/20000);//1000000000);
			cc = new standardCalcCache<duple, bool>(new checker());
			Set.setCache(cc);
			Set s = new Set(3, "3");
			s = new Set(s,7, "7");
			Console.WriteLine(s.worksWith("109"));
		}
		
		public static void run()
		{
			iap = lib.fileCache<lib.InfiniteArrayOfPrimes2>("Primes", (()=>(new ScratchPad1.lib.InfiniteArrayOfPrimes2(int.MaxValue/2))));
			//iap = new ScratchPad1.lib.InfiniteArrayOfPrimes2(int.MaxValue/2);//1000000000);
			cc = new standardCalcCache<duple, bool>(new checker());
			Set.setCache(cc);
			int ClusterSize = 5;
			List<Set> masterList = new List<Set>();
			bool contin = true;
			for(int i=0; contin; i++)
			{
				int prime = iap[i];
				string primeS = prime.ToString();
				int masterListLength = masterList.Count;
				for(int s=0; s<masterListLength; s++)
				{
					Set oS = masterList[s];
					if(oS.worksWith(primeS))
					{
						Set newS = new Set(oS,prime, primeS);
						masterList.Add(newS);
						if(newS.Count == ClusterSize)
							Console.WriteLine(newS);
					}
				}
				masterList.Add(new Set(prime, primeS));
				//masterList.Sort();
				cc.clearCache();
			}
		}
		
		public class Set : IComparable<Set>
		{
			int[] a;
			string[] aS;
			static calcCache<duple, bool> ccache;
			
			public int Count
			{
				get{return a.Length;}
			}
			
			public static void setCache(calcCache<duple, bool> cc)
			{
				ccache = cc;
			}
			
			public Set(int i, string iS)
			{
				a = new int[]{i};
				aS = new string[]{iS};				
			}
			
			public Set(Set b, int i, string iS)
			{
				int le = b.a.Length;
				a = new int[le+1];
				aS = new string[le+1];
				Array.Copy(b.a,a,le);
				Array.Copy(b.aS,aS,le);
				a[le] = i;
				aS[le] = iS;
			}
			
			public bool worksWith(string iS)
			{
				foreach(string s in aS)
				{
					if(!ccache.getVal(new duple(iS, s)))
						return false;
				}
				return true;
			}
			
			public int CompareTo(Set other)
			{
				return a.Length-other.a.Length;
			}
			
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(" : ");
				int total = 0;
				foreach(int i in a)
				{
					sb.Append(i);
					sb.Append(", ");
					total+= i;
				}
				sb.Insert(0, total);
				return sb.ToString();
			}
		}
		
		public class checker : ccObj<duple,bool>
		{			
			public bool getVal(duple d, calcCache<duple, bool> c)
			{
				string a = d.a;
				string b = d.b;
				if(iap.Contains(int.Parse(a+b)) && iap.Contains(int.Parse(b+a)))
					return true;
				else
					return false;
			}
		}
		
		public struct duple : IComparable<duple>
		{
			public string a;
			public string b;
			
			public duple(string a, string b)
			{
				this.a = a;
				this.b = b;
			}
			
			public override string ToString()
			{
				return "(A,B): (" + a + "," + b + ")";
			}
			
			public override bool Equals(object obj)
			{
				duple oth = (duple)obj;
				return ((a.Equals(oth.a)) && (b.Equals(oth.b)));
			}
			
			public override int GetHashCode()
			{
				return a.GetHashCode()^b.GetHashCode();
			}
			
			public int CompareTo(duple other)
			{
				return 0;
			}
		}
	}
	
	class Euler059
	{		
		static int passLength = 3;
		static string[] commonPairs = new string[]{"th","he","an","re","er","in","on","at","nd","st","es","en"};
		static int numCharTries = 3;
		
		static byte minPassChar = 97;
		static byte maxPassCharPlus1 = 123;
		
		private static List<passwordByte>[] initQubyte()
		{			
			List<passwordByte>[] qubyte = new List<passwordByte>[passLength];
			for(int i=0; i<qubyte.Length; i++)
			{
				qubyte[i] = new List<passwordByte>();
				for(byte b = minPassChar; b<maxPassCharPlus1; b++)
					qubyte[i].Add(new passwordByte(b));
			}
			return qubyte;
		}
		
		private static List<passwordTry> initQupass(List<byte[]> possiblePasswords, byte[] encrypted)
		{
			passwordTry.setParameters(commonPairs);
			List<passwordTry> passTries = possiblePasswords.ConvertAll<passwordTry>((f=> new passwordTry(f)));
			for(int i=0; i<encrypted.Length; i++)
			{
				byte encryptedByte = encrypted[i];
				
				foreach(passwordTry pt in passTries)
				{
					pt.Add(encryptedByte);
				}
			}
			passTries.Sort();
			return passTries;
		}
		
		private static void fillQubyte(List<passwordByte>[] qubyte, byte[] encrypted)
		{
			int passCharIndex = 0;
			for(int i=0; i<encrypted.Length; i++)
			{
				byte encryptedByte = encrypted[i];
				
				foreach(passwordByte pb in qubyte[passCharIndex])
				{
					pb.Add(encryptedByte);
				}
				passCharIndex++;
				passCharIndex%=passLength;
			}
			foreach(List<passwordByte> list in qubyte)
			{
				list.Sort();
				list.Reverse();
			}
		}
		
		private static List<byte[]> getPossiblePasswords(List<passwordByte>[] qubyte,int numTriesPerChar)
		{
			List<byte[]> possPass = new List<byte[]>();
			
			foreach(int[] perm in Euler093.Dice.allPossibleRolls(qubyte.Length,numTriesPerChar))
			{
				var pass = new byte[perm.Length];
				for(int i=0; i<perm.Length; i++)
				{
					pass[i] = qubyte[i][perm[i]-1].getPassByte();
				}
				possPass.Add(pass);
			}
			return possPass;
		}
		
		public static void run()
		{
			byte[] encrypted = load();
			List<passwordByte>[] qubyte = initQubyte();
			fillQubyte(qubyte,encrypted);
						
			List<byte[]> possiblePasswords = getPossiblePasswords(qubyte, numCharTries);
			List<passwordTry> passTries = initQupass(possiblePasswords,encrypted);
			for(int i=0; i<passTries.Count; i++)
			{
				Console.WriteLine(passTries[i]);
			}
			long fin = 0;
			foreach(byte b in passTries[passTries.Count-1].decrypt(encrypted))
				fin += b;
			Console.WriteLine(fin);
		}
				
		private static byte[] load()
		{
			List<byte> cypherText = new List<byte>();
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\cipher1.txt");
			foreach(string s in lines)
			{
				string[] bs = s.Split(new char[]{','});
				for(int i=0; i<bs.Length; i++)
				{
					cypherText.Add(byte.Parse(bs[i]));
				}
			}
			return cypherText.ToArray();
		}
		
		private class passwordByte : IComparable<passwordByte>
		{
			byte passb;
			
			int score;
			
			public byte getPassByte()
			{
				return passb;
			}
			
			public passwordByte(byte passByte)
			{
				this.passb = passByte;
				score = -1;
			}
			
			public void Add(byte cryptedByte)
			{
				int res = passb^cryptedByte;
								
				if(res<97 || res>122)
					score -= 1;
				else
					score += 1;
			}
			
			public int getScore()
			{
				return score;
			}
			
			public int CompareTo(passwordByte other)
			{
				return this.getScore() - other.getScore();
			}
		}
		
		private class passwordTry : IComparable<passwordTry>
		{
			static bool[,] pants;
			byte[] password;
			int passIndex;
			int score;
			int prevByte;
			List<char> previewBytes;
			
			public static void setParameters(string[] commonPairs)
			{
				pants = (bool[,])Array.CreateInstance(typeof(bool),256,256);
				foreach(string ss in commonPairs)
				{
					pants[(int)ss[0],(int)ss[1]] = true;
				}
			}
			
			public passwordTry(byte[] password)
			{
				this.password = password;
				passIndex = 0;
				prevByte = -1;
				previewBytes = new List<char>();
			}
			
			public byte[] decrypt(byte[] crypted)
			{
				int currIndex = 0;
				byte[] decrypted = new byte[crypted.Length];
				for(int i=0; i<crypted.Length; i++)
				{
					decrypted[i] = (byte)(password[currIndex]^crypted[i]);
					currIndex++;
					currIndex%=password.Length;
				}
				return decrypted;
			}
			
			public void Add(byte cryptedByte)
			{
				int currByte = password[passIndex]^cryptedByte;
				if(currByte>64 && currByte<91)	//is uppercase?
					currByte+=32;			//then lowercase it!
				
				if(previewBytes.Count<16)
					previewBytes.Add((char)currByte);
				if(prevByte!=-1 && pants[prevByte, currByte])
				{
					score++;
				}
				prevByte = currByte;
				passIndex++;
				passIndex%=password.Length;
			}
			
			public int getScore()
			{
				return score;
			}
						
			public int CompareTo(passwordTry other)
			{
				return this.getScore() - other.getScore();
			}
			
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(Array.ConvertAll<byte,char>(password, (f=>(char)f)));
				sb.Append(':');
				sb.Append(previewBytes.ToArray());
				return sb.ToString();
			}
		}
		
		private static byte[] xorEncrypt(string data, string password)
		{
			byte[] passBytes = Array.ConvertAll(password.ToCharArray(),(f=>(((byte)f))));
			byte[] dataBytes = Array.ConvertAll(data.ToCharArray(),(f=>(((byte)f))));
			int currIndex = 0;
			for(int i=0; i<dataBytes.Length; i++)
			{
				dataBytes[i] = (byte)(password[currIndex]^dataBytes[i]);
				currIndex++;
				currIndex%=password.Length;
			}
			return dataBytes;
		}
		
		private static string xorDecrypt(byte[] crypted, string password)
		{
			byte[] passBytes = Array.ConvertAll(password.ToCharArray(),(f=>(((byte)f))));
			int currIndex = 0;
			char[] decrypted = new char[crypted.Length];
			for(int i=0; i<crypted.Length; i++)
			{
				decrypted[i] = (char)(password[currIndex]^crypted[i]);
				currIndex++;
				currIndex%=password.Length;
			}
			return new string(decrypted);
		}
	}
	
	class Euler058
	{
		public static void run()
		{
			Console.WriteLine(search());
		}
		
		private static double search()
		{
			long sumTotal = 1;
			long sumPrime = 0;
			
			lib.InfiniteArrayOfPrimes iap = new ScratchPad1.lib.InfiniteArrayOfPrimes(1000000000);
			
			int offset = 2;
			int curr = 1;
			
			while(true)
			{
				for(int i=0; i<4;i++)
				{
					curr+=offset;
					sumTotal++;
					if(iap.Contains(curr))
						sumPrime++;
				}
				offset+=2;
				if(((100*sumPrime)/sumTotal)<10)
					return Math.Sqrt((double)curr);
			}
		}
	}
	
	class Euler057
	{
		public static void run()
		{
			Fraction start = new Fraction(3,2);
			int count = 0;
			for(int i=2; i<=1000; i++)
			{
				start.SumTo(1);
				start.Invert();
				start.SumTo(1);
				if(start.numerator.ToString().Length>start.denominator.ToString().Length)
				{
					Console.WriteLine(++count);
				}
			}
			Console.WriteLine("Final Answer: " + count.ToString());
		}
		
		public class Fraction
		{
			public zint numerator;
			public zint denominator;
			
			public Fraction(zint numerator, zint denominator)
			{
				this.numerator = numerator;
				this.denominator = denominator;
			}
			
			public Fraction(int numerator, int denominator)
			{
				this.numerator = new zint(numerator);
				this.denominator = new zint(denominator);
			}
			
			public void SumTo(int x)
			{
				for(int i=0; i<x;i++)
					numerator.addTo(denominator);
			}
			
			public void Invert()
			{
				zint num = denominator;
				denominator = numerator;
				numerator = num;
			}
			
			public Fraction Duplicate()
			{
				return new Fraction(numerator.duplicate(),denominator.duplicate());
			}
			
			public override string ToString()
			{
				return numerator.ToString() + "/" + denominator.ToString();
			}
		}
	}
	
	class Euler056
	{
		public static void run()
		{
			long maxSum=0;
			for(int a=1;a<=100;a++)
			{
				zint aa = new zint(a);
				for(int b=1;b<=100;b++)
				{
					zint tot = aa.duplicate();
					tot.power(b);
					long v = lib.DigitSum(tot.ToString());
					if(v>maxSum)
					{
						maxSum = v;
						Console.WriteLine("A: " + a.ToString() + " B: " + b.ToString() + " = DigitSum: " + v.ToString());
					}
				}
			}
		}
	}
	
	class Euler055
	{
		public static void run()
		{
			int count=0;
			for(int i=1;i<10000;i++)
				if(isLychrel(i))
					Console.WriteLine((++count).ToString() + " : " + i);
			Console.WriteLine("Final Answer: " + count.ToString());
		}
		
		public static bool isLychrel(int x)
		{
			zint v = revAdd(new zint(x));
			for(int i=0;i<51;i++)
			{
				if(lib.isPalindrome(v.ToString()))
					return false;
				else
					v = revAdd(v); 
			}
			return true;
		}
		
		public static zint revAdd(zint x)
		{
			zint retVal = x.duplicate();
			char[] cA = x.ToString().ToCharArray();
			Array.Reverse(cA);
			retVal.addTo(zint.Parse(new string(cA)));
			return retVal;
		}
	}
	
	class Euler054
	{
		public static void run1()
		{
			string[] test = new string[]{
			"5H 5C 6S 7S KD 2C 3S 8S 8D TD",
			"5D 8C 9S JS AC 2C 5C 7D 8S QH",
			"2D 9C AS AH AC 3D 6D 7D TD QD",
			"4D 6S 9H QH QC 3D 6D 7H QD QS",
			"2H 2D 4C 4D 4S 3C 3D 3S 9S 9D"			
			};
			foreach(string line in test)
			{
				string[] handStrings = line.Split(new char[]{' '});
				card[] cards = Array.ConvertAll<string,card>(handStrings,(f => new card(f)));
				pokerhand player1 = new pokerhand(cards,0);
				pokerhand player2 = new pokerhand(cards,5);
				if(player1>player2)
					Console.WriteLine("Player 1 wins!");
				else
					Console.WriteLine("Player 2 wins!");
			}
		}
		
		public static void run()
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\poker.txt");
			int win1 = 0;
			foreach(string line in lines)
			{
				string[] handStrings = line.Split(new char[]{' '});
				card[] cards = Array.ConvertAll<string,card>(handStrings,(f => new card(f)));
				pokerhand player1 = new pokerhand(cards,0);
				pokerhand player2 = new pokerhand(cards,5);
				if(player1>player2)
					win1++;
			}
			Console.WriteLine(win1);
		}
					
		public class pokerhand
		{
			card[] cards;
			//first int is the handtype, second is facevalue of the highest card in the handtype, the others are facevalues sorted high-to-low (to break ties)
			int[] val;
			
			public pokerhand(card[] cards)
			{
				this.cards = cards;
				eval();
			}
			
			public pokerhand(card[] cards, int index)
			{
				this.cards = new card[5];
				for(int i=0; i<5; i++)
				{
					this.cards[i] = cards[index+i];
				}
				eval();
			}
			
			#region evaluate the hand
			private void eval()
			{
				fillInSortedCardValues();
				if(allSameSuit())
				{
					if(isConsecutive())
					{
						if(val[2]==14)
							val[0] = 10; //Royal Flush
						else
							val[0] = 9; //Straight Flush
					}
					else
					{
						val[0] = 6; //Flush
					}
				}
				else
				{
					if(isConsecutive())
					{
						val[0] = 5; //Straight
					}
					else
					{
						int[] count = incidenceCount(val, 2,5,14);
						int[] numSets = incidenceCount(count,0,count.Length,4);
						if(numSets[4]==1)
						{
							val[0] = 8; //Four Of A Kind
							val[1] = val[4]; //of rank val[4]
						}
						else if(numSets[3]==1)
						{
							if(numSets[2]==1)
							{
								val[0] = 7; //Full House
								val[1] = 0;
								for(int i=0; i<=14; i++) //rank = triplet*100 + pair
								{
									if(count[i]==3)
										val[1]+=i*100;
									else if(count[i]==2)
										val[1]+=i;
								}
							}
							else
							{
								val[0] = 4; //Three Of A Kind
								val[1] = val[4]; //of rank val[4]
							}
						}
						else if(numSets[2]==2)
						{
							val[0] = 3; //Two Pair
							int lowpair = 30;
							int highpair = 0;
							for(int i=0; i<=14; i++) //rank = highpair*100 + lowpair
							{
								if(count[i]==2)
								{
									if(i>highpair)
										highpair = i;
									if(i<lowpair)
										lowpair = i;
								}
							}
							val[1] = 100*highpair + lowpair;
						}
						else if(numSets[2]==1)
						{
							val[0] = 2; //One Pair
							for(int i=0; i<=14; i++) //rank = facevalue of pair
							{
								if(count[i]==2)
									val[1]=i;
							}
						}
					}
				}
			}
			
			private static int[] incidenceCount(int[] iA, int index, int length, int max)
			{
				int[] res = new int[max+1];
				for(int i=0; i<length; i++)
				{
					res[iA[index+i]]++;
				}
				return res;
			}
						
			private void fillInSortedCardValues()
			{
				List<int> retVal = new List<int>();
				for(int i=0; i<5; i++)
					retVal.Add(cards[i].faceValue());
				retVal.Sort();
				retVal.Reverse();
				retVal.Insert(0,0);
				retVal.Insert(0,0);
				val = retVal.ToArray();
			}
			
			private bool allSameSuit()
			{
				int suit = cards[0].suit();
				for(int i=1; i<5; i++)
					if(suit!=cards[i].suit())
						return false;
				return true;
			}
			
			private bool isConsecutive()
			{
				for(int i=3; i<val.Length; i++)
				{
					if(val[i-1]!=val[i]+1)
						return false;
				}
				return true;
			}
			#endregion
			
			#region compare pokerhands based on precalculated val
			public int CompareTo(pokerhand other)
			{
				int i = 0;
				while(i<cards.Length && val[i]==other.val[i])
					i++;
				return val[i].CompareTo(other.val[i]);
			}
			
			public static bool operator > (pokerhand a, pokerhand b)
			{
				return a.CompareTo(b)>0;
			}
			
			public static bool operator < (pokerhand a, pokerhand b)
			{
				return  a.CompareTo(b)<0;
			}
			#endregion
		}
		
		public class card
		{
			int cardsuit;
			int val;
			
			public static card Parse(string s)
			{
				return new card(s);
			}
			
			public card(string s)
			{
				switch (s[0]) {
						case 'T':
						val = 10;
						break;
						case 'J':
						val = 11;
						break;
						case 'Q':
						val = 12;
						break;
						case 'K':
						val = 13;
						break;
						case 'A':
						val = 14;
						break;
						default:
						val = ((int)(s[0]))-48;
						break;
				}
				switch (s[1]) {
						case 'H':
						cardsuit = 1;
						break;
						case 'C':
						cardsuit = 2;
						break;
						case 'S':
						cardsuit = 3;
						break;
						default:
						cardsuit = 4;
						break;
				}
			}
			
			public int faceValue()
			{
				return val;
			}
			
			public int suit()
			{
				return cardsuit;
			}
		}
	}
	
	class Euler053
	{
		public static void run()
		{
			long count = 0;
			for(int n=1; n<=100;n++)
			{
				for(int r=1; r<=n; r++)
				{
					long v = (nCr(n,r));
					if(v<0 || v>1000000)
						count++;
				}
			}
			Console.WriteLine(count);
		}
		
		public static long nCr(int n, int r)
		{
			excla numer = new excla(n);
			excla denom1 = new excla(r);
			excla denom2 = new excla(n-r);
			numer.DivideBy(denom1);
			numer.DivideBy(denom2);
			return numer.eval();
		}
		
		public class excla
		{
			List<int> factors;
			public excla(int x)
			{
				factors = new List<int>();
				for(int i=x; i>0; i--)
					factors.Add(i);
			}
			
			public long eval()
			{
				long t = 1;
				foreach(int i in factors)
				{
					try
					{t*=i;}
					catch
					{return -1;}
				}
				return t;
			}
			
			public void DivideBy(excla e)
			{
				lib.InfiniteArrayOfPrimes iap = new ScratchPad1.lib.InfiniteArrayOfPrimes(101);
				List<int> eF = new List<int>();
				
				foreach(int ei in e.factors)
				{
					int[] pfs = lib.PrimeFactors(ei,iap,true);
					eF.AddRange(pfs);
				}
				
				foreach(int ei in eF)
				{
					for(int i=0; i<factors.Count;i++)
					{
						if(factors[i]%ei==0)
						{
							factors[i] = factors[i]/ei;
							break;
						}
					}
				}
			}
		}
	}	
	
	class Euler052
	{
		static int seriesNum = 6;
		public static void run()
		{
			int last = 1;
			for(int i=1; !isIt(i); i++)
				last = i;
			Console.WriteLine(last+1);
		}
		
		public static bool isIt(int x)
		{
			int[][] d = new int[seriesNum][];
			for(int i=0;i<seriesNum; i++)
				d[i] = lib.digitCounts(((i+1)*x));
			
			int[] start = d[0];
			for(int i=1;i<seriesNum;i++)
				if(!lib.ArrayEqual<int>(start,d[i]))
					return false;
			
			return true;
		}
	}
	
	class Euler051
	{
		public static void run()
		{
			//test(120383);
			//Console.WriteLine(search(6));
			//Console.WriteLine(search(7));
			Console.WriteLine(search(8));
		}
		
		public static int search(int numPrimesFamily)
		{
			lib.InfiniteArrayOfPrimes iap = new ScratchPad1.lib.InfiniteArrayOfPrimes(1000000);
			calcCache<string, int> cc = new standardCalcCache<string, int>(new ccO(iap));
			for(int ix = 11330; true; ix++)	//get the prime
			{
				int x = iap[ix];
				string xS = x.ToString();
				List<string> possibles = genPossibles(xS, 'X', 0);	//list all possible replacement sites.
				foreach(string pos in possibles)
				{
					if(cc.getVal(pos)==numPrimesFamily)
					{
						int zz = 0;
						if(pos.StartsWith("X"))
							zz = int.Parse(pos.Replace('X', (char)(49))); //if start with X, replace with 1
						else
							zz = int.Parse(pos.Replace('X', (char)(48))); //if no start with X, replace with 0
						Console.WriteLine(pos);
						return zz;
					}
				}
			}
		}
		
		public static void test(int x)
		{
			lib.InfiniteArrayOfPrimes iap = new ScratchPad1.lib.InfiniteArrayOfPrimes(1000000);
			calcCache<string, int> cc = new standardCalcCache<string, int>(new ccO(iap));
			string final = "";
				string xS = x.ToString();
				List<string> possibles = genPossibles(xS, 'X', 0);	//list all possible replacement sites.
				foreach(string pos in possibles)
				{
					if(cc.getVal(pos)==8)
					{
						Console.WriteLine(pos);
						final = pos;
					}
				}
				
				string s = final;
				for(int i=0;i<10;i++)
				{
					int zz = int.Parse(s.Replace('X', (char)(i+48)));
					if(s[0]!='X' || i!=0)					
						if(iap.Contains(zz))
							Console.WriteLine(zz);
				}
		}
		
		private class ccO : ccObj<string, int>
		{
			IList<int> iap;
			public ccO(IList<int> iap)
			{
				this.iap = iap;
			}
			public int getVal(string x, calcCache<string, int> c)
			{
				return numPrimes(x, iap);
			}
		}
		
		private static int numPrimes(string s, IList<int> iap)
		{
			int count = 0;
			for(int i=0;i<10;i++)
				{
					int zz = int.Parse(s.Replace('X', (char)(i+48)));
					if(s[0]!='X' || i!=0)					
						if(iap.Contains(zz))
							count++;
				}
			return count;
		}
		
		public static List<string> genPossibles(string departure, char replacement, int start)
		{
			List<string> retVal=null;
			if(start==departure.Length)
			{
				retVal = new List<string>(new string[]{departure});
			}
			else
			{
				char[] ds = departure.ToCharArray();
				ds[start] = replacement;
				retVal = genPossibles(new string(ds),replacement,start+1);
				
				List<string> rr = genPossibles(departure,replacement,start+1);
				retVal.AddRange(rr);
			}
			return retVal;
		}
	}
	
	class Euler050
	{
		public static void run()
		{
			int allBelow = 1000000;
			int sumPrime = 5;  //5 = 2+3
			int chainCount = 2;
			int[] primes = lib.allPrimesBelow(allBelow);
			for(int i=0; (primes[i])*chainCount <allBelow; i++) //if (currentPrime*longestChain)>=allBelow, no longer chains are possible (that == a prime <allBelow)
			{
				int currSum = primes[i] + primes[i+1];
				for(int y=i+1;currSum<allBelow; currSum+= primes[++y])
				{
					int currChain = (1+y)-i;
					if(  chainCount<currChain   &&   0<=Array.BinarySearch<int>(primes,currSum))
					{
						sumPrime = currSum;
						chainCount = currChain;
						Console.WriteLine(sumPrime.ToString() + " = " + primes[i].ToString() + " + ...(" + chainCount.ToString() + ")" );
					}
				}
			}
			Console.WriteLine("Final Answer: " + sumPrime.ToString());	
		}
	}
	
	class Euler049
	{
		public static void run()
		{
			Console.WriteLine(search());
		}
		
		public static string search()
		{
			int[] primes = lib.allPrimesBelow(10000);
			for(int ix = 0 ; ix<primes.Length; ix++)
			{
				while(primes[ix]<1000 || primes[ix]==1487) //have to make an exception for the example ^_^"
					ix++;
				
				for(int iy = ix+1; iy<primes.Length; iy++)
				{
					int x = primes[ix]; int y = primes[iy];
					
					if(lib.haveSameDigits(x,y))
					{
						int diff = y-x;
						int z = y+diff;
						if(lib.haveSameDigits(y,z) && 0<Array.BinarySearch<int>(primes,z))
							return x.ToString() + y.ToString() + z.ToString();
					}
				}
			}
			return "Not Found";
		}
	}
	
	class Euler048
	{
		//Yea its horribly inefficient, plus it calculates the entire number instead of just the last 10 digits.
		//But I was going for "fastest implementation". So, just work on other Euler problems while this runs ^_^".
		//ADENDUM: I eventually made a limitedzint, but never tried nor care to try it with this problem.
		//More ADENDUM: So, I made zint fast enough that it dont matter no more. NVMD.
		public static void run()
		{
			zint zTot = new zint(1);
			for(int i=2; i<=1000; i++)
			{
				zint t = new zint(i);
				t.power(i);
				zTot.addTo(t);
			}
			Console.WriteLine(zTot);
		}
	}
	
	class Euler047
	{
		public static void run()
		{
			int count = 0;
			int lastVal = 0;
			
			int rr = 4;
			
			lib.InfiniteArrayOfPrimes iap = new ScratchPad1.lib.InfiniteArrayOfPrimes(50000);
			
			for(int i=2; count<rr; i++)
			{
				lastVal = i;
				if(lib.PrimeFactors(i,iap,false).Length==rr)
					count++;
				else
					count = 0;
			}
			Console.WriteLine(lastVal-(rr-1));
		}
	}
	
	class Euler046
	{
		public static void run()
		{
			lib.nGonalInt<long> prim = new ScratchPad1.lib.nGonalInt<long>(new prime());
			lib.nGonalInt<long> ds = new ScratchPad1.lib.nGonalInt<long>(new doublesquare());
			lib.nGonalInt<long> oc = new ScratchPad1.lib.nGonalInt<long>(new oddcomp(prim));
			
			bool keepGoing = true;
			long ocI = 0;
			for(int i=1; keepGoing; i++)
			{
				ocI = oc.getAtI(i);
				keepGoing = testOC(ocI, prim, ds);
			}
			Console.WriteLine(ocI);
		}
		
		public static bool testOC(long ocI, lib.nGonalInt<long> prim, lib.nGonalInt<long> ds)
		{			
			long p = prim.getAtI(1);
			for(int pIndex=1; p<ocI;p = prim.getAtI(++pIndex))
			{
				if(ds.isN(ocI - p))
					return true;
			}
			return false;
		}
		
		public class oddcomp : lib.ngonalIntObject<long>
		{
			lib.nGonalInt<long> p;
			long curr;
			public oddcomp(lib.nGonalInt<long> Primes)
			{
				p = Primes;
				curr = 1;
			}
			
			public long gen(int x)
			{
				curr+=2;
				while(p.isN(curr))
					curr+=2;
				return curr;
			}
		}
		
		public class prime : lib.ngonalIntObject<long>
		{
			lib.PrimeFactory pf;
			public prime()
			{
				pf = new ScratchPad1.lib.PrimeFactory();
			}
			
			public long gen(int x)
			{	
				return  pf.getNext();
			}
		}
		
		public class doublesquare : lib.ngonalIntObject<long>
		{
			public long gen(int x)
			{
				long v = x;
				return ((long)2)*v*v;
			}
		}
	}
		
	class Euler045
	{
		public static void run()
		{
			lib.nGonalInt<long> t = new lib.nGonalInt<long>(new trig());
			lib.nGonalInt<long> p = new lib.nGonalInt<long>(new pent());
			lib.nGonalInt<long> h = new lib.nGonalInt<long>(new hex());
			
			for(int i=286; i>0; i++)
			{
				long v = t.getAtI(i);
				if(p.isN(v) && h.isN(v))
				{
					Console.WriteLine(v);
					i=-1;
				}
			}
		}
		
		public class trig : lib.ngonalIntObject<long>
		{
			public long gen(int x)
			{
				long n = (long)x;
				long v = 0;
				if(x%2==0)
					v = (n/2)*(n+1);
				else
					v = (n)*((n+1)/2);
				return v;
			}
		}
		
		public class pent : lib.ngonalIntObject<long>
		{
			public long gen(int x)
			{
				long n = (long)x;
				long v = 0;
				if(x%2==0)
					v = (n/2)*((3*n)-1);
				else
					v = (n)*(((3*n)-1)/2);
				return v;
			}
		}
		
		public class hex : lib.ngonalIntObject<long>
		{
			public long gen(int x)
			{
				long n = (long)x;
				long v = n*((2*n)-1);
				return v;
			}
		}
	}
	
	class Euler044
	{
		public static void run()
		{
			Euler045.pent sp = new Euler045.pent();
			specialNgonal p = new specialNgonal(sp);
			qManager q = new qManager();
			
			bool isDone = false;
			while(!isDone)
			{
				duple d=q.getNext();
				long j = sp.gen(d.a);
				long k = sp.gen(d.b);
				if(p.isN(k-j) && p.isN(k+j))
				{
					isDone = true;
					Console.WriteLine(k-j);
				}
			}		
		}
	
		public class qManager
		{
			int total = 1;
			Queue<duple> q;
			public qManager()
			{
				 q = new Queue<duple>();
			}
			
			public duple getNext()
			{
				while(q.Count==0)
					genMore();
				return q.Dequeue();
			}
			
			private void genMore()
			{
				total++;
				for(int a = 1; a<=total/2; a++)
				{
					int b = total-a;
					if(a!=b)
						q.Enqueue(new duple(a,b));
				}
			}
		}
	
		public struct duple
		{
			public int a;
			public int b;
			
			public duple(int a, int b)
			{
				this.a = a;
				this.b = b;
			}
		}
				
		public class specialNgonal
		{
			Dictionary<long, int> threshIs;
			long[] threshes;
			lib.ngonalIntObject<long> nno;
			int MaxValue = int.MaxValue-100000;
			int granularity = 3000000;
			public specialNgonal(lib.ngonalIntObject<long> nno)
			{
				threshIs = new Dictionary<long, int>();
				this.nno = nno;
				threshes = new long[granularity+1];
				int curr = 0;
				for(int i=1; i<MaxValue; i+=((MaxValue)/granularity)+1)
				{
					long v = nno.gen(i);
					threshes[curr++] = v;
					threshIs.Add(v, i);
				}
				threshes[curr] = long.MaxValue;
			}
			
			public int getI(long v)
			{
				int t = Array.BinarySearch<long>(threshes,v);
				if(t<0)
				{
					t = ~t;
					t-=1;
				}
				long r = threshes[t];
				int start = threshIs[r];
				for(int i=start; r<=v; i++)
				{
					r = nno.gen(i);
					if(r==v)
						return i;
				}
				return -1;
			}
			
			public bool isN(long v)
			{
				return (getI(v)>0);
			}
		}
	}
		
	class Euler043
	{
		public static void test()
		{
			Console.WriteLine(isSpecial(new int[]{1,4,0,6,3,5,7,2,8,9}));
		}
		
		public static void run()
		{
			test();
			long total = 0;
			Euler024.lexPerm lp = new Euler024.lexPerm(10);
			for(int[] curr = lp.getNext(1); curr!=null; curr = lp.getNext(1))
			{			
				if(isSpecial(curr))
				{
					total += long.Parse(
						curr[0].ToString() + 
						curr[1].ToString() + 
						curr[2].ToString() + 
						curr[3].ToString() + 
						curr[4].ToString() + 
						curr[5].ToString() + 
						curr[6].ToString() + 
						curr[7].ToString() + 
						curr[8].ToString() + 
						curr[9].ToString()
					);
				}
			}
			Console.WriteLine(total);
		}
		
		public static bool isSpecial(int[] iA)
		{  //yea, this can be done neater with offset-incrementation and an array of primes, but im lazy, 'kay!?
			int x = 0;
			
			x = int.Parse(iA[1].ToString() + iA[2].ToString() + iA[3].ToString());
			if(x%2!=0) return false;
			
			x = int.Parse(iA[2].ToString() + iA[3].ToString() + iA[4].ToString());
			if(x%3!=0) return false;
			
			x = int.Parse(iA[3].ToString() + iA[4].ToString() + iA[5].ToString());
			if(x%5!=0) return false;
			
			x = int.Parse(iA[4].ToString() + iA[5].ToString() + iA[6].ToString());
			if(x%7!=0) return false;
			
			x = int.Parse(iA[5].ToString() + iA[6].ToString() + iA[7].ToString());
			if(x%11!=0) return false;
			
			x = int.Parse(iA[6].ToString() + iA[7].ToString() + iA[8].ToString());
			if(x%13!=0) return false;
			
			x = int.Parse(iA[7].ToString() + iA[8].ToString() + iA[9].ToString());
			if(x%17!=0) return false;
			
			return true;
		}
	}
	
	class Euler042
	{
		public static void run()
		{
			triangular t = new triangular();
			wordCruncher w = new wordCruncher();
			
			long total = 0;
			
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\words.txt");
			foreach(string line in lines)
			{
				string s=line.Replace("\"","" );
				string[] sA = s.Split(new char[]{','});
				foreach(string word in sA)
				{
					if(t.isT(w.crunch(word)))
					total++;
				}
			}
			
			Console.WriteLine(total);
		}
		
		private class wordCruncher
		{
			Dictionary<char, int> wCache;
			public wordCruncher()
			{
				wCache = new Dictionary<char, int>();
				string s = "0ABCDEFGHIJKLMNOPQRSTUVWXYZ";
				for(int i=1; i<s.Length; i++)
				{
					wCache.Add(s[i],i);
				}
			}
			
			public int crunch(string s)
			{
				int sum = 0;
				foreach(char c in s.ToCharArray())
				{
					sum+=wCache[c];
				}
				return sum;
			}
		}
		
		private class triangular
		{
			int currN;
			Dictionary<int, bool> tCache;
			public triangular()
			{
				currN = 0;
				tCache = new Dictionary<int, bool>();
			}
			
			public bool isT(int x)
			{
				
				while(currN-20<=x)
				{
					tCache.Add((currN*(currN+1))/2, true);
					currN++;
				}
				return tCache.ContainsKey(x);
			}
		}
	}
	
	class Euler041
	{
		public static void run()
		{
			int[] primes = lib.allPrimesBelow(1000000000);
			for(int i=primes.Length-1; i>0; i--)
			{
				if(isPand(primes[i]))
				{
					Console.WriteLine(primes[i]);
					i=0;
				}
			}
		}
		
		public static bool isPand(int x)
		{
			string s = x.ToString();
			
			bool[] t = new bool[s.Length+1];
			foreach(char c in s.ToCharArray())
			{
				int i = ((int)c)-48;
				if(i==0 || i>(s.Length) || t[i])
					return false;
				else
					t[i] = true;
			}
			return true;
		}
	}
	
	class Euler040
	{
		public static void run()
		{
			StringBuilder sb = new StringBuilder(1000500);
			for(int i=1; sb.Length<1000002; i++)
			{
				sb.Append(i.ToString());
			}
			int mult = 1;
			for(int i=1; i<=1000000; i*=10)
			{
				int v = ((int)(sb[i-1]))-48;
				mult*=v;
			}
			
			Console.WriteLine(mult);
		}
		
	}
	
	class Euler039
	{
		public static void run()
		{
			Console.WriteLine(count(120));
			int maxVal = 0;
			int maxI = 0;
			for(int i=1;i<=1000; i++)
			{
				int c = count(i);
				if(c>maxVal)
				{
					maxVal = c;
					maxI = i;
					Console.WriteLine(i.ToString() + " Contains " + c.ToString() + " Solutions");
				}
			}
			
			Console.WriteLine("Final Result:" + maxI.ToString());
		}
		
		public static int count(int p)
		{
			int count=0;
			int hp = 1+(p/2);
			for(int x = 1; x<hp; x++)
			{
				for(int y=x; y<hp; y++)
				{
					int z = p-(x+y);
					if(isPyth(x,y,z))
						count++;
				}
			}
			return count;
		}
		
		public static bool isPyth(int a, int b, int c)
		{
			return ((a*a + b*b)==(c*c));
		}
	}
	
	class Euler038
	{
		public static void run()
		{
			int max = 1;
			
			for(int i=1; i<1000000; i++)
			{				
				int p = pander(i);
				if(p>=max)
				{
					max = p;
				}
				if(p>0)
					Console.WriteLine("int: " + i.ToString() + " Results in " + p.ToString());
			}
			
			//max = pander(192);
			
			Console.WriteLine("Done! Final Results is " + max.ToString());			
		}
		
		public static int pander(int x)
		{
			string s = "";
			for(int i=1; s.Length<9 ;i++)
			{
				s+= (x*i).ToString();
			}
			
			if(s.Length!=9)
				return 0;
			
			if(s.Contains("0"))
				return 0;
			
			if(!lib.doesntRepeatChars(s))
				return 0;
			
			return int.Parse(s);
		}
	}
	
	class Euler037
	{
		public static void test()
		{
			int[] primes = lib.allPrimesBelow(500000);
			Dictionary<int, int> primeDic = new Dictionary<int, int>(primes.Length);
			foreach(int p in primes)
			{
				primeDic.Add(p,p);
			}
			
			Console.WriteLine(isTrunk(3797, primeDic));
		}
		
		public static void run()
		{
			int[] primes = lib.allPrimesBelow(1000000);
			Dictionary<int, int> primeDic = new Dictionary<int, int>(primes.Length);
			foreach(int p in primes)
			{
				primeDic.Add(p,p);
			}
			
			int[] trunks = new int[11];
			int trunkIndex = 0;
			for(int i = 4; trunkIndex<11; i++)
			{
				if(isTrunk(primes[i], primeDic))
					trunks[trunkIndex++] = primes[i];
			}
			
			int sum = 0;
			foreach(int i in trunks)
			{
				sum+=i;
			}
			Console.WriteLine(sum);
		}
		
		public static bool isTrunk(int x, Dictionary<int,int> primeDic)
		{
			string xS = x.ToString();
			for(int i=xS.Length-1; i>0; i--)
			{
				int testInt = int.Parse(xS.Substring(0,i));
				if(!primeDic.ContainsKey(testInt))
					return false;
			}
			
			for(int i=1; i<xS.Length; i++)
			{
				int testInt = int.Parse(xS.Substring(i,xS.Length-(i)));
				if(!primeDic.ContainsKey(testInt))
					return false;
			}
			return true;
		}
	}
	
	class Euler036
	{
		public static void run()
		{
			int sumTotal = 0;
			for(int i=0; i<1000000; i++)
			{
				if(lib.isPalindrome(i.ToString()))
				{
					string t = Convert.ToString(i, 2);
					if(lib.isPalindrome(t))
					{
						sumTotal+=i;
					}
				}
			}
			Console.WriteLine("Done: " + sumTotal.ToString());
		}
	}
	
	class Euler035
	{
		public static void run()
		{
			int max = 1000000;
			List<int> circularPrimes = new List<int>();
			
			int[] primes = lib.allPrimesBelow(max);
			Dictionary<int, int> primeDic = new Dictionary<int, int>(primes.Length);
			
			foreach(int p in primes)
			{
				primeDic.Add(p,p);
			}
			
			for(int i=0; i<primes.Length; i++)
			{
				int prime = primes[i];
				if(isCircular(prime,primeDic))
					circularPrimes.Add(primes[i]);
			}
			
			Console.WriteLine(circularPrimes.Count);
		}
		
		public static bool isCircular(int x, Dictionary<int,int> primeDic)
		{
			if(x.ToString().Contains("0"))
				return false; //Stuff with 0 in it is never circular, plus it confuses logic.
			for(int t = rotate(x); primeDic.ContainsKey(t) ; t = rotate(t))
				if(t==x)
					return true;
			return false;
		}
		
		public static int rotate(int x)
		{
			string xS = x.ToString();
			string final = xS.Substring(1) + xS.Substring(0,1);
			return int.Parse(final);
		}
	}
	
	class Euler034
	{
		public static void run()
		{
			int[] factorials = factFun();
			
			int[] num = new int[9];
			for(int i=0; i<9;i++)
				num[i] = -1;
			num[8] = 3;
			int sumTotal = 0;
			
			for(int curr=3;curr<1000000000;curr++)
			{
				int sum=0;					
				for(int i=0; i<num.Length;i++)
					if(num[i]!=-1)
						sum+=factorials[num[i]];
				if(sum==curr)
				{
					sumTotal+=sum;
					Console.WriteLine(sum.ToString() + " : " + sumTotal.ToString());
				}
				int carry = 1;
				for(int i=8; i>=0; i--)
				{
					if(carry>0 && num[i]==-1)
						num[i]=0;
					num[i]+=carry;
					carry = 0;
					if(num[i]==10)
					{
						num[i]=0;
						carry=1;
					}
				}
			}
		}
		
		public static int[] factFun()
		{
			int[] iA = new int[10];
			iA[0] = 1;
			int currVal = 1;
			for(int i=1; i<10;i++)
			{
				iA[i] = currVal*i;
				currVal = iA[i];
			}
			return iA;
		}
	}
	
	class Euler033
	{
		public static void run()
		{
			tabulator t = doJob();
			int totalNum = 1;
			int totalDen = 1;
			for(int i=0; i<t.numSuccess; i++)
			{
				totalNum*=((10*(t.nums[i*4])) + t.nums[(i*4)+1]);
				totalDen*=((10*(t.nums[i*4+2])) + t.nums[(i*4)+3]);
			}
			Console.WriteLine(totalNum.ToString() + "/" + totalDen.ToString() + " :Learn to simplify yourself!");
			Console.WriteLine("Ok, I'll do it for you: 1/" + totalDen/totalNum);
		}
		
		private static tabulator doJob()
		{
			int prec = 10000;
			
			tabulator t = new tabulator();
			for(int n1 = 1; n1<10; n1++)
			{
				for(int n2 = 1; n2<10; n2++)
				{
					for(int d1 = 1; d1<10; d1++)
					{
						for(int d2 = 1; d2<10; d2++)
						{
							int numerator = (int.Parse((n1.ToString()+n2.ToString())));
							int denominator = (int.Parse((d1.ToString()+d2.ToString())));
							if(numerator<denominator)
							{
								if(n1==d1)
								{
									if(((prec*n2)/d2)==((prec*numerator)/denominator))
										t.itWorks(n1, n2, d1, d2);
								}
								else if(n1==d2)
								{
									if(((prec*n2)/d1)==((prec*numerator)/denominator))
										t.itWorks(n1, n2, d1, d2);
								}
								else if(n2==d2)
								{
									if(((prec*n1)/d1)==((prec*numerator)/denominator))
										t.itWorks(n1, n2, d1, d2);
								}
								else if(n2==d1)
								{
									if(((prec*n1)/d2)==((prec*numerator)/denominator))
										t.itWorks(n1, n2, d1, d2);
								}
							}
						}
					}
				}
			}
			return t;
		}
		
		private class tabulator
		{
			public int numSuccess;
			public List<int> nums;
			
			public tabulator()
			{
				nums = new List<int>();
			}
			
			public void itWorks(int n1, int n2, int d1, int d2)
			{
				numSuccess++;
				nums.AddRange(new int[]{n1, n2, d1, d2});
				Console.WriteLine("Added " + n1.ToString() + n2.ToString() +"/" + d1.ToString() + d2.ToString());
			}
		}
	}
	
	class Euler032
	{
		public static void run()
		{
			List<int> penta = new List<int>();
			long sumAll = 0;
			for(int a=1; a<99; a++)
			{
				if(a%11!=0 && a%10!=0)
				for(int b=1; b<2000; b++)
				{
					if(b%10==0) b++;
					int mult = a*b;
					bool[] test = new bool[10];
					char[] cA = ((a.ToString())+(b.ToString())+((mult).ToString())).ToCharArray();
					if(cA.Length==9)
					{
						bool works = true;
						foreach(char c in cA)
						{
							int x = ((int)c)-48;
							if(x==0 || test[x])
								works = false;
							else
								test[x] = true;
						}
						if(works)
						{
							if(!penta.Contains(mult))
							{
								penta.Add(mult);
								sumAll+=mult;
								Console.WriteLine(a.ToString() + "*" + b.ToString() + "=" + mult.ToString() + " Now at: " + sumAll.ToString());
							}
						}
					}
				}
			}
			
			Console.WriteLine("Final Total: " + sumAll.ToString());
		}
	}
	
	class Euler031
	{
		public static void run()
		{
			Console.WriteLine(count(0, 0));
		}
		
		public static int count(int currCoin, int currSum)
		{
			if(currCoin==7)
			{
				if(currSum==200 || currSum==0)
					return 1;
				else
					return 0;
			}
			int currCount = 0;
			for(int i=0; (currSum + i*coinValues[currCoin])<=200; i++)
			{
				int newSum = currSum + i*coinValues[currCoin];
				currCount += count(currCoin+1,newSum);
			}
			return currCount;
		}
		
		public static int[] coinValues = new int[]{1,2,5,10,20,50,100,200};
	}
	
	class Euler030
	{
		public static void run()
		{
			int pow = 5;
			int[] powers = new int[10];
			for(int i=0; i<10; i++)
				powers[i] = (int)Math.Pow(i,pow);
			
			int[] num = new int[9];
			num[8] = 2;
			
			int sumTotal = 0;
			
			for(int curr=2;curr<1000000000;curr++)
			{
				int sum=0;
				for(int i=0; i<num.Length;i++)
					sum+=powers[num[i]];
				if(sum==curr)
				{
					sumTotal+=sum;
					Console.WriteLine(sum.ToString() + " : " + sumTotal.ToString());
				}
				int carry = 1;
				for(int i=8; i>=0; i--)
				{
					num[i]+=carry;
					carry = 0;
					if(num[i]==10)
					{
						num[i]=0;
						carry=1;
					}
				}
			}
		}
	}
	
	class Euler029
	{
		public static void run()
		{
			Dictionary<string, string> dic = new Dictionary<string, string>();
			int upTo = 100;
			for(int a = 2; a<=upTo; a++)
			{
				zint curr = new zint(a);
				for(int b=2; b<=upTo; b++)
				{
					curr.multWith(a);
					string c = curr.ToString();
					if(!dic.ContainsKey(c))
						dic.Add(c,c);
				}
			}
			Console.WriteLine(dic.Count);
		}
	}
	
	class Euler028
	{
		public static void run()
		{
			long sumTotal = 1;
			
			int offset = 2;
			int curr = 1;
			while(curr<(1001*1001))
			{
				sumTotal+= curr*4 + offset*10;
				curr+=offset*4;
				offset+=2;
			}
			Console.WriteLine(sumTotal);
		}
	}
	
	class Euler027
	{
		public static void run()
		{
			int MaxPrime = 100000;
			SortedList<int,int> primes = new SortedList<int, int>();
			int[] p = lib.allPrimesBelow(MaxPrime);
			foreach(int i in p)
			{
				primes.Add(i,i);
			}
			
			int maxVal = 0; int maxA = 0; int maxB = 0;
			for(int a= -999; a<1000; a++)
			{
				for(int b= -999; b<1000; b++)
				{
					int i;
					for(i=0; i<primes.Count; i++)
					{
						int x = (i*i) + (a*i) + b;
						if(x>=MaxPrime)
							Console.WriteLine("Need more Primes!");
						if(!primes.ContainsKey(x))
							break;
					}
					if(i>=maxVal)
					{
						maxVal = i; maxA = a; maxB = b;
						Console.WriteLine("A: " + a.ToString() + " B: " + b.ToString() + " = " + i.ToString() + " Consecutive Primes.");
						Console.WriteLine("  A*B = " + (a*b).ToString());
					}
				}
			}
		}
	}
	
	class Euler026
	{
		public static void run()
		{
			int maxPat=0; int maxVal = 0;
			for(int i=1; i<1000; i++)
			{
				int pat = div(i);
				if(pat>=maxPat)
				{
					maxPat = pat;
					maxVal = i;
					Console.WriteLine(i.ToString() + " = " + pat.ToString());
				}
			}
		}
		
		public static int div(int x)
		{
			List<int> cache = new List<int>();
			int prevRem = 1;
			while(!cache.Contains(prevRem))
			{
				cache.Add(prevRem);
				int rem;
				Math.DivRem(prevRem*10,x,out rem);
				prevRem = rem;
			}
			int two = cache.LastIndexOf(prevRem);
			return cache.Count-two;
		}
	}
	
	class Euler025
	{
		public static void run()
		{
			zFibber zf = new zFibber();
			string z = "";
			while(z.Length<1000)
			{
				long t;
				z = zf.Next(out t).ToString();
				Console.WriteLine(t.ToString());
			}
		}
		
		public class zFibber
		{
			zint prevF;
			zint currF;
			long i; 
			public zFibber()
			{
				prevF = new zint(1);
				currF = new zint(1);
				i = 0;
			}
			
			public zint Next(out long term)
			{
				if(i++>1)
				{
					zint temp = currF.duplicate();
					temp.addTo(prevF);
					prevF = currF;
					currF = temp;
				}
				term = i;
				return currF.duplicate();
			}
		}
	}
	
	class Euler024
	{
		public static void run()
		{
			lexPerm lp = new lexPerm(10);
			int[] milth = lp.getNext(999999);
			string a = "answer = ";
			foreach(int i in milth)
				a+=(i + " ");
			Console.WriteLine(a);
		}
		
		public class lexPerm
		{
			int[] current;
			int[] end;
			
			public lexPerm(int numberItems)
			{
				current = new int[numberItems];
				end = new int[numberItems];
				for(int i=0;i<numberItems;i++)
				{
					current[i] = i;
					end[i] = numberItems-(i+1);
				}
			}
			
			/// <summary>
			/// Get the next permutation after a certain number of steps
			/// </summary>
			/// <param name="numberSteps">number of steps forward (can be 0 for current)</param>
			/// <returns>the permutation in the form of an array</returns>
			public int[] getNext(int numberSteps)
			{
				for(int step=0; step<numberSteps;step++) 
				{
					if(lib.ArrayEqual<int>(current,end))
						return null;
					
	                int t;
	                if (current[current.Length - 1] > current[current.Length - 2])
	                {	//onsole.WriteLine("next is quick");
	                    t = current[current.Length - 1];
	                    current[current.Length - 1] = current[current.Length - 2];
	                    current[current.Length - 2] = t;
	                }
	                else
	                {	//onsole.WriteLine("next is slow");
	                    int i = current.Length - 2;
	                    while (current[i] > current[i + 1]) i--;
	                    int j = newIntI(current, i);
	                    t = current[i];
	                    current[i] = current[j];
	                    current[j] = t;
	
	                    int[] buffer = new int[current.Length];
	                    for (int tt = 0; tt < buffer.Length; tt++)
	                        buffer[tt] = -1;
	                    int bufferI = 0;
	                    for (t = i + 1; t < current.Length; t++)
	                        buffer[bufferI++] = current[t];
	                    for (j = current.Length - 1; j > i; j--)
	                    {
	                        int maxBuffI = 0;
	                        for (t = 1; t < bufferI; t++)
	                            if (buffer[maxBuffI] < buffer[t]) maxBuffI = t;
	                        current[j] = buffer[maxBuffI];
	                        buffer[maxBuffI] = -1;
	                    }
	                }
				}
				return (int[])(current.Clone());
			}
			
			private static int newIntI(int[] a, int startI)
            {
                int res = startI + 1;
                for (int j = startI + 2; j < a.Length; j++)
                    if (a[j] > a[startI] && a[j] < a[res]) res = j;
                return res;
            }
		}
	}
	
	class Euler023
	{
		public static void run()
		{
			List<int> abundants = new List<int>();
			for(int i=12;i< 28123; i++)
			{
				int[] facts = lib.allFactors(i);
				int sum = 0;
				foreach(int x in facts)
					sum+=x;
				if(i<sum)
					abundants.Add(i);
			}
			
			bool[] res = new bool[28123];
			int numAbun = abundants.Count;
			for(int x=0; x<numAbun; x++)
			{
				for(int y=0; y<numAbun; y++)
				{
					int a = abundants[x];
					int b = abundants[y];
					if((a+b)>=28123)
						break;
					else
						res[a+b] = true;
				}
			}
			
			long finalTotal = 0;
			
			for(int i=1; i<res.Length; i++)
			{
				if(!res[i])
					finalTotal+=i;
			}
			Console.WriteLine(finalTotal);
		}
	}
	
	class Euler022
	{
		public static void run()
		{
			int tot =0;
			string[] lines = System.IO.File.ReadAllLines(@"C:\temp\names.txt");
			SortedList<string, int> sorted = new SortedList<string, int>();
			foreach(string line in lines)
			{
				string s=line.Replace("\"","" );
				string[] sA = s.Split(new char[]{','});
				foreach(string name in sA)
				{
					sorted.Add(name, alphaScore(name));
				}
			}
			
			for(int i=0; i<sorted.Count;i++)
			{
				tot+=((i+1)*sorted.Values[i]);
			}
			
			Console.WriteLine(tot);
		}
		
		public static int alphaScore(string name)
		{
			int tot = 0;
			foreach(char c in name.ToCharArray())
			{
				tot += (int) ((c-'A')+1);
			}
			return tot;
		}
	}
	
	class Euler021
	{
		public static void run()
		{
			List<int> l = new List<int>();
			calcCache<int,int> c = new standardCalcCache<int, int>(new exaCCOBJ());
			for(int i=1; i<10000;i++)
			{
				int v = c.getVal(i);
				if(i!=v && c.getVal(v)==i)
					l.Add(i);
			}
			
			int total = 0;
			foreach(int t in l)
			{
				total+=t;
			}
			
			Console.WriteLine(total);
		}
		
		public class exaCCOBJ : ccObj<int,int>
		{
			public exaCCOBJ()
			{
			}		
				
			public int getVal(int x, calcCache<int,int> c)
			{
				int tot = 0;
				int[] tA = lib.allFactors(x);
				foreach(int a in tA)
					tot+=a;
				return tot;
			}
		}	
	
	}
	
	class Euler019
	{		
		public static void run()
		{
			int total = 0;
			DateTime d = new DateTime(1901,1,1);
			DateTime f = new DateTime(2000,12,31);
			while(d!=f)
			{
				if(isFirstSunday(d))
					total++;
				d = d.AddDays(1);
			}
			Console.WriteLine(total);		
		}
		public static bool isFirstSunday(DateTime d)
		{
			if(d.DayOfWeek == DayOfWeek.Sunday && d.Day==1)
				return true;
			else
				return false;
		}
	}
	
	public class triangle
	{
		int[][] tr;
		int curr;
		public triangle(int maxheight)
		{
			tr = new int[maxheight][];
			curr = -1;
		}
		
		public void addRow(string x)
		{
			string[] sA = x.Split(' ');
			int[] row = new int[curr+2];
			int i = 0;
			foreach(string s in sA)
			{
				row[i++] = int.Parse(s);
			}
			tr[++curr] = row;
		}
		
		public void collapseRow()
		{
			int[] pRow = tr[curr-1];
			int[] cRow = tr[curr];
			for(int i=0; i<pRow.Length;i++)
				pRow[i]+=Math.Max(cRow[i],cRow[i+1]);
			tr[curr--] = null;
		}
		
		public int getTop()
		{
			return tr[0][0];
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for(int i=0; i<tr.Length&&tr[i]!=null;i++)
			{
				int[] row = tr[i];
				foreach(int x in row)
				{
					sb.Append(x);
					sb.Append(' ');
				}
				sb.Remove(sb.Length-1,1);
				sb.AppendLine();
			}
			return sb.ToString();
		}
	}
	
	public class wordify
	{
		static string[] num = new string[]{"", "one", "two", "three", "four","five","six","seven","eight","nine","ten","eleven","twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"};
		static string[] tens = new string[]{"","","twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"};
		
		public static string numb(int x)
		{
			StringBuilder sb = new StringBuilder();
			if(x/1000>0)
			{
				sb.Append(num[x/1000] + " thousand");
				x-= 1000*(x/1000);
			}
			if(x/100>0)
			{
				if(sb.Length!=0)
					sb.Append(" ");
				sb.Append(num[x/100] + " hundred");
				x-= 100*(x/100);
			}
			if(sb.Length!=0 && x!=0)
				sb.Append(" and");
			if(x>=20)
			{
				if(sb.Length!=0)
					sb.Append(" ");
				sb.Append(tens[x/10]);
				x-= 10*(x/10);
			}
			if(x>0)
			{
				if(sb.Length!=0)
					sb.Append(" ");
				sb.Append(num[x]);
			}
			return sb.ToString();
		}
		
		public static int countLetters(string a)
		{
			int tot = 0;
			foreach(char c in a.ToCharArray())
			{
				if(c!=' ')
					tot++;
			}
			return tot;
		}
	}

	public struct tuple : IComparable<tuple>
	{
		public int a;
		public int b;
		int hash;
		
		public tuple(int a, int b)
		{
			this.a = a;
			this.b = b;
			hash = a*b;
		}
		
		public override bool Equals(object obj)
		{
			tuple o = (tuple)obj;
			if((this.a==o.a&&this.b==o.b) || (this.a==o.b&&this.b==o.a))
				return true;
			else
				return false;
		}
		
		public override int GetHashCode()
		{
			return hash;
		}
		
		public int CompareTo(tuple other)
		{
			return 0;
		}
	}
	
	public struct point : IComparable<point>
	{
		public int a;
		public int b;
		int hash;
		
		public point(int a, int b)
		{
			this.a = a;
			this.b = b;
			hash = a*b;
		}
		
		public override bool Equals(object obj)
		{
			point o = (point)obj;
			if(this.a==o.a&&this.b==o.b)
				return true;
			else
				return false;
		}
		
		public override int GetHashCode()
		{
			return hash;
		}
		
		public override string ToString()
		{
			return "(A,B): (" + a.ToString() + "," + b.ToString() + ")";
		}
		
		public static point operator + (point a, point b)
		{
			return new point(a.a+b.a, a.b+b.b);
		}
		
		public static point operator - (point a, point b)
		{
			return new point(a.a-b.a, a.b-b.b);
		}
		
		public int CompareTo(point other)
		{
			return 0;
		}
	}
	
	public class exampleCCOBJ : ccObj<tuple,long>
	{
		public exampleCCOBJ()
		{
		}		
			
		public long getVal(tuple x, calcCache<tuple,long> c)
		{
			if(x.a==0 || x.b==0)
				return (long)1;
			else
			{
				return c.getVal(new tuple(x.a-1,x.b)) + c.getVal(new tuple(x.a,x.b-1));
			}
		}
	}
	
	public interface ccObj<T,V> where T: IComparable<T>
	{
		V getVal(T x, calcCache<T,V> c);
	}
	
	public interface calcCache<T,V> where T: IComparable<T>
	{
		V getVal(T x);
		void clearCache();
		void partialTrim();
		int cacheSize();
	}
	
	public class standardCalcCache<T,V> : calcCache<T, V> where T:IComparable<T>
	{
		private Dictionary<T,V> cache;
		private ccObj<T,V> cco;
		
		public standardCalcCache(ccObj<T,V> cco)
		{
			cache = new Dictionary<T,V>();
			this.cco = cco;
		}
				
		public V getVal(T x)
		{
			V retVal;
			
			if(!cache.TryGetValue(x,out retVal))
			{
				retVal = cco.getVal(x, this);
				cache.Add(x, retVal);
			}
			return retVal;
		}
		
		public void clearCache()
		{
			cache.Clear();
		}
		
		public int cacheSize()
		{
			return cache.Count;
		}
		
		public void partialTrim()
		{
			keepQuarterOfCache();
		}
		
		/// <summary>
		/// Removes a quarter of the cache.
		/// Ostensably, think of it as making an array of all the keys, sorting it by the key's IComparable,
		/// and keeping just the [0 to 1/4] subset
		/// </summary>
		private void keepQuarterOfCache()
		{
			int limit = cache.Count/4;
			List<KeyValuePair<T, V>> tempCache = new List<KeyValuePair<T, V>>(limit*2);
			
			foreach(KeyValuePair<T, V> kvp in cache)
			{
				if(tempCache.Count<limit)
				{
					tempCache.Add(kvp);
					if(tempCache.Count==limit)
					{
						tempCache.Sort((x,y)=>x.Key.CompareTo(y.Key));
					}
				}
				else if(tempCache.Count==limit*2)
				{
					tempCache.Sort((x,y)=>x.Key.CompareTo(y.Key));
					tempCache.RemoveRange(limit,limit);
				}
				else if(kvp.Key.CompareTo(tempCache[limit-1].Key)>0)
					tempCache.Add(kvp);					
			}
			cache.Clear();
			cache = new Dictionary<T, V>(limit*4);
			foreach(KeyValuePair<T, V> kvp in tempCache)
				cache.Add(kvp.Key,kvp.Value);
		}
	}
	
	
	//Idea: a dictionary of the strong, plus a costum hashmap (using the hash of the key) of 10-1000 weakReferenced dictionaries.
	public class weakReferenceCache<T,V> : calcCache<T, V> where T:IComparable<T>
	{
		private ccObj<T,V> cco;
		public weakReferenceCache(ccObj<T,V> cco)
		{
			this.cco = cco;
		}
		
		public V getVal(T x)
		{
			throw new NotImplementedException();
		}
		
		public void partialTrim()
		{
			
		}
		
		public void clearCache()
		{
			throw new NotImplementedException();
		}
		
		public int cacheSize()
		{
			throw new NotImplementedException();
		}
	}
	
	//if you just want the last few digits. Each limit int>0 is 9 digits. add a few buffer ints more than you need just in case ;-)
	public struct limitedzint
	{
		public static limitedzint one = new limitedzint(1,1);
		public static limitedzint zero = new limitedzint(0,1);
		
		int[] intList;
		
		public void increment()
		{
			for(int i=0; intList.Length>i; i++)
			{
				if(intList[i]==999999999)
					intList[i] = 0;
				else
				{
					intList[i]++;
					return;
				}
			}
			
		}
		
		public limitedzint(int x, int digitsDividedByNine)
		{
			intList = new int[digitsDividedByNine];
			intList[0] = x;
			limit();
		}
		
		public limitedzint(int[] x)
		{
			intList = new int[x.Length];
			Array.Copy(x,intList,x.Length);
		}
		
		public void addTo(limitedzint x)
		{
			int[] newIntList =  new int[intList.Length];
			for(int i=0; i<x.intList.Length && i<intList.Length; i++)
			{
				newIntList[i] = intList[i] + x.intList[i];
			}
			intList = newIntList;
			limit();
		}
		
		private void limit()
		{
			int carryover = 0;
			for(int i=0; intList.Length>i; i++)
			{
				intList[i]+=carryover;
				while(intList[i]>999999999)
				{
					intList[i]-= 1000000000;
					carryover++;
				}
			}
		}
		
		/// <summary>
		/// Multiply with [WARNING: NOT OPTIMIZED!!]
		/// </summary>
		/// <param name="x">int to multiply with</param>
		public void multWith(int x)
		{
			if(x==0)
			{
				intList = new int[intList.Length];
				return;
			}
			else if(x==1)
				return;
			
			limitedzint temp = this.duplicate();
			for(long i=0; i<x-1; i++)
			{
				addTo(temp);
			}
			limit();
		}
				
		public limitedzint duplicate()
		{
			return new limitedzint(this.intList);
		}
		
		/// <summary>
		/// Raise to a power [Warning, NOT OPTiMIZED!!]
		/// </summary>
		/// <param name="x">power to raise to</param>
		public void power(int x)
		{
			int l = int.Parse(this.ToString()); //hope this thing aint too big!!
			for(int i=0; i<x-1; i++)
			{
				this.multWith(l);
			}
			limit();
		}
		
		override public string  ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(intList[intList.Length-1]);
			for(int i=intList.Length-2; i>-1; i--)
			{
				sb.Append(intList[i].ToString("D9"));
			}
			return sb.ToString();
		}
	}
	
	public class zint : System.IComparable<zint>, System.IEquatable<zint>
	{
		List<int> store;
		
		public zint(long x)
		{	
			store = new List<int>();
			long carriedOver = x;
			while(store.Count==0 || carriedOver>0)
			{
				addWithCarry(ref carriedOver,0,store);				
			}
		}
		
		private zint(int[] x)
		{
			store = new List<int>();
			for(int i=0; i<x.Length; i++)
			{
				store.Add(x[i]);
			}
		}
		
		public zint(double d)
		{
			if(d<1)
				throw new ArgumentOutOfRangeException();
			string dS = d.ToString("G");
			
			int exp = 0;
			
			string[] sA = dS.Split(new char[]{'E'});
			if(sA.Length==2)
			{
				exp += int.Parse(sA[1].Substring(1));
				dS = sA[0];
			}
			
			sA = dS.Split(new char[]{'.'});
			if(sA.Length!=1)
			{
				dS = sA[0] + sA[1];
				exp -= sA[1].Length;				
			}
			zint tZ = zint.Parse(dS);
			tZ.ee(exp);
			this.store = tZ.store;
		}
		
		public void ee(int x)
		{
			if(x>0)
			{
				int carryover;
				int num = Math.DivRem(x,9,out carryover);
				int mult = 1;
				for(int i=0; i<carryover;i++)
					mult*=10;
				multWith(mult);
				for(int i=0; i<num; i++)
					store.Insert(0,0);
			}
			else if(x<0)
			{
				int carryover;
				int num = Math.DivRem(-x,9,out carryover);
				store.RemoveRange(0,num);
				int mult = 1;
				for(int i=0; i<carryover;i++)
					mult*=10;
				
				carryover = 0;
				for(int i=store.Count-1; i>=0; i--)
				{
					int oldI = store[i];
					int newI = store[i]/mult;
					store[i] = carryover*(1000000000/mult) + newI;
					carryover = oldI-(newI*mult);					
				}
			}
			trim();
		}
		
		public void addTo(long x)
		{
			long carriedOver = x;
			List<int> total = new List<int>();
			for(int i=0; i<store.Count; i++)
			{
				addWithCarry(ref carriedOver, store[i], total);
			}
			while(carriedOver>0)
			{
				addWithCarry(ref carriedOver,0,total);				
			}
			store = total;
		}
		
		public void addTo(zint x)
		{
			List<int> a = store;
			List<int> b = x.store;
			
			if(x.store.Count>store.Count)
			{
				a = x.store;
				b = store;
			}			
			
			long carriedOver = 0;
			List<int> total = new List<int>();
			for(int i=0; i<a.Count; i++)
			{
				long ss = a[i];
				if(i<b.Count)
					ss+=b[i];
				addWithCarry(ref carriedOver,ss, total);						
			}
			while(carriedOver>0)
			{
				addWithCarry(ref carriedOver,0,total);				
			}
			store = total;
		}
		
		public void substract(int x)
		{
			store[0] -= x;
			for(int i=0; store[i]<0; i++)
			{
				store[i]+=1000000000;
				store[i+1]--;
			}
			trim();
		}
		
		public void substract(zint x)
		{			
			int res = this.CompareTo(x);
			if(res<0)
				throw new ArgumentOutOfRangeException();
			else if(res==0)
			{
				store = new List<int>();
				store.Add(0);
			}
			else
			{
				for(int i=0; i<x.store.Count; i++)
				{
					int pos = store[i];
					int neg = x.store[i];
					if(pos<neg)
					{
						pos +=1000000000;
						store[i+1]--;
					}
					store[i] = pos-neg;
				}
				trim();
			}
		}
		
		private void trim()
		{
			while(store.Count>1 && store[store.Count-1]==0)
				store.RemoveAt(store.Count-1);			
		}
		
		private void addTo(long x, int offset)
		{
			while(store.Count<offset+1)
				store.Add(0);
			
			List<int> total = new List<int>();
			for(int i=0; i<offset; i++)
			{
				total.Add(store[i]);
			}
			
			long carriedOver = x;
			for(int i=offset; i<store.Count; i++)
			{
				addWithCarry(ref carriedOver, store[i], total);
			}
			while(carriedOver>0)
			{
				addWithCarry(ref carriedOver,0,total);				
			}
			store = total;
		}
		
		public void multWith(int x)
		{
			if(x==0)
			{
				store.Clear();
				store.Add(0);
				return;
			}
			else if(x==1)
				return;
			
			long carriedOver = 0;
			List<int> total = new List<int>();
			for(int i=0; i<store.Count; i++)
			{
				addWithCarry(ref carriedOver, Math.BigMul(store[i],x), total);						
			}
			while(carriedOver>0)
			{
				addWithCarry(ref carriedOver,0,total);				
			}
			store = total;
		}
		
		private static void addWithCarry(ref long carriedOver, long x, List<int> list)
		{
				long ss =  x + carriedOver;
				
				if(ss>999999999)
				{
					carriedOver = ss/1000000000;
					ss = ss-(carriedOver*1000000000);
				}
				else
					carriedOver = 0;
				
				list.Add((int)ss);	
		}
				
		public void multWith(long x)
		{
			if(x<int.MaxValue)
				multWith((int)x);
			else
				multWith(new zint(x));
		}
		
		public void multWith(zint x)
		{
			if(x.store.Count==1)
				multWith(x.store[0]);
			else
			{
				List<int> mystore = this.store;
				store = new List<int>();
				store.Add(0);
				for(int i=0;i<mystore.Count;i++)
					for(int j=0;j<x.store.Count;j++)
					{
						long b = Math.BigMul(mystore[i],x.store[j]);
						addTo(b,i+j);
					}
			}
		}
		
		public zint duplicate()
		{
			return new zint(this.store.ToArray());
		}
		
		public void DivideBy(int divisor)
		{
			long carriover = 0;
			for(int i=store.Count-1; i>=0; i--)
			{
				carriover = carriover*1000000000 + (store[i]);				
				long result = Math.DivRem(carriover,(long)divisor,out carriover);
				store[i] = (int) result;
			}
			trim();
		}
		
		public double approx()
		{
			double aprox = 0;
			double ee = 1; 
			for(int i=0; i<store.Count; i++)
			{
				aprox+=ee*((double)store[i]);
				ee*=1000000000;
			}
			return aprox;
		}
		
		public void DivideBy(zint divisor, int stack)
		{
			int res = this.CompareTo(divisor);
			if(res<0)
			{
				store = new List<int>();
				store.Add(0);
			}
			else if(res==0)
			{
				store = new List<int>();
				store.Add(1);
			}
			else
			{
				//guesstimate
				double guesstimate = this.approx()/divisor.approx();
				
				zint guess = new zint(guesstimate);
				
				zint multi = guess.duplicate();
				multi.multWith(divisor);
				if(multi<this)
				{
					zint tZ = this.duplicate();
					tZ.substract(multi);
					tZ.DivideBy(divisor, stack+1);
					guess.addTo(tZ);
					multi = guess.duplicate();
					multi.multWith(divisor);
				}
				
				if(multi>this)
				{
					zint tZ = multi.duplicate();
					tZ.substract(this);
					tZ.DivideBy(divisor, stack+1);
					guess.substract(tZ);
					multi = guess.duplicate();
					multi.multWith(divisor);
				}
				
				while(multi>this)
				{					
					guess.substract(new zint(1));
					multi = guess.duplicate();
					multi.multWith(divisor);
				}
				
				this.store = guess.store;
			}
		}
		
		public int remainder(int divisor)
		{
			long carriover = 0;
			for(int i=store.Count-1; i>=0; i--)
			{
				carriover = carriover*1000000000 + (store[i]);
				carriover = carriover%divisor;
			}
			return (int)carriover;
		}
		
		public void power(int x)
		{
			SortedList<int, zint> cache = new SortedList<int, zint>();
			int curr = 1;
			while(x>curr)
			{
				cache.Add(curr,this.duplicate());
				int i=1;
				while(cache.Keys[cache.Count-i]>x-curr)
					i++;
				int fact = cache.Keys[cache.Count-i];
				curr+=fact;
				this.multWith(cache[fact]);
			}
		}
		
		override public string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(store[store.Count-1]);
			for(int i=store.Count-2; i>-1; i--)
			{
				sb.Append(store[i].ToString("D9"));
			}
			return sb.ToString();
		}
		
		public static zint Parse(string s)
		{
			int n = (s.Length/9);
			if(s.Length>n*9)
				n++;
			int[] iA = new int[n];
			for(int i=0; i<iA.Length; i++)
			{
				int length = Math.Min(9,s.Length-(i*9));
				iA[i] = int.Parse(s.Substring(
					s.Length-(i*9+length),
					length));
			}
			return new zint(iA);
		}
		
		public int CompareTo(int other)
		{
			int v = store.Count;
			if(v==1)
			{
				return store[0].CompareTo(other);
			}
			else if(v>2 || store[1]>2)
			{
				return 1;
			}
			else
			{
				return (store[1]*1000000000-other)+store[0];
			}
		}
		
		public int CompareTo(zint other)
		{
			int v = store.Count.CompareTo(other.store.Count);
			if(v!=0)
				return v;
			else
			{
				for(int i=store.Count-1; i>=0; i--)
				{
					int b = store[i].CompareTo(other.store[i]);
					if(b!=0)
						return b;
				}
				return 0;
			}
		}
		
		public override int GetHashCode()
		{
			return this.store[0];
		}
		
		public override bool Equals(object obj)
		{
			zint z = (zint)obj;
			return Equals(z);
		}
		
		public bool Equals(zint other)
		{
			if(other.store.Count!=store.Count)
				return false;
			for(int i=0; i<store.Count;i++)
			{
				if(store[i]!=other.store[i])
					return false;
			}
			return true;
		}
		
		public static zint operator * (zint a, int b)
		{
			zint ad = a.duplicate();
			ad.multWith(b);
			return ad;
		}
		
		public static zint operator * (zint a, zint b)
		{
			zint ad = a.duplicate();
			ad.multWith(b);
			return ad;
		}
		
		public static zint operator - (zint a, zint b)
		{
			zint ad = a.duplicate();
			ad.substract(b);
			return ad;
		}
		
		public static zint operator - (zint a, int b)
		{
			zint ad = a.duplicate();
			ad.substract(b);
			return ad;
		}
		
		public static zint operator ++ (zint a)
		{
			zint ad = a.duplicate();
			ad.addTo(1);
			return ad;
		}
		
		public static zint operator + (zint a, zint b)
		{
			zint ad = a.duplicate();
			ad.addTo(b);
			return ad;
		}
		
		public static zint operator + (zint a, int b)
		{
			zint ad = a.duplicate();
			ad.addTo(b);
			return ad;
		}
		
		public static bool operator > (zint a, zint b)
		{
			return a.CompareTo(b)>0;
		}
		
		public static bool operator < (zint a, zint b)
		{
			return  a.CompareTo(b)<0;
		}
		
		public static bool operator >= (zint a, zint b)
		{
			return a.CompareTo(b)>=0;
		}
		
		public static bool operator <= (zint a, zint b)
		{
			return  a.CompareTo(b)<=0;
		}
		
		public static bool operator == (zint a, zint b)
		{
			bool bnull = object.ReferenceEquals(b, null);
			bool anull = object.ReferenceEquals(a, null);
			
			if(bnull || anull)
			{
				if(bnull && anull)
					return true;
				else
					return false;
			}
			return a.CompareTo(b)==0;
		}
		
		public static bool operator != (zint a, zint b)
		{
			return !(a==b);
		}
		
		public static bool operator > (zint a, int b)
		{
			return a.CompareTo(b)>0;
		}
		
		public static bool operator < (zint a, int b)
		{
			return  a.CompareTo(b)<0;
		}
		
		public static bool operator >= (zint a, int b)
		{
			return a.CompareTo(b)>=0;
		}
		
		public static bool operator <= (zint a, int b)
		{
			return  a.CompareTo(b)<=0;
		}
		
		public static bool operator == (zint a, int b)
		{
			if(object.ReferenceEquals(a, null))
				return false;
			else
				return a.CompareTo(b)==0;
		}
		
		public static bool operator != (zint a, int b)
		{
			return !(a==b);
		}
	}
	
	[Serializable]
	public struct BigInt
	{
		int store;
		long times;
							   //int.Max / 2 (approx.), permits store suming without fear of overflow
		static int splitter =  1073740000;
		public BigInt (long b)
		{
			times = 0;
			while(b>splitter)
			{
				b-=splitter;
				times++;
			}
			store = (int)b;
		}
		
		public int CompareTo(BigInt other)
		{
			int v = times.CompareTo(other.times);
			if(v!=0)
				return v;
			else
				return store.CompareTo(other.store);
		}
		
		private void normalize()
		{
			while(store>=splitter)
			{
				store-=splitter;
				times++;
			}
		}
		
		#region operators
		
		public static BigInt operator + (BigInt a, long b)
		{
			b+=a.store;
			while(b>splitter)
			{
				b-=splitter;
				a.times++;
			}
			a.store=(int)b;
			return a;
		}
		
		public static BigInt operator - (BigInt a, BigInt b)
		{
			a.times -= b.times;
			if(a.store<b.store)
			{
				a.times--;
				a.store+=splitter;
			}
			a.store -= b.store;
			return  a;
		}
		
		public static BigInt operator + (BigInt a, BigInt b)
		{
			a.times += b.times;
			a.store += b.store;
			a.normalize();
			return  a;
		}
		
		public static BigInt operator * (BigInt a, int b)
		{
			a.times*=b;
			long t = Math.BigMul(a.store,b);
			var factor = t/splitter;
			t-=factor*splitter;
			a.times+=factor;
			a.store = (int)t;
			return a;
		}
		
		public static bool operator > (BigInt a, BigInt b)
		{
			return a.CompareTo(b)>0;
		}
		
		public static bool operator < (BigInt a, BigInt b)
		{
			
			var v = a.times - b.times;
			if(v==0)
				v = a.store - b.store;
			return v<0;
		}
		
		public static bool operator >= (BigInt a, BigInt b)
		{
			return a.CompareTo(b)>=0;
		}
		
		public static bool operator <= (BigInt a, BigInt b)
		{
			return  a.CompareTo(b)<=0;
		}
		
		public static bool operator == (BigInt a, BigInt b)
		{
			return a.CompareTo(b)==0;
		}
		
		public static bool operator != (BigInt a, BigInt b)
		{
			return (a.store!=b.store || a.times!=b.times);
		}
				
		#endregion
		
		public string components()
		{
			return (splitter.ToString() + "*" + times.ToString() + " + " + store.ToString());
		}
		
		public override string ToString()
		{
			decimal d = splitter;
			d*=times;
			d+=store;
			return d.ToString();
		}
		
		public override bool Equals(object obj)
		{
			return this == (BigInt)obj;
		}
		
		public override int GetHashCode()
		{
			return store^(times.GetHashCode());
		}
	}
		
	public struct BigInt2
	{
		ulong store;
		int times;
							   //ulong.Max / 2 (approx.), permits store suming without fear of overflow
		static ulong splitter =  9223372000000000000ul;
		
		public BigInt2 (ulong k)
		{
			store = k;
			times = 0;
		}
		
		public int CompareTo(BigInt2 other)
		{
			int v = times.CompareTo(other.times);
			if(v!=0)
				return v;
			else
				return store.CompareTo(other.store);
		}
		
		private void normalize()
		{
			while(store>=splitter)
			{
				store-=splitter;
				times++;
			}
		}
		
		#region operators
		
		public static BigInt2 operator + (BigInt2 a, ulong b)
		{
			while(b>splitter)
			{
				b-=splitter;
				a.times++;
			}
			a.store+=b;
			a.normalize();
			return a;
		}
		
		public static BigInt2 operator - (BigInt2 a, BigInt2 b)
		{
			a.times -= b.times;
			if(a.store<b.store)
			{
				a.times--;
				a.store+=splitter;
			}
			a.store -= b.store;
			return  a;
		}
		
		public static BigInt2 operator + (BigInt2 a, BigInt2 b)
		{
			a.times += b.times;
			a.store += b.store;
			a.normalize();
			return  a;
		}
					
		public static bool operator > (BigInt2 a, BigInt2 b)
		{
			return a.CompareTo(b)>0;
		}
		
		public static bool operator < (BigInt2 a, BigInt2 b)
		{
			return  a.CompareTo(b)<0;
		}
		
		public static bool operator >= (BigInt2 a, BigInt2 b)
		{
			return a.CompareTo(b)>=0;
		}
		
		public static bool operator <= (BigInt2 a, BigInt2 b)
		{
			return  a.CompareTo(b)<=0;
		}
		
		public static bool operator == (BigInt2 a, BigInt2 b)
		{
			return a.CompareTo(b)==0;
		}
		
		public static bool operator != (BigInt2 a, BigInt2 b)
		{
			return !(a==b);
		}
				
		#endregion
		
		public override string ToString()
		{
			decimal d = splitter;
			d*=times;
			d+=store;
			return d.ToString();
		}
		
		public override bool Equals(object obj)
		{
			return this == (BigInt2)obj;
		}
		
		public override int GetHashCode()
		{
			return times^(store.GetHashCode());
		}
	}
	
	
	public class lib
	{
		
		public struct SqrGen2
		{
			public ulong currIndex;
			public BigInt2 currValue;
			
			public SqrGen2(ulong k)
			{
				currValue = new BigInt2(k*k);
				currIndex = k;
			}
			
			public BigInt2 getNext()
			{
				currValue += currIndex;
				currIndex++;
				currValue += currIndex;
				return currValue;
			}
		}
		
		[Serializable]
		public struct SqrGen
		{
			public long currIndex;
			public BigInt currValue;
			
			public SqrGen(long k)
			{
				currValue = new BigInt(k*k);
				currIndex = k;
			}
			
			public BigInt getNext()
			{
				currValue += (currIndex + currIndex + 1);
				currIndex++;
				return currValue;
			}
		}
			
		public static long DigitSum(string s)
		{
			long tot = 0;
			foreach(char c in s.ToCharArray())
			{
				tot += (((int)(c))-48);
			}
			return tot;
		}
		
		public static bool haveSameDigits(int x, int y)
		{
			return lib.ArrayEqual<int>(digitCounts(x),digitCounts(y));
		}
		
		public static int[] digitCounts(int x)
		{
			int[] iC = new int[10];
			foreach(char c in x.ToString().ToCharArray())
			{
				int digit = ((int)c)-48;
				iC[digit]++;
			}
			return iC;
		}
		
		public static int[] PrimeFactors(int x, bool repeats)
		{
			return PrimeFactors(x,new List<int>(allPrimesBelow(x+1)), repeats);
		}
		//Needs a list of primes, atleast all below x.
		public static int[] PrimeFactors(int x, IList<int> Primes, bool repeats)
		{
			IList<int> p = Primes;
			List<int> l = new List<int>();
			int y = x;
			for(int i=0; y>1; i++)
			{
				while((y%(p[i]))==0)
				{
					if(repeats || !l.Contains(p[i]))
						l.Add(p[i]);
					y/=p[i];
				}
			}
			return l.ToArray();
		}
		
		public static int[] PrimeFactors(long x, IList<int> Primes, bool repeats)
		{
			IList<int> p = Primes;
			List<int> l = new List<int>();
			long y = x;
			for(int i=0; y>1; i++)
			{
				while((y%(p[i]))==0)
				{
					if(repeats || !l.Contains(p[i]))
						l.Add(p[i]);
					y/=p[i];
				}
			}
			return l.ToArray();
		}
		
		public static bool ArrayEqual<T>(T[] a, T[] b)
		{
			if(a.Length!=b.Length) return false;
			for(int i=0; i<a.Length; i++)
				if(!a[i].Equals(b[i]))
					return false;
			return true;
		}
		
		public static T[] ArrayUnion<T>(T[] a, T[] b)
		{
			List<T> retVal = new List<T>();
			foreach(T o in a)
				if(!retVal.Contains(o))
					retVal.Add(o);
			foreach(T o in b)
				if(!retVal.Contains(o))
					retVal.Add(o);
			return retVal.ToArray();
		}
		
		public static BitArray isPrimeForAllBelow(int x)
		{
			BitArray buff = new BitArray(x,true);
			for(int curr=2; curr<buff.Length; curr++)
			{
				if(buff[curr]) //true is a prime
				{
					for( int i = curr*2; i<x; i+=curr)
						buff[i] = false; //this aint a prime, so false it.
				}
			}
			return buff;
		}
		
		public static int[] allPrimesBelow(int x)
		{
			List<int> list = new List<int>();
			BitArray buff = new BitArray(x);
			
			for(int curr=2; curr<buff.Length; curr++)
			{
				if(!buff[curr]) //FALSE is a prime, thus we negate
				{
					list.Add(curr);
					for( int i = curr*2; i<x; i+=curr)
						buff[i] = true; //this aint a prime, so TRUE it.
				}
			}
			return list.ToArray();
		}
		
		public class largeBitArray
		{
			BitArray[] bAA;
			long length;
			private static int Split = int.MaxValue;
			
			public largeBitArray(long length, bool defaultVal)
			{
				this.length = length;
				long b = length/Split;
				if(b*Split<length)
					b++;
				bAA = new BitArray[b];
				int curr = 0;
				while(length>Split)
				{
					bAA[curr++] = new BitArray(Split, defaultVal);
					length-=Split;
				}
				bAA[curr] = new BitArray((int)length);
			}
			
			public largeBitArray(long length)
			{
				this.length = length;
				long b = length/Split;
				if(b*Split<length)
					b++;
				bAA = new BitArray[b];
				int curr = 0;
				while(length>Split)
				{
					bAA[curr++] = new BitArray(Split);
					length-=Split;
				}
				bAA[curr] = new BitArray((int)length);
			}
			
			public bool this[long index]
			{
				get{
					int a = (int)(index/((long)Split));
					return bAA[a][(int)(index - Math.BigMul(a,Split))];
				}
				set{
					int a = (int)(index/((long)Split));
					bAA[a][(int)(index - Math.BigMul(a,Split))] = value;					
				}
			}
			
			public long Length
			{
				get
				{
					return length;
				}
			}
		}
		
		public static HashSet<long> HashSetAllPrimesBelow(long x)
		{
			HashSet<long> pants = new HashSet<long>();
			largeBitArray buff = new largeBitArray(x);
			for(long curr=2; curr<x; curr++)
			{
				if(!buff[curr]) //FALSE is a prime, thus we negate
				{
					pants.Add(curr);
					for( long i = curr*2; i<x; i+=curr)
						buff[i] = true; //this aint a prime, so TRUE it.
				}
			}
			return pants;
		}
		
		public static long[] allPrimesBelow(long x)
		{
			List<long> list = new List<long>();
			largeBitArray buff = new largeBitArray(x);
			
			for(long curr=2; curr<buff.Length; curr++)
			{
				if(!buff[curr]) //FALSE is a prime, thus we negate
				{
					list.Add(curr);
					for( long i = curr*2; i<x; i+=curr)
						buff[i] = true; //this aint a prime, so TRUE it.
				}
			}
			return list.ToArray();
		}
		
		public static int[][] FactorsForAllNumsBelow(int x)
		{
			int[][] all = new int[x][];
			all[0] = new int[]{};
			all[1] = new int[]{};
			
			for(int curr=2; curr<x; curr++)
			{
				if(all[curr]==null)
					all[curr] = new int[]{};
				for( int i = curr*2; i<x; i+=curr)
				{
					if(all[i]==null)
					{
						all[i] = new int[]{curr};
					}
					else
					{
						int[] l = all[i];
						int[] newl = new int[l.Length+1];
						Array.Copy(l, newl, l.Length);
						newl[l.Length] = curr;
						all[i] = newl;
					}						
				}
			}
			
			return all;
		}		
		public static int[][] primeFactorsForAllBetween(int min, int max)
		{
			int[][] all = new int[(max+1)-min][];
			BitArray buff = new BitArray(max+1);

			for(int curr=2; curr<buff.Length; curr++)
			{
				if(!buff[curr]) //FALSE is a prime, thus we negate
				{
					for( int i = curr; i<=max; i+=curr)
					{
						buff[i] = true; //this aint a prime, so TRUE it.
						if(i>=min)
						{
							int[] f = all[i-min];
							if(f==null)
							{
								all[i-min] = new int[]{curr};
							}
							else
							{
								int[] newl = new int[f.Length+1];
								Array.Copy(f, newl, f.Length);
								newl[f.Length] = curr;
								all[i-min] = newl;
							}	
						}
					}
				}
			}
			
			return all;
		}		
		public static HashSet<int>[] primeFactorsForAllNumsBelow2(int x)
		{
			HashSet<int>[] all = new HashSet<int>[x];
			for(int i=0; i<all.Length; i++)
				all[i] = new HashSet<int>();
			largeBitArray buff = new largeBitArray(x);
			
			for(int curr=2; curr<buff.Length; curr++)
			{
				if(!buff[curr]) //FALSE is a prime, thus we negate
				{
					all[curr].Add(curr);
					for( int i = curr*2; i<x; i+=curr)
					{
						buff[i] = true; //this aint a prime, so TRUE it.
						all[i].Add(curr);	
					}
				}
			}
			
			return all;
		}		
		
		
		public static int[][] primeFactorCountForAllNumsBelow(int x)
		{
			int[][] all = new int[x][];
			all[0] = new int[]{1};
			all[1] = new int[]{1};
			
			for(int curr=2; curr<all.Length; curr++)
			{
				if(all[curr]==null) //FALSE is a prime, thus we negate
				{
					int[] tA = new int[curr+1];
					tA[0] = 1; tA[curr] = 1;
					all[curr] = tA;
					
					for( int i = curr*2; i<x; i+=curr)
					{
						if(all[i]==null)
						{
							tA = new int[curr+1];
							tA[0] = i/curr; tA[curr]=1;
							while(tA[0]%curr==0)
							{
								tA[0]/=curr;
								tA[curr]++;
							}
							all[i] = tA;
						}
						else
						{
							int[] l = all[i];
							tA = new int[curr+1];
							Array.Copy(l, tA, l.Length);
							while(tA[0]%curr==0)
							{
								tA[0]/=curr;
								tA[curr]++;
							}
							all[i] = tA;
						}						
					}
				}
			}
			
			return all;
		}		
		
		
		public static int[][] primeFactorsForAllNumsBelow(int x)
		{
			int[][] all = new int[x][];
			largeBitArray buff = new largeBitArray(x);
			all[0] = new int[]{};
			all[1] = new int[]{};
			
			for(int curr=2; curr<buff.Length; curr++)
			{
				if(!buff[curr]) //FALSE is a prime, thus we negate
				{
					all[curr] = new int[]{curr};
					for( int i = curr*2; i<x; i+=curr)
					{
						buff[i] = true; //this aint a prime, so TRUE it.
						if(all[i]==null)
						{
							all[i] = new int[]{curr};
						}
						else
						{
							int[] l = all[i];
							int[] newl = new int[l.Length+1];
							Array.Copy(l, newl, l.Length);
							newl[l.Length] = curr;
							all[i] = newl;
						}						
					}
				}
			}
			
			return all;
		}		
		
		public class InfiniteArrayOfPrimes : IList<int>
		{
			SortedList<int, int> cache;
			int curr;
			int currVal;
			
			public InfiniteArrayOfPrimes(int start)
			{
				cache = new SortedList<int, int>();
				int[] p = lib.allPrimesBelow(start);
				for(int i=0; i<p.Length; i++)
				{
					Add(i,p[i]);
				}
			}
			
			private void Add(int x, int prime)
			{
				curr = x;
				currVal = prime;
				cache.Add(x, prime);				
			}
			
			private void Add()
			{
				while(!isPrime(++currVal))
					;
				cache.Add(++curr,currVal);
			}
				
			private bool isPrime(int x)
			{
				foreach(int i in cache.Values)
				{
					if(x%i==0)
						return false;
				}
				return true;
			}
			
			public int this[int index]
			{
				get {
					while(index>curr)
			 			Add();
			 		return cache[index];
				}
				set {
					throw new NotImplementedException();
				}
			}
			
			public int Count {
				get {
					return cache.Count;
				}
			}
			
			public bool IsReadOnly {
				get{return true;}
			}
			
			public int IndexOf(int item)
			{
				throw new NotImplementedException();
			}
			
			public void Insert(int index, int item)
			{
				throw new NotImplementedException();
			}
			
			public void RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			
			public void Add(int item)
			{
				throw new NotImplementedException();
			}
			
			public void Clear()
			{
				throw new NotImplementedException();
			}
			
			public bool Contains(int item)
			{
				while(currVal<item)
					this.Add();
				return this.cache.ContainsValue(item);
			}
			
			public void CopyTo(int[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}
			
			public bool Remove(int item)
			{
				throw new NotImplementedException();
			}
			
			public IEnumerator<int> GetEnumerator()
			{
				throw new NotImplementedException();
			}
			
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}
		
		[Serializable]
		public class InfiniteArrayOfPrimes2 : IList<int>
		{
			int lastPrime;
			List<int> cache;
			
			public InfiniteArrayOfPrimes2(int start)
			{
				cache = new List<int>();
				int[] p = lib.allPrimesBelow(start);
				for(int i=0; i<p.Length; i++)
				{
					AddPrime(p[i]);
				}
				lastPrime = p[p.Length-1];
			}
			
			private void AddPrime(int prime)
			{
				cache.Add(prime);			
			}
			
			private void AddPrime()
			{
				int currVal = cache[cache.Count-1];
				while(!isPrime(++currVal))
					;
				cache.Add(currVal);
				lastPrime = currVal;
			}
				
			private bool isPrime(int x)
			{
				foreach(int i in cache)
				{
					if(x%i==0)
						return false;
				}
				return true;
			}
			
			public int this[int index]
			{
				get {
					while(index>=cache.Count)
						AddPrime();
			 		return cache[index];
				}
				set {
					throw new NotImplementedException();
				}
			}
			
			public int Count {
				get {
					return cache.Count;
				}
			}
			
			public bool IsReadOnly {
				get{return true;}
			}
			
			public int IndexOf(int item)
			{
				if(cache[cache.Count-1]>item)
					return cache.BinarySearch(item);
				else
				{
					while(lastPrime<item)
						AddPrime();
					if(lastPrime==item)
						return cache.Count-1;
					else
						return ~(cache.Count-1);
				}				
			}
			
			public void Insert(int index, int item)
			{
				throw new NotImplementedException();
			}
			
			public void RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			
			public void Add(int item)
			{
				throw new NotImplementedException();
			}
			
			public void Clear()
			{
				cache.Clear();
			}
			
			public bool Contains(int item)
			{
				return 0<=IndexOf(item);
			}
			
			public void CopyTo(int[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}
			
			public bool Remove(int item)
			{
				throw new NotImplementedException();
			}
			
			public IEnumerator<int> GetEnumerator()
			{
				throw new NotImplementedException();
			}
			
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}
		
		public class InfiniteArrayOfPrimes3
		{
			long lastPrime;
			long count;
			List<long[]> cache;
			
			public InfiniteArrayOfPrimes3(long start)
			{
				cache = new List<long[]>();
				long[] p = lib.allPrimesBelow(start);
				cache.Add(p);
				lastPrime = p[p.Length-1];
				count = p.Length;
			}
						
			private void AddPrime()
			{
				List<long> primeList = new List<long>();
				long min = lastPrime;
				long max = lastPrime + lastPrime/2;
				
				largeBitArray buff = new largeBitArray(max+1);
				
				for(long curr=2; curr<buff.Length; curr++)
				{
					if(!buff[curr]) //if FALSE, this is a prime
					{
						for( long i = curr; i<=max; i+=curr)
						{
							buff[i] = true; //this aint a prime, so TRUE it.
						}
						if(curr>min)
							primeList.Add(curr);
					}
				}
				
				lastPrime = primeList[primeList.Count-1];
				count+=primeList.Count;
				cache.Add(primeList.ToArray());
			}
			
			public long this[long index]
			{
				get {
					while(index>=count)
						AddPrime();
					int arrayNum = 0;
					long tot = cache[0].Length;
					while(tot<=index)
						tot+=cache[++arrayNum].Length;
					tot -= cache[arrayNum].Length;
					long newI = index - tot;
					return cache[arrayNum][newI];
				}
				set {
					throw new NotImplementedException();
				}
			}
			
			public long IndexOf(long item)
			{
				while(item>lastPrime)
					AddPrime();
				int arrayNum = 0;
				long tot = 0;
				for(long[] arr = cache[arrayNum]; arr[arr.Length-1]<item; arr=cache[++arrayNum])
					tot+=arr.Length;
				
				int r = Array.BinarySearch<long>(cache[arrayNum],item);
				
				if(r>=0)
					return (tot+((long)r));
				else
					return ~(tot + ((long)(~r)));
			}
			
			public void Clear()
			{
				cache.Clear();
			}
			
			public bool Contains(long item)
			{
				return 0<=IndexOf(item);
			}
		}
		
		public class PrimeFactory
		{
			List<int> primeList;
			int curr;
			
			public PrimeFactory()
			{
				primeList = new List<int>();
				curr = 1;
			}
			
			public int getNext()
			{
				while(!isPrime(++curr))
					;
				primeList.Add(curr);
				return curr;
			}
			
			private bool isPrime(int x)
			{
				foreach(int i in primeList)
				{
					if(x%i==0)
						return false;
				}
				return true;
			}
		}
		
		public interface ngonalLongObject<T>
		{
			T gen(long x);
		}
		
		public interface ngonalIntObject<T>
		{
			T gen(int x);
		}
		
		public class nGonalLong<T> where T:IComparable<T>
		{
			ngonalLongObject<T> nn;
			Dictionary<T, long> backward;
			Dictionary<long,T> forward;
			long curr;
			public nGonalLong(ngonalLongObject<T> n)
			{
				nn = n;
				forward = new Dictionary<long, T>();
				backward = new Dictionary<T, long>();
				curr = 0;
			}
			
			public T getAtI(long x)
			{
				while(forward.Count-2<x)
					Add();
				return forward[x];
			}
			
			private void Add()
			{				
					T v = nn.gen(++curr);
					forward.Add(curr,v);
					backward.Add(v,curr);
			}
			
			public bool isN(T x)
			{
				while(forward[curr].CompareTo(x)<=0);
					Add();
				return backward.ContainsKey(x);
			}
			
			public long getI(T v)
			{
				if(isN(v))
					return  backward[v];
				else
					return -1;
			}
		}
		
		public class nGonalInt<T> where T:IComparable<T>
		{
			ngonalIntObject<T> nn;
			Dictionary<T, int> backward;
			Dictionary<int,T> forward;
			int curr;
			public nGonalInt(ngonalIntObject<T> n)
			{
				nn = n;
				forward = new Dictionary<int, T>();
				backward = new Dictionary<T, int>();
				curr = 0;
			}
			
			public T getAtI(int x)
			{
				while(forward.Count-2<x)
					Add();
				return forward[x];
			}
			
			private void Add()
			{				
					T v = nn.gen(++curr);
					forward.Add(curr,v);
					backward.Add(v,curr);
			}
			
			public bool isN(T x)
			{
				while(forward[curr].CompareTo(x)<=0);
					Add();
				return backward.ContainsKey(x);
			}
			
			public int getI(T v)
			{
				if(isN(v))
					return  backward[v];
				else
					return -1;
			}
		}
		
		public static int[] firstXprimes(int x)
		{
			int primeNumberNumber = x;
			int[] longArr = new int[primeNumberNumber];
			int index=0;
			int testVal=2;
			while(index<primeNumberNumber)
			{
				if(fxphelper(index,testVal,longArr))
					longArr[index++] = testVal;
				testVal++;
			}
			return longArr;
		}
		public static bool fxphelper(int index, int testVal, int[] longArr)
		{
			for(int i=0; i<index; i++)
				{
					if(testVal%(longArr[i])==0)
						return false;
				}
			return true;
		}
		
		public static bool isFactor(long a, long b)
		{
			if((b%a) == 0)
				return true;
			else 
				return false;
		}
		
		public static int[] allFactors(int a)
		{
			List<int> facts = new List<int>();
			facts.Add(1);
			int lim = (int)Math.Sqrt(a);
			for(int i= 2; i<=lim+1; i++)
			{
				if(a%i == 0)
				{
					if(!facts.Contains(i))
						facts.Add(i);
					int q = a/i;
					if(!facts.Contains(q))
						facts.Add(q);
				}
			}
			return facts.ToArray();
		}
		
		public static int numFactors(long a)
		{
			int factCount=0;
			for(long i= (long)1; i<=a; i++)
			{
				if(isFactor(i,a))
					factCount++;
			}
			return factCount;
		}
		
		public static bool isPalindrome(string s)
		{
			int len = s.Length;
			
			for(int i=0; i<((len/2)+1); i++)
			{
				if( s[i]!=s[len-(1+i)])
				   return false;
			}
			
			return true;
		}
		
		public static bool isPalindrome(long a)
		{
			return isPalindrome(a.ToString());			
		}
		
		public static bool isPrime(long a)
		{
			long split = 1 + (long)Math.Pow((double)a, .5);
			for(long i=2; i<split; i++)
				if(a%i==0)
					return false;
			return true;
		}
		
		public static bool doesntRepeatChars(string s)
		{
			Dictionary<char, char> charCache = new Dictionary<char, char>();
			char[] cA = s.ToCharArray();
			foreach(char c in cA)
			{
				if(charCache.ContainsKey(c))
					return false;
				charCache.Add(c,c);
			}
			return true;
		}
		
		public static void fileCacheSave<T>(string filename, T o)
		{
			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, o);
			stream.Close();
		}
		
		public static T fileCacheLoad<T>(string filename)
		{			
			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			T retVal = (T) formatter.Deserialize(stream);
			stream.Close();
			return retVal;
		}
		
		public static TReturn fileCache<T,TReturn>(string filename, Func<T,TReturn> generator, T input)
		{
			TReturn retVal;
			
			if(File.Exists(filename))
			{
				retVal = fileCacheLoad<TReturn>(filename);
			}
			else
			{
				retVal = generator(input);
				fileCacheSave(filename, retVal);				
			}
			return retVal;
		}
		
		public static TReturn fileCache<TReturn>(string filename, Func<TReturn> generator)
		{
			TReturn retVal;
			
			if(File.Exists(filename))
			{
				retVal = fileCacheLoad<TReturn>(filename);				
			}
			else
			{
				retVal = generator();
				fileCacheSave(filename, retVal);
			}
			return retVal;
		}
	}
}