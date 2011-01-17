using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class IndexingSkipListTest
    {
        public class MockCoinFlipper : ICoinFlipper
        {
            public bool DefaultFlip { get; set; }

            private Queue<bool> _nextFlips;

            public MockCoinFlipper()
            {
                DefaultFlip = false;
                _nextFlips = new Queue<bool>();
            }

            public void QueueFlip(bool value)
            {
                _nextFlips.Enqueue(value);
            }

            public bool Flip()
            {
                return _nextFlips.Count == 0 ? DefaultFlip : _nextFlips.Dequeue();
            }
        }
        private IndexingSkipList<object> _target;
        private List<object> _values;
        private List<IndexingSkipList<object>.HeaderNode> _headers;
        private List<IndexingSkipList<object>.Node> _leaves;
        private MockCoinFlipper _coinFlipper;
        
        [SetUp]
        public void Setup()
        {
            _coinFlipper = new MockCoinFlipper();

            _values = new List<object>()
		            {
		                new object(),
		                new object(),
		                new object(),
		            };

            _target = new IndexingSkipList<object>();
        }

        private void LinkNodes(IndexingSkipList<object>.Node before, IndexingSkipList<object>.Node after)
        {
            if (before != null)
            {
                before.Next = after;
            }
            if (after != null)
            {
                after.Previous = before;
            }
        }

        private void StackNodes(IndexingSkipList<object>.Node below, IndexingSkipList<object>.Node above)
        {
            if (below != null)
            {
                below.Above = above;
            }
            if (above != null)
            {
                above.Below = below;
            }
        }

        private void CreateHeaders(int headers)
        {
            _headers = new List<IndexingSkipList<object>.HeaderNode>();

            for (int i = 0; i < headers; i++)
            {
                var currentNode = new IndexingSkipList<object>.HeaderNode();
                _headers.Add(currentNode);

                if (i > 0)
                {
                    StackNodes(currentNode, _headers[i - 1]);
                }
            }
        }

        private void CreateAbove(IndexingSkipList<object>.Node node, int numberAbove)
        {
            var previousNode = node;
            for (int i = 0; i < numberAbove; i++)
            {
                var currentNode = new IndexingSkipList<object>.Node();
                StackNodes(previousNode, currentNode);
                previousNode = currentNode;
            }
        }


        private void CreateLeaves(int leaves, int itemsPerNode)
        {
            _leaves = new List<IndexingSkipList<object>.Node>();

            for (int i = 0; i < leaves; i++)
            {
                var currentNode = new IndexingSkipList<object>.Node(itemsPerNode, new object());
                _leaves.Add(currentNode);

                if (i > 0)
                {
                    LinkNodes(_leaves[i - 1], currentNode);
                }
            }
        }

        private void CreateTree(int headers, int leaves, int itemsPerNode)
        {
            CreateHeaders(headers);
            CreateLeaves(leaves, itemsPerNode);

            LinkNodes(_headers[_headers.Count - 1], _leaves[0]);

            _target.TopLeft = _headers[0];
            _target.BottomLeft = _headers[_headers.Count - 1];
            _target.CoinFlipper = _coinFlipper;
        }

        private void CreateThreeLevelTree(int itemsPerNode)
        {
            int headers = 3;
            int leaves = 5;
            CreateTree(headers, leaves, itemsPerNode);

            CreateAbove(_leaves[1], 1);
            _leaves[1].Above.ItemsInNode = 2 * itemsPerNode;

            CreateAbove(_leaves[3], 1);
            _leaves[3].Above.ItemsInNode = 2 * itemsPerNode;

            LinkNodes(_headers[1], _leaves[1].Above);
            LinkNodes(_leaves[1].Above, _leaves[3].Above);

            _headers[0].ItemsInNode = 5 * itemsPerNode;
            _headers[1].ItemsInNode = 1 * itemsPerNode;

            _target.TotalItems = leaves * itemsPerNode; 
        }

        private void CreateMostlyEmptyTree()
        {
            CreateTree(3, 5, 0);
            _leaves[4].ItemsInNode = 1;

            CreateAbove(_leaves[1], 1);
            _leaves[1].Above.ItemsInNode = 0;

            CreateAbove(_leaves[3], 1);
            _leaves[3].Above.ItemsInNode = 1;

            LinkNodes(_headers[1], _leaves[1].Above);
            LinkNodes(_leaves[1].Above, _leaves[3].Above);

            _headers[0].ItemsInNode = 1;
            _headers[1].ItemsInNode = 0;
        }

        private void AssertLinked(IndexingSkipList<object>.Node before, IndexingSkipList<object>.Node after)
        {
            Assert.IsNotNull(before);
            Assert.IsNotNull(after);
            Assert.AreEqual(before, after.Previous);
            Assert.AreEqual(after, before.Next);
        }

        [Test]
        public void Add_OneItem_ItemInList()
        {
            _coinFlipper.QueueFlip(true);
            _target.CoinFlipper = _coinFlipper;
            _target.Add(0, 1, _values[0]);

            var node = _target.GetLeaf(0);
            Assert.AreEqual(_values[0], node.Value);
        }

        [Test]
        public void Add_OneItemInThreeLevelList_ItemInList()
        {
            CreateThreeLevelTree(1);
            var newNode = _target.Add(0, 1, _values[0]);

            Assert.IsNotNull(newNode);
            AssertLinked(_headers[2], newNode);

            Assert.AreEqual(2, _headers[1].ItemsInNode);
            Assert.AreEqual(6, _headers[0].ItemsInNode);
        }

        [Test]
        public void Add_OneItemInThreeLevelListWithPromotion_ItemInList()
        {
            CreateThreeLevelTree(1);
            _coinFlipper.QueueFlip(true);

            var newNode = _target.Add(0, 1, _values[0]);

            Assert.IsNotNull(newNode);
            AssertLinked(_headers[2], newNode);

            Assert.IsNotNull(newNode.Above);
            AssertLinked(_headers[1], newNode.Above); 

            Assert.AreEqual(2, newNode.Above.ItemsInNode);

            Assert.AreEqual(0, _headers[1].ItemsInNode);
            Assert.AreEqual(6, _headers[0].ItemsInNode);
        }

        [Test]
        public void Add_OneItemInThreeLevelListWithPromotionDeepInList_ItemInList()
        {
            CreateThreeLevelTree(1);
            _coinFlipper.QueueFlip(true);
            _coinFlipper.QueueFlip(true);

            var newNode = _target.Add(3, 1, _values[0]);

            Assert.IsNotNull(newNode);
            AssertLinked(_leaves[2], newNode);
            AssertLinked(newNode, _leaves[3]);

            Assert.IsNotNull(newNode.Above);

            AssertLinked(_leaves[1].Above, newNode.Above);
            AssertLinked(newNode.Above, _leaves[3].Above);

            Assert.AreEqual(2, _leaves[1].Above.ItemsInNode);
            Assert.AreEqual(1, newNode.Above.ItemsInNode);

            Assert.IsNotNull(newNode.Above.Above);

            AssertLinked(_headers[0], newNode.Above.Above);

            Assert.IsNull(newNode.Above.Above.Next);

            Assert.AreEqual(3, _headers[0].ItemsInNode);
            Assert.AreEqual(3, newNode.Above.Above.ItemsInNode);
        }

        [Test]
        public void Add_TwoItems_ItemsInList()
        {
            _target.Add(0, 1, _values[0]);
            _target.Add(1, 1, _values[1]);

            Assert.AreEqual(_values[0], _target.GetLeaf(0).Value);
            Assert.AreEqual(_values[1], _target.GetLeaf(1).Value);
        }

        [Test]
        public void Add_ManyManyItems_AllValuesFound()
        {
            int numberOfItems = 10000;
            List<object> values = new List<object>(numberOfItems);

            for (int i = 0; i < numberOfItems; i++)
            {
                var value = new object();
                values.Add(value);
                _target.Add(i * 3, 3, value);
            }

            for (int i = 0; i < numberOfItems; i++)
            {
                Assert.AreEqual(values[i], _target.GetLeaf(i * 3).Value);
                Assert.AreEqual(values[i], _target.GetLeaf(i * 3 + 1).Value);
                Assert.AreEqual(values[i], _target.GetLeaf(i * 3 + 2).Value);
            }
        }

        [Test]
        public void GetLeaf_IndexOneExists_ReturnsNode()
        {
            CreateThreeLevelTree(1);
            var node = _target.GetLeaf(1);
            Assert.AreEqual(_leaves[1], node);
        }

        [Test]
        public void GetLeaf_IndexZero_ReturnsNode()
        {
            CreateThreeLevelTree(1);
            var node = _target.GetLeaf(0);
            Assert.AreEqual(_leaves[0], node);
        }

        [Test]
        public void GetLeaf_WithIndicesLeftIndexFallsExactlyOnBoundary_IndicesLeftZero()
        {
            CreateThreeLevelTree(1);
            int indicesLeft;
            var node = _target.GetLeaf(1, out indicesLeft);
            Assert.AreEqual(_leaves[1], node);
            Assert.AreEqual(0, indicesLeft);
        }

        [Test]
        public void GetLeaf_WithIndicesLeftMultipleItemsPerLeafAndIndexFallsWithinLeaf_IndicesLeftHasCarryOver()
        {
            CreateThreeLevelTree(3);
            int indicesLeft;
            var node = _target.GetLeaf(1, out indicesLeft);
            Assert.AreEqual(_leaves[0], node);
            Assert.AreEqual(1, indicesLeft);
        }

        [Test]
        public void GetLeaf_ThreeItemsPerNode_ReturnsNode()
        {
            CreateThreeLevelTree(3);
            var node = _target.GetLeaf(1);
            Assert.AreEqual(_leaves[0], node);
        }

        [Test]
        public void UpdateNumberOfItems_DeepInTree_UpdatesParentNodes()
        {
            CreateThreeLevelTree(1);
            _target.UpdateItemsInNode(_leaves[2], 3);

            Assert.AreEqual(3, _leaves[2].ItemsInNode);
            Assert.AreEqual(4, _leaves[1].Above.ItemsInNode);
            Assert.AreEqual(7, _headers[0].ItemsInNode);
        }

        [Test]
        public void AddAfter_ItemInListWithPromotions_UpdatesParents()
        {
            CreateThreeLevelTree(1);
            _coinFlipper.DefaultFlip = true;

            IndexingSkipList<object>.Node newNode = _target.AddAfter(_leaves[1], 2, _values[0]);

            Assert.AreEqual(_values[0], newNode.Value);

            Assert.AreEqual(2, newNode.ItemsInNode);
            Assert.AreEqual(3, newNode.Above.ItemsInNode);
            Assert.AreEqual(1, _leaves[1].Above.ItemsInNode); 

            AssertLinked(_leaves[1], newNode);
            AssertLinked(_leaves[1].Above, newNode.Above);
            AssertLinked(_headers[0], newNode.Above.Above);
            AssertLinked(_headers[0].Above, newNode.Above.Above.Above);
        }

        [Test]
        public void AddAfter_NoPromotions_UpdatesParents()
        {
            CreateThreeLevelTree(1);

            IndexingSkipList<object>.Node newNode = _target.AddAfter(_target.BottomLeft, 2, _values[0]);

            Assert.AreEqual(_values[0], newNode.Value);

            Assert.AreEqual(2, newNode.ItemsInNode);
            Assert.AreEqual(3, _headers[1].ItemsInNode);
            Assert.AreEqual(7, _headers[0].ItemsInNode);

            AssertLinked(_target.BottomLeft, newNode);
        }

        [Test]
        public void Remove_InsideOfTree_UpdatesParents()
        {
            CreateThreeLevelTree(1);
            _target.Remove(_leaves[2]);
            
            Assert.AreEqual(1, _leaves[1].Above.ItemsInNode);
            Assert.AreEqual(4, _headers[0].ItemsInNode);

            AssertLinked(_leaves[1], _leaves[3]);
            AssertLinked(_leaves[1].Above, _leaves[3].Above);
        }

        [Test]
        public void Remove_InsideOfWithParents_UpdatesParents()
        {
            CreateThreeLevelTree(1);
            _target.Remove(_leaves[1]);

            Assert.AreEqual(4, _headers[0].ItemsInNode);
            Assert.AreEqual(2, _headers[1].ItemsInNode);

            AssertLinked(_leaves[0], _leaves[2]);
            AssertLinked(_headers[1], _leaves[3].Above);
        }

        [Test]
        public void AddAndRemove_ManyManyItems_AllValuesFound()
        {
            int numberOfItems = 20000;
            List<object> values = new List<object>(numberOfItems);

            List<IndexingSkipList<object>.Node> nodes = new List<IndexingSkipList<object>.Node>(numberOfItems);

            IndexingSkipList<object>.Node lastNode = _target.BottomLeft;
            for (int i = 0; i < numberOfItems; i++)
            {
                var value = new object();
                values.Add(value);
                lastNode = _target.AddAfter(lastNode, 3, value);
                nodes.Add(lastNode);
            }

            for (int i = 0; i < numberOfItems; i++)
            {
                _target.Remove(nodes[i]);
            }

            var currentNode = _target.TopLeft;
            while(currentNode != null && currentNode != _target.BottomLeft)
            {
                Assert.AreEqual(0, currentNode.ItemsInNode);
                Assert.AreEqual(null, currentNode.Next);
                currentNode = currentNode.Below;
            }

            Assert.AreEqual(_target.BottomLeft, currentNode);
        }

        [Test]
        public void Clear_Always_RemovesAll()
        {
            CreateThreeLevelTree(1);

            var originalBottomLeft = _target.BottomLeft;
            
            _target.Clear();
            
            var newBottomLeft = _target.BottomLeft;

            Assert.AreSame(originalBottomLeft, _target.BottomLeft);
            Assert.AreSame(originalBottomLeft, _target.TopLeft);
            Assert.IsNull(_target.TopLeft.Below);
            Assert.IsNull(_target.TopLeft.Above);
            Assert.IsNull(_target.TopLeft.Next);
            Assert.IsNull(_target.TopLeft.Previous);
            Assert.AreEqual(_target.TopLeft.ItemsInNode, 0);
        }

        [Test]
        public void GetIndex_FirstLeafInTree_ReturnsCorrectIndex()
        {
            CreateThreeLevelTree(3);

            int index = _target.GetIndex(_leaves[0]);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void GetIndex_MiddleLeafInTree_ReturnsCorrectIndex()
        {
            CreateThreeLevelTree(3);

            int index = _target.GetIndex(_leaves[2]);
            Assert.AreEqual(6, index);
        }

        [Test]
        public void GetLeafByNodeIndex_FirstLeaf_ReturnsCorrectLeaf()
        {
            CreateThreeLevelTree(3);

            var nodeFound = _target.GetLeafByNodeIndex(0);
            Assert.AreEqual(_leaves[0], nodeFound);
        }

        [Test]
        public void GetLeafByNodeIndex_MiddleLeaf_ReturnsCorrectLeaf()
        {
            CreateThreeLevelTree(3);

            var nodeFound = _target.GetLeafByNodeIndex(3);
            Assert.AreEqual(_leaves[3], nodeFound);
        }
    }
}
