//
// Library: KwData
// File:    BPlusTreeFind.cs
// Purpose: Define BPlusTreeDictionary seek functions without a TreePath.
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

        /// <summary>Get the leftmost leaf.</summary>
        /// <remarks>Used by iteration.</remarks>
        internal LeafNode GetFirstLeaf ()
        {
            for (Node node = _root; ; node = ((InternalNode) node).FirstChild)
            {
                LeafNode leaf = node as LeafNode;
                if (leaf != null)
                    return leaf;
            }
        }
		internal LeafNode GetLastLeaf()
		{
			for (Node node = _root; ; node = ((InternalNode)node).LastChild)
			{
				LeafNode leaf = node as LeafNode;
				if (leaf != null)
					return leaf;
			}
		}


		/// <summary>Perform lite search for key.</summary>
		/// <param name="key">Target of search.</param>
		/// <param name="index">When found, holds index of returned Leaf; else ~index of nearest greater key.</param>
		/// <returns>Leaf holding target (found or not).</returns>
		internal LeafNode Find (TKey key, out int index)
        {
            //  Method is unfolded on comparer to improve speed 5%.
            if (_comparer == Comparer<TKey>.Default)
                for (Node node = _root; ; )
                {
                    int nodeIndex = node.Search (key);

                    InternalNode branch = node as InternalNode;
                    if (branch == null)
                    {
                        index = nodeIndex;
                        return (LeafNode) node;
                    }

                    node = branch.GetChild (nodeIndex < 0 ? ~nodeIndex : nodeIndex + 1);
                }
            else
            {
                for (Node node = _root; ; )
                {
                    int nodeIndex = node.Search (key, _comparer);

                    InternalNode branch = node as InternalNode;
                    if (branch == null)
                    {
                        index = nodeIndex;
                        return (LeafNode) node;
                    }

                    node = branch.GetChild (nodeIndex < 0 ? ~nodeIndex : nodeIndex + 1);
                }
            }
        }

        #endregion
    }
}
