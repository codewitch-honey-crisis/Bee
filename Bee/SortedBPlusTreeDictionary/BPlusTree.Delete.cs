//
// Library: KwData
// File:    BPlusTreeDelete.cs
// Purpose: Define internal BPlusTreeDictionary delete operations.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bee
{
    public partial class SortedBPlusTreeDictionary<TKey, TValue>
    {
        #region Internal methods

        // Delete element already found at path.
        private void _Delete (TreePath path)
        {
			++VersionDirect;
            int leafIndex = path.TopNodeIndex;
            LeafNode leaf = (LeafNode) path.TopNode;

            leaf.Remove (leafIndex);
            --CountDirect;

            if (leafIndex == 0)
                if (leaf.KeyCount != 0)
                    path.SetPivot (path.TopNode.GetKey (0));
                else
                {
                    Debug.Assert (leaf.RightLeaf == null, "only the rightmost leaf should ever be emptied");

                    // Leaf is empty.  Prune it unless it is the only leaf in the tree.
                    LeafNode leftLeaf = (LeafNode) path.GetLeftNode ();
                    if (leftLeaf != null)
                    {
                        leftLeaf.RightLeaf = leaf.RightLeaf;
						 _Demote (path);
                    }

                    return;
                }

            // Leaf underflow?
            if (leaf.KeyCount < leaf.KeyCapacity / 2)
            {
                LeafNode rightLeaf = leaf.RightLeaf;
                if (rightLeaf != null)
                    if (leaf.KeyCount + rightLeaf.KeyCount > leaf.KeyCapacity)
                    {
                        // Balance leaves by shifting pairs from right leaf.
                        int shifts = leaf.KeyCapacity - (leaf.KeyCount + rightLeaf.KeyCount - 1) / 2;
                        leaf.Add (rightLeaf, 0, shifts);
                        rightLeaf.Remove (0, shifts);
                        path.TraverseRight ();
                        path.SetPivot (path.TopNode.GetKey (0));
                    }
                    else
                    {
                        // Coalesce right leaf to current leaf and prune right leaf.
                        leaf.Add (rightLeaf, 0, rightLeaf.KeyCount);
                        leaf.RightLeaf = rightLeaf.RightLeaf;
						path.TraverseRight ();
                        _Demote (path);
                    }
            }
        }


        // Leaf has been emptied, now non-lazy delete its pivot.
        private void _Demote (TreePath path)
        {
            for (; ; )
            {
                Debug.Assert (path.Height > 0);
                path.Pop ();

                InternalNode branch = (InternalNode) path.TopNode;
                if (path.TopNodeIndex == 0)
                {
                    if (branch.KeyCount == 0)
                        // Cascade when rightmost branch is empty.
                        continue;

                    // Rotate pivot for first child.
                    TKey pivot0 = branch.GetKey (0);
                    branch.RemoveKey (0);
                    branch.RemoveChild (0);
                    path.SetPivot (pivot0);
                }
                else
                {
                    // Typical branch pivot delete.
                    branch.RemoveKey (path.TopNodeIndex - 1);
                    branch.RemoveChild (path.TopNodeIndex);
                }

                InternalNode right = (InternalNode) path.TraverseRight ();
                if (right == null)
                {
                    if (branch == _root && branch.KeyCount == 0)
                    {
                        // Prune the empty root.
                        InternalNode newRoot = branch.FirstChild as InternalNode;
                        if (newRoot != null)
                        {
                            _root = (InternalNode) branch.FirstChild;
                            --HeightDirect;
                        }
                    }
                    return;
                }

                if (branch.KeyCount + right.KeyCount < branch.KeyCapacity)
                {
                    // Coalesce left: move pivot and right sibling nodes.
                    branch.AddKey (path.GetPivot ());

                    for (int i = 0; ; ++i)
                    {
                        branch.Add (right.GetChild (i));
                        if (i >= right.KeyCount)
                            break;
                        branch.AddKey (right.GetKey (i));
                    }

                    // Cascade demotion.
                    continue;
                }

                // Branch underflow?
                if (branch.KeyCount < branch.KeyCapacity / 2)
                {
                    int shifts = (right.KeyCount - branch.KeyCount) / 2 - 1;

                    // Balance branches to keep ratio.  Rotate thru the pivot.
                    branch.AddKey (path.GetPivot ());

                    // Shift pairs from right sibling.
                    for (int rightIndex = 0; ; ++rightIndex)
                    {
                        branch.Add (right.GetChild (rightIndex));

                        if (rightIndex >= shifts)
                            break;

                        branch.AddKey (right.GetKey (rightIndex));
                    }

                    path.SetPivot (right.GetKey (shifts));
                    right.Remove (0, shifts + 1);
                }

                return;
            }
        }

        #endregion
    }
}
