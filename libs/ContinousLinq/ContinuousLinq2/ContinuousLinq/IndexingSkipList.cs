using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Text;

namespace ContinuousLinq
{
    public interface ICoinFlipper
    {
        bool Flip();
    }

    public class CoinFlipper : ICoinFlipper
    {
        Random _rand = new Random();

        public bool Flip()
        {
            return (_rand.Next() & 1) == 0;
        }
    }

    public class IndexingSkipList<TValue> : IEnumerable<TValue>
    {
        [DebuggerDisplay("[{ItemsInNode}]")]
        public class Node
        {
            public int ItemsInNode { get; internal set; }
            public TValue Value { get; internal set; }

            public Node Next { get; internal set; }
            public Node Below { get; internal set; }
            public Node Previous { get; internal set; }
            public Node Above { get; internal set; }

            public Node()
            {
            }

            public Node(int nodesBelow, TValue value)
            {
                this.Value = value;
                this.ItemsInNode = nodesBelow;
            }
        }

        public class HeaderNode : Node
        {
        }

        private int _currentLevel;
        private int _maxLevel;

        public int TotalItems { get; internal set; }

        internal ICoinFlipper CoinFlipper { get; set; }
        public Node BottomLeft { get; internal set; }
        public Node TopLeft { get; internal set; }

        public IndexingSkipList()
        {
            CoinFlipper = new CoinFlipper();
            _currentLevel = 0;
            _maxLevel = 32;
            TotalItems = 0;

            TopLeft = new HeaderNode();
            BottomLeft = TopLeft;
        }

        public Node GetLeaf(int index)
        {
            Node closestNode;
            int indicesLeft;
            if (!TryFindNodeForIndex(index, out closestNode, out indicesLeft))
            {
                throw new KeyNotFoundException();
            }

            return closestNode;
        }

        public Node GetLeaf(int index, out int indicesLeft)
        {
            Node closestNode;
            if (!TryFindNodeForIndex(index, out closestNode, out indicesLeft))
            {
                throw new KeyNotFoundException();
            }

            return closestNode;
        }

        public int GetIndex(IndexingSkipList<TValue>.Node node)
        {
            int index = 0;
            var currentNode = node;
            while (currentNode != null)
            {
                int indicesToClosestAboveIncludingCurrentNode;
                var nodeAbove = GetClosestAbove(currentNode, out indicesToClosestAboveIncludingCurrentNode);
                index += indicesToClosestAboveIncludingCurrentNode - currentNode.ItemsInNode;
                currentNode = nodeAbove;
            }

            return index;
        }

        private bool TryFindNodeForIndex(int index, out Node nodeMatchingKey, out int indicesLeft)
        {
            Node currentNode = GetClosestNode(index, out indicesLeft);

            while(currentNode != null && currentNode.ItemsInNode == 0)
            {
                currentNode = currentNode.Next;
            }

            if (currentNode != null)
            {
                nodeMatchingKey = currentNode;
                return true;
            }

            nodeMatchingKey = null;
            return false;
        }

        private Node GetClosestNode(int index, out int indicesLeft)
        {
            var currentNode = GetClosestNode(index, TopLeft, out indicesLeft);
            while (currentNode.Below != null)
            {
                currentNode = currentNode.Below;
                currentNode = GetClosestNode(indicesLeft, currentNode, out indicesLeft);
            }

            if (currentNode.ItemsInNode <= indicesLeft)
            {
                indicesLeft -= currentNode.ItemsInNode;
                currentNode = currentNode.Next;
            }

            return currentNode;
        }

        public Node GetClosestNode(int index, Node startingNode, out int indicesLeft)
        {
            var currentNode = startingNode;
            indicesLeft = index;
            while (currentNode.ItemsInNode < indicesLeft && currentNode.Next != null)
            {
                indicesLeft -= currentNode.ItemsInNode;
                currentNode = currentNode.Next;
            }

            return currentNode;
        }

        private void PromoteEntireTreeToNewLevel(int itemsBeforeNewNodeBelow, TValue value, Node newNodeBelow)
        {
            if (newNodeBelow != null &&
                _currentLevel < _maxLevel &&
                ShouldPromoteToNextLevel())
            {
                var newTopLeftNode = new HeaderNode();

                TopLeft.Above = newTopLeftNode;
                newTopLeftNode.Below = TopLeft;

                newTopLeftNode.ItemsInNode = TotalItems;

                SplitNode(newTopLeftNode, itemsBeforeNewNodeBelow, value, newNodeBelow);

                TopLeft = newTopLeftNode;

                _currentLevel++;
            }
        }

        public Node Add(int index, int itemsInRange, TValue value)
        {
            Node newLeafNode;

            int newNumberOfItems = TotalItems + itemsInRange;

            Node newNodeBelow = AddRecursive(index, itemsInRange, value, TopLeft, out newLeafNode);

            if (newLeafNode != null)
            {
                TotalItems = newNumberOfItems;
            }

            PromoteEntireTreeToNewLevel(index, value, newNodeBelow);

            return newLeafNode;
        }

        private Node AddRecursive(int index, int itemsInRange, TValue value, Node currentNode, out Node newLeaf)
        {
            int indicesLeftOverToReachTargetIndex;
            var closestNodeAtThisLevel = GetClosestNode(index, currentNode, out indicesLeftOverToReachTargetIndex);

            if (closestNodeAtThisLevel.Below != null)
            {
                var newNodeBelow = AddRecursive(indicesLeftOverToReachTargetIndex, itemsInRange, value, closestNodeAtThisLevel.Below, out newLeaf);
                closestNodeAtThisLevel.ItemsInNode += itemsInRange;

                if (newNodeBelow != null && ShouldPromoteToNextLevel())
                {
                    var newSplittingNode = SplitNode(
                        closestNodeAtThisLevel,
                        indicesLeftOverToReachTargetIndex,
                        value,
                        newNodeBelow);

                    return newSplittingNode;
                }
            }
            else
            {
                newLeaf = InsertNewNode(itemsInRange, value, closestNodeAtThisLevel);
                return newLeaf;
            }

            return null;
        }

        private Node SplitNode(Node node, int numberOfItems, TValue value, Node nodeBelow)
        {
            Node splittingNode = InsertNewNode(node.ItemsInNode - numberOfItems, value, node, nodeBelow);

            node.ItemsInNode = numberOfItems;

            return splittingNode;
        }

        private static Node InsertNewNode(int itemsInRange, TValue value, Node nodeBefore)
        {
            Node newNode = new Node(itemsInRange, value);
            newNode.Next = nodeBefore.Next;

            if (newNode.Next != null)
            {
                newNode.Next.Previous = newNode;
            }

            newNode.Previous = nodeBefore;

            nodeBefore.Next = newNode;

            return newNode;
        }

        private static Node InsertNewNode(int itemsInNode, TValue value, Node nodeBefore, Node belowForNewNode)
        {
            var newNode = InsertNewNode(itemsInNode, value, nodeBefore);
            newNode.Below = belowForNewNode;
            belowForNewNode.Above = newNode;

            return newNode;
        }

        public Node AddAfter(Node nodeBefore, int itemsInNode, TValue value)
        {
            TotalItems += itemsInNode;

            var newNode = InsertNewNode(itemsInNode, value, nodeBefore);

            int itemsBeforeClosestAbove;
            var closestAbove = GetClosestAbove(nodeBefore, out itemsBeforeClosestAbove);

            bool canPromoteToNextLevel = true;

            var currentNode = newNode;
            while (closestAbove != null)
            {
                closestAbove.ItemsInNode += itemsInNode;

                if (canPromoteToNextLevel && ShouldPromoteToNextLevel())
                {
                    var newSplittingNode = SplitNode(closestAbove, itemsBeforeClosestAbove, value, currentNode);
                    currentNode = newSplittingNode;
                    closestAbove = GetClosestAbove(currentNode.Previous, out itemsBeforeClosestAbove);
                }
                else
                {
                    canPromoteToNextLevel = false;
                    currentNode = closestAbove;
                    closestAbove = GetClosestAbove(currentNode);
                }
            }

            if (canPromoteToNextLevel)
            {
                PromoteEntireTreeToNewLevel(itemsBeforeClosestAbove, value, currentNode);
            }

            return newNode;
        }

        public void Remove(Node nodeToRemove)
        {
            TotalItems -= nodeToRemove.ItemsInNode;

            var currentNode = nodeToRemove;
            while (currentNode.Above != null)
            {
                var nodeAbove = currentNode.Above;
                nodeAbove.ItemsInNode -= nodeToRemove.ItemsInNode;
                nodeAbove.Previous.ItemsInNode += nodeAbove.ItemsInNode;

                RemoveNode(currentNode);
                currentNode = nodeAbove;
            }
            
            RemoveNode(currentNode);
            currentNode = GetClosestAbove(currentNode);
            while (currentNode != null)
            {
                currentNode.ItemsInNode -= nodeToRemove.ItemsInNode;
                currentNode = GetClosestAbove(currentNode);
            }
        }

        private Node GetClosestAbove(Node node)
        {
            var currentNode = node;

            while (currentNode != null)
            {
                if (currentNode.Above != null)
                {
                    return currentNode.Above;
                }
                currentNode = currentNode.Previous;
            }

            return null;
        }

        private Node GetClosestAboveStoppingAtTopLeft(Node node, out int itemsToNode)
        {
            Node closestAbove = GetClosestAbove(node, out itemsToNode);
            if (closestAbove == null)
            {
                closestAbove = this.TopLeft;
            }
            return closestAbove;
        }

        private Node GetClosestAbove(Node node, out int itemsToNode)
        {
            var currentNode = node;
            itemsToNode = 0;
            while (currentNode != null)
            {
                itemsToNode += currentNode.ItemsInNode;
                if (currentNode.Above != null)
                {
                    return currentNode.Above;
                }

                currentNode = currentNode.Previous;
            }

            return null;
        }

        public void UpdateItemsInNode(Node node, int newNumberOfItems)
        {
            int differenceInItems = newNumberOfItems - node.ItemsInNode;
            TotalItems += differenceInItems;

            var currentNode = node;
            while (currentNode != null)
            {
                currentNode.ItemsInNode += differenceInItems;

                currentNode = GetClosestAbove(currentNode);
            }
        }

        public void Clear()
        {
            this.TopLeft = this.BottomLeft;
            this.TopLeft.Below = null;
            this.TopLeft.Above = null;
            this.TopLeft.Next = null;
            this.TopLeft.Previous = null;
            this.TopLeft.ItemsInNode = 0;
            this.TotalItems = 0;
        }

        private bool ShouldPromoteToNextLevel()
        {
            return this.CoinFlipper.Flip();
        }

        public string Visualize()
        {
            StringBuilder sb = new StringBuilder();

            var currentHeader = this.TopLeft;
            while (currentHeader != null)
            {
                Node previousNode = null;
                var currentNode = currentHeader;
                while (currentNode != null)
                {
                    var leaf = currentNode;
                    while (leaf.Below != null)
                    {
                        leaf = leaf.Below;
                    }

                    int distance = 0;
                    var previousLeaf = leaf.Previous;
                    while (previousLeaf != null)
                    {
                        var above = previousLeaf;
                        while (above != null && above != previousNode)
                        {
                            above = above.Above;
                        }

                        if (above == previousNode)
                        {
                            break;
                        }

                        distance++;
                        previousLeaf = previousLeaf.Previous;
                    }

                    for (int i = 0; i < distance; i++)
                    {
                        sb.Append("    ");
                    }

                    sb.AppendFormat("[{0,2}]", currentNode.ItemsInNode);

                    previousNode = currentNode;
                    currentNode = currentNode.Next;
                }
                sb.AppendLine();
                currentHeader = currentHeader.Below;
            }

            return sb.ToString();
        }

        private void RemoveNode(Node node)
        {
            if (node.Previous != null)
            {
                node.Previous.Next = node.Next;
            }

            if (node.Next != null)
            {
            	node.Next.Previous = node.Previous;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            var currentNode = this.BottomLeft.Next;
            while (currentNode != null)
            {
                yield return currentNode.Value;
                currentNode = currentNode.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Node GetLeafByNodeIndex(int nodeIndex)
        {
            var currentNode = this.BottomLeft.Next;

            int i;
            for (i = 0; currentNode != null && i < nodeIndex; i++)
            {
                currentNode = currentNode.Next;
            }

            return currentNode;
        }
    }
}
