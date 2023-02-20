////
////  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
////  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
////  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
////  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
////  REMAINS UNCHANGED.
////
////  Email:  gustavo_franco@hotmail.com
////
////  Copyright (C) 2006 Franco, Gustavo 
////
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//
//namespace TGS.PathFinding {
//
//	public interface IPriorityQueue<T> {
//		int Push (T item);
//
//		T Pop ();
//
//		T Peek ();
//
//		void Update (int i);
//	}
//
//	public class PriorityQueueB<T> : IPriorityQueue<T> {
//		protected List<T>       InnerList = new List<T> ();
//		protected IComparer<T>  mComparer;
//
//		public PriorityQueueB ()
//		{
//			mComparer = Comparer<T>.Default;
//		}
//
//		public PriorityQueueB (IComparer<T> comparer)
//		{
//			mComparer = comparer;
//		}
//
//		public IComparer<T> comparer { get { return mComparer; } }
//
//		public PriorityQueueB (IComparer<T> comparer, int capacity)
//		{
//			mComparer = comparer;
//			InnerList.Capacity = capacity;
//		}
//
//		protected void SwitchElements (int i, int j) {
//			T h = InnerList [i];
//			InnerList [i] = InnerList [j];
//			InnerList [j] = h;
//		}
//
//		protected virtual int OnCompare (int i, int j) {
//			return mComparer.Compare (InnerList [i], InnerList [j]);
//		}
//
//		/// <summary>
//		/// Push an object onto the PQ
//		/// </summary>
//		/// <param name="O">The new object</param>
//		/// <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
//		public int Push (T item) {
//			int p = InnerList.Count, p2;
//			InnerList.Add (item); // E[p] = O
//			do {
//				if (p == 0)
//					break;
//				p2 = (p - 1) / 2;
//				if (OnCompare (p, p2) < 0) {
//					SwitchElements (p, p2);
//					p = p2;
//				} else
//					break;
//			} while(true);
//			return p;
//		}
//
//		/// <summary>
//		/// Get the smallest object and remove it.
//		/// </summary>
//		/// <returns>The smallest object</returns>
//		public T Pop () {
//			T result = InnerList [0];
//			int p = 0, p1, p2, pn;
//			int count = InnerList.Count - 1;
//			InnerList [0] = InnerList [count];
//			InnerList.RemoveAt (count);
//			do {
//				pn = p;
//				p1 = 2 * p + 1;
//				p2 = p1 + 1; //2 * p + 2;
//				if (count > p1 && OnCompare (p, p1) > 0) // links kleiner		// InnerList.Count
//					p = p1;
//				if (count > p2 && OnCompare (p, p2) > 0) // rechts noch kleiner	// InnerList.Count
//					p = p2;
//				
//				if (p == pn)
//					break;
//				SwitchElements (p, pn);
//			} while(true);
//
//			return result;
//		}
//
//		/// <summary>
//		/// Notify the PQ that the object at position i has changed
//		/// and the PQ needs to restore order.
//		/// Since you dont have access to any indexes (except by using the
//		/// explicit IList.this) you should not call this function without knowing exactly
//		/// what you do.
//		/// </summary>
//		/// <param name="i">The index of the changed object.</param>
//		public void Update (int i) {
//			int p = i, pn;
//			int p1, p2;
//			do {	// aufsteigen
//				if (p == 0)
//					break;
//				p2 = (p - 1) / 2;
//				if (OnCompare (p, p2) < 0) {
//					SwitchElements (p, p2);
//					p = p2;
//				} else
//					break;
//			} while(true);
//			if (p < i)
//				return;
//			do {	   // absteigen
//				pn = p;
//				p1 = 2 * p + 1;
//				p2 = 2 * p + 2;
//				if (InnerList.Count > p1 && OnCompare (p, p1) > 0) // links kleiner
//					p = p1;
//				if (InnerList.Count > p2 && OnCompare (p, p2) > 0) // rechts noch kleiner
//					p = p2;
//				
//				if (p == pn)
//					break;
//				SwitchElements (p, pn);
//			} while(true);
//		}
//
//		/// <summary>
//		/// Get the smallest object without removing it.
//		/// </summary>
//		/// <returns>The smallest object</returns>
//		public T Peek () {
//			if (InnerList.Count > 0)
//				return InnerList [0];
//			return default(T);
//		}
//
//		public void Clear () {
//			InnerList.Clear ();
//		}
//
//		public int Count {
//			get{ return InnerList.Count; }
//		}
//
//		public void RemoveLocation (T item) {
//			int index = -1;
//			for (int i=0; i<InnerList.Count; i++) {
//                
//				if (mComparer.Compare (InnerList [i], item) == 0)
//					index = i;
//			}
//
//			if (index != -1)
//				InnerList.RemoveAt (index);
//		}
//
//		public T this [int index] {
//			get { return InnerList [index]; }
//			set { 
//				InnerList [index] = value;
//				Update (index);
//			}
//		}
//	}
//}

//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//
//  Switched from Generic List to Generic Vector and additional performance changes by Kronnect

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace TGS.PathFinding {
	public interface IPriorityQueue<T> {
		int Push (T item);

		T Pop ();

		T Peek ();

		void Update (int i);
	}

	public class PriorityQueueB<T> : IPriorityQueue<T> {
		protected T[] InnerList;
		protected IComparer<T> mComparer;
		protected int InnerListCount;

		public PriorityQueueB () {
			mComparer = Comparer<T>.Default;
			InnerList = new T[4096];
			InnerListCount = 0;
		}

		public PriorityQueueB (IComparer<T> comparer) {
			mComparer = comparer;
			InnerList = new T[4096];
			InnerListCount = 0;
		}

		public IComparer<T> comparer { get { return mComparer; } }


		protected void SwapElements (int i, int j) {
			T h = InnerList [i];
			InnerList [i] = InnerList [j];
			InnerList [j] = h;
		}

		protected virtual int OnCompare (int i, int j) {
			return mComparer.Compare (InnerList [i], InnerList [j]);
		}

		/// <summary>
		/// Push an object onto the PQ
		/// </summary>
		/// <param name="item">The new object</param>
		/// <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
		public int Push (T item) {
			int p = InnerListCount, p2;
			if (InnerListCount >= InnerList.Length) {
				ResizeInnerList ();
			}
			InnerList [InnerListCount++] = item;
			do {
				if (p == 0)
					break;
				p2 = (p - 1) / 2;
				T InnerListp = InnerList [p];
				T InnerListp2 = InnerList [p2];
				if (mComparer.Compare (InnerListp, InnerListp2) < 0) {
					// Swap
					InnerList [p] = InnerListp2;
					InnerList [p2] = InnerListp;
					p = p2;
				} else
					break;
			} while(true);
			return p;
		}

		void ResizeInnerList () {
			T[] newInnerList = new T[InnerListCount * 2];
			Array.Copy (InnerList, newInnerList, InnerListCount);
			InnerList = newInnerList;
		}

		/// <summary>
		/// Get the smallest object and remove it.
		/// </summary>
		/// <returns>The smallest object</returns>
		public T Pop () {
			T result = InnerList [0];
			int p = 0, p1, p2, pn;
			int count = InnerListCount - 1;
			InnerList [0] = InnerList [count]; // InnerList.Count - 1
			InnerListCount--;
			do {
				pn = p;
				p1 = 2 * p + 1;
				p2 = p1 + 1; //2 * p + 2;

				if (count > p1) {
					if (mComparer.Compare (InnerList [p], InnerList [p1]) > 0) {
						p = p1;
					}
				}
				if (count > p2) {
					if (mComparer.Compare (InnerList [p], InnerList [p2]) > 0) {
						p = p2;
					}
				}

				if (p == pn)
					break;

				// Swap
				T h = InnerList [p];
				InnerList [p] = InnerList [pn];
				InnerList [pn] = h;

			} while(true);

			return result;
		}

		/// <summary>
		/// Notify the PQ that the object at position i has changed
		/// and the PQ needs to restore order.
		/// Since you dont have access to any indexes (except by using the
		/// explicit IList.this) you should not call this function without knowing exactly
		/// what you do.
		/// </summary>
		/// <param name="i">The index of the changed object.</param>
		public void Update (int i) {
			int p = i, pn;
			int p1, p2;
			do {	// aufsteigen
				if (p == 0)
					break;
				p2 = (p - 1) / 2;
				if (OnCompare (p, p2) < 0) {
					SwapElements (p, p2);
					p = p2;
				} else
					break;
			} while(true);
			if (p < i)
				return;
			do {	   // absteigen
				pn = p;
				p1 = 2 * p + 1;
				p2 = 2 * p + 2;
				if (InnerListCount > p1 && OnCompare (p, p1) > 0) // links kleiner
					p = p1;
				if (InnerListCount > p2 && OnCompare (p, p2) > 0) // rechts noch kleiner
					p = p2;

				if (p == pn)
					break;
				SwapElements (p, pn);
			} while(true);
		}

		/// <summary>
		/// Get the smallest object without removing it.
		/// </summary>
		/// <returns>The smallest object</returns>
		public T Peek () {
			if (InnerListCount > 0)
				return InnerList [0];
			return default(T);
		}

		public void Clear () {
			InnerListCount = 0; //.Clear ();
		}

		public int Count {
			get{ return InnerListCount; }
		}

		public void RemoveLocation (T item) {
			int index = -1;
			for (int i = 0; i < InnerListCount; i++) {
				if (mComparer.Compare (InnerList [i], item) == 0) {
					index = i;
					break;
				}
			}

			if (index != -1) {
				//																InnerList.RemoveAt (index);
				for (int i = index; i < InnerListCount - 1; i++) {
					InnerList [i] = InnerList [i + 1];
				}
				InnerListCount--;
			}
		}

		public T this [int index] {
			get { return InnerList [index]; }
			set { 
				InnerList [index] = value;
				Update (index);
			}
		}
	}
}