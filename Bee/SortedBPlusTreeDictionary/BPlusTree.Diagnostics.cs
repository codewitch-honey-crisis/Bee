#if DIAGNOSTICS
//
// Library: KwData
// File:    BPlusTreeDiagnostics.cs
// Purpose: Define BPlusTreeDictionary API for Debug builds only.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bee
{
#if DEBUG
    [Serializable]
    public class BPlusTreeInsaneException : Exception
    {
        public BPlusTreeInsaneException () { }
        public BPlusTreeInsaneException (string message) : base (message) { }
        public BPlusTreeInsaneException (string message, Exception inner) : base (message, inner) { }
    }


    public partial class BPlusTreeDictionary<TKey, TValue>
    {
        /// <summary>
        /// Perform diagnostics check for data structure internal errors. Since this is an
        /// in-memory managed structure, any errors would indicate a bug. Also performs space
        /// complexity diagnostics to ensure that all non-rightmost nodes maintain 50% fill.
        /// </summary>
        /// </exclude>
        public void SanityCheck ()
        {
            int Order = root.KeyCapacity + 1;

            LeafNode lastLeaf = Check (root, 1, true, default (TKey), null);
            if (lastLeaf.RightLeaf != null)
                throw new BPlusTreeInsaneException ("Last leaf has invalid RightLeaf");
        }


        private LeafNode Check
        (
            InternalNode branch,
            int level,
            bool isRightmost,
            TKey anchor,  // ignored when isRightmost true
            LeafNode visited
        )
        {
            if (branch.KeyCapacity != root.KeyCapacity)
                throw new BPlusTreeInsaneException ("Branch KeyCapacity Inconsistent");

            if (!isRightmost && (branch.KeyCount + 1) < branch.KeyCapacity / 2)
                throw new BPlusTreeInsaneException ("Branch underfilled");

            if (branch.ChildCount != branch.KeyCount + 1)
                throw new BPlusTreeInsaneException ("Branch ChildCount wrong");

            for (int i = 0; i < branch.ChildCount; ++i)
            {
                TKey anchor0 = i == 0 ? anchor : branch.GetKey (i - 1);
                bool isRightmost0 = isRightmost && i < branch.ChildCount;
                if (i < branch.KeyCount - 1)
                    if (branch.GetKey (i).CompareTo (branch.GetKey (i + 1)) >= 0)
                        throw new BPlusTreeInsaneException ("Branch keys not ascending");

                if (level + 1 < height)
                {
                    InternalNode child = (InternalNode) branch.GetChild (i);
                    visited = Check (child, level + 1, isRightmost0, anchor0, visited);
                }
                else
                {
                    LeafNode leaf = (LeafNode) branch.GetChild (i);
                    visited = Check (leaf, isRightmost0, anchor0, visited);
                }
            }
            return visited;
        }


        private LeafNode Check
        (
            LeafNode leaf,
            bool isRightmost,
            TKey anchor,
            LeafNode visited
        )
        {
            if (leaf.KeyCapacity != root.KeyCapacity)
                throw new BPlusTreeInsaneException ("Leaf KeyCapacity Inconsistent");

            if (!isRightmost && leaf.KeyCount < leaf.KeyCapacity / 2)
                throw new BPlusTreeInsaneException ("Leaf underfilled");

            if (!anchor.Equals (default (TKey)) && !anchor.Equals (leaf.GetKey (0)))
                throw new BPlusTreeInsaneException ("Leaf has wrong anchor");

            for (int i = 0; i < leaf.KeyCount; ++i)
                if (i < leaf.KeyCount - 1 && leaf.GetKey (i).CompareTo (leaf.GetKey (i + 1)) >= 0)
                    throw new BPlusTreeInsaneException ("Leaf keys not ascending");

            if (visited == null)
            {
                if (!anchor.Equals (default (TKey)))
                    throw new BPlusTreeInsaneException ("Inconsistent visited, anchor");
            }
            else
                if (visited.RightLeaf != leaf)
                    throw new BPlusTreeInsaneException ("Leaf has bad RightLeaf");

            return leaf;
        }


        /// <summary>
        /// Display contents of tree by level (breadth first).
        /// </summary>
        /// </exclude>
        public void Dump ()
        {
            int level = 0;
            Node first;

            for (; ; )
            {
                TreePath branchPath = new TreePath (this, level);
                first = branchPath.TopNode;
                if (first is LeafNode)
                    break;

                InternalNode branch = (InternalNode) first;

                Console.Write ("L{0}: ", level);
                for (; ; )
                {
                    branch.Dump ();
                    branch = (InternalNode) branchPath.TraverseRight ();

                    if (branch == null)
                        break;
                    Console.Write (" | ");
                }
                ++level;
                Console.WriteLine ();
            }

            TreePath leafPath = new TreePath (this, level);
            Console.Write ("L{0}: ", level);
            for (LeafNode leaf = (LeafNode) first; ; )
            {
                leaf.Dump ();
                leaf = (LeafNode) leafPath.TraverseRight ();
                if (leaf == null)
                    break;

                if (leafPath.IsFirstChild)
                    Console.Write (" | ");
                else
                    Console.Write ("|");
            }
            Console.WriteLine ();
        }
    }



    internal abstract partial class Node
    {
        internal void Dump ()
        {
            for (int k = 0; k < this. KeyCount; k++)
            {
                if (k > 0)
                    Console.Write (",");

                Console.Write (GetKey (k));
            }
        }
    }



    internal partial class TreePath
    {
        internal bool IsFirstChild
        { get { return this.indexStack[Height - 2] == 0; } }

        
        /// <summary>Make an empty path.</summary>
        /// <param name="tree">Target of path.</param>
        internal TreePath (BPlusTreeDictionary<TKey, TValue> tree)
        {
            indexStack = new List<int> ();
            nodeStack = new List<Node> ();
            IsFound = false;

            Push (tree.root, 0);
        }


        /// <summary>Make a path to leftmost branch or leaf at the given level.</summary>
        /// <param name="tree">Target of path.</param>
        /// <param name="level">Level of node to seek where root is level 0.</param>
        /// <remarks>Used only for diagnostics.</remarks>
        internal TreePath (BPlusTreeDictionary<TKey, TValue> tree, int level)
            : this (tree)
        {
            Node node = TopNode;
            for (; ; )
            {
                if (level <= 0)
                    break;
                node = ((InternalNode) node).GetChild (0);
                Push (node, 0);
                --level;
            }
        }
    }
#endif
}
#endif 