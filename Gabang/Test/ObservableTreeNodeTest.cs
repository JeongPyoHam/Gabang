using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GabangCollection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gabang.Test
{
    [TestClass]
    public class ObservableTreeNodeTest
    {
        private List<ObservableTreeNode> _linearized;
        private ObservableTreeNode _rootNode;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            bool notifyIndexSelf = true;
            if (TestContext.Properties.Contains("SelfInCollection"))
            {
                notifyIndexSelf = bool.Parse((string) TestContext.Properties["SelfInCollection"]);
            }
            _linearized = new List<ObservableTreeNode>();
            _rootNode = new ObservableTreeNode(0, notifyIndexSelf);
            _rootNode.CollectionChanged += Target_CollectionChanged;

            if (notifyIndexSelf)
            {
                _linearized.Add(_rootNode);
            }
        }

        [TestCleanup]
        public void CleanupTest()
        {
            _rootNode.CollectionChanged -= Target_CollectionChanged;

            _rootNode = null;
            _linearized = null;
        }

        [TestMethod]
        public void ObservableTreeNodeConstructorTest()
        {
            var target = new ObservableTreeNode(1234);
            Assert.AreEqual(false, target.HasChildren, "Default HasChildren value");
            Assert.AreEqual(1234, target.Content);
            Assert.AreEqual(1, target.TotalNodeCount);
            Assert.AreEqual(0, target.Children.Count);
        }

        [TestMethod]
        public void ObservableTreeNodeAddChildTest()
        {
            var target = _rootNode;
            target.InsertChildAt(0, GetTestTree());

            int[] expected = { 0, 1, 11, 111, 112, 12, 121, 122, 13, 131, 1311, 1312, 132 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void ObservableTreeNodeRemoveChildTest()
        {
            var target = _rootNode;
            target.InsertChildAt(0, GetTestTree());

            target.Children[0].Children[2].RemoveChild(0);

            int[] expected = { 0, 1, 11, 111, 112, 12, 121, 122, 13, 132 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void AddLeafChildTest()
        {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(10));
            target.AddChild(new ObservableTreeNode(11));
            target.AddChild(new ObservableTreeNode(12));

            int[] expected = { 0, 10, 11, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void AddChildTest()
        {
            var target = _rootNode;
            target.AddChild(new ObservableTreeNode(10));

            var added = new ObservableTreeNode(11);
            added.AddChild(new ObservableTreeNode(111));
            added.AddChild(new ObservableTreeNode(112));

            target.AddChild(added);
            target.AddChild(new ObservableTreeNode(12));

            int[] expected = { 0, 10, 11, 111, 112, 12 };

            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void InsertChildOrderTest()
        {
            var target = _rootNode;
            target.InsertChildAt(0, new ObservableTreeNode(12));
            target.InsertChildAt(0, new ObservableTreeNode(10));
            target.InsertChildAt(1, new ObservableTreeNode(11));

            int[] expected = { 0, 10, 11, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void InsertAnoterTreeTest()
        {
            var target = _rootNode;
            target.InsertChildAt(0, new ObservableTreeNode(12));

            target.InsertChildAt(0, new ObservableTreeNode(10));

            var added = new ObservableTreeNode(11);
            added.AddChild(new ObservableTreeNode(111));
            added.AddChild(new ObservableTreeNode(112));

            target.InsertChildAt(1, added);

            int[] expected = { 0, 10, 11, 111, 112, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void InsertChildTest()
        {
            var target = _rootNode;
            target.InsertChildAt(0, new ObservableTreeNode(10));
            target.InsertChildAt(1, new ObservableTreeNode(11));
            target.InsertChildAt(2, new ObservableTreeNode(12));

            target.Children[1].InsertChildAt(0, new ObservableTreeNode(111));

            int[] expected = { 0, 10, 11, 111, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        public void RemoveLeafChildTest()
        {
            var target = _rootNode;
            target.InsertChildAt(0, new ObservableTreeNode(10));
            target.InsertChildAt(1, new ObservableTreeNode(11));
            target.InsertChildAt(2, new ObservableTreeNode(12));

            target.RemoveChild(1);

            int[] expected = { 0, 10, 12 };
            AssertLinearized(expected, _linearized, target);
        }

        [TestMethod]
        [TestProperty("SelfInCollection", "false")]
        public void SelfInCollectionTest()
        {
            var target = _rootNode;

            target.AddChild(new ObservableTreeNode(10));
            target.AddChild(new ObservableTreeNode(20));
            target.AddChild(new ObservableTreeNode(30));

            target.Children[0].AddChild(new ObservableTreeNode(110));
            target.Children[1].AddChild(new ObservableTreeNode(120));
            target.Children[1].AddChild(new ObservableTreeNode(121));

            target.Children[1].RemoveChild(0);

            int[] expected = { 10, 110, 20, 121, 30 };
            AssertLinearized(expected, _linearized, target);
        }

        private void AssertLinearized(int[] expected, IList<ObservableTreeNode> target, ObservableTreeNode targetTree)
        {
            Assert.AreEqual(expected.Length, targetTree.TotalNodeCount);
            Assert.AreEqual(expected.Length, target.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], target[i].Content, string.Format("{0}th item is different", i));
            }
        }

        private void Target_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int insertIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems)
                    {
                        _linearized.Insert(insertIndex, (ObservableTreeNode)item);
                        insertIndex++;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _linearized.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _linearized.RemoveRange(1, _linearized.Count - 1);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    Assert.Fail("Not supported collection change detected");
                    break;
            }
        }

        private ObservableTreeNode GetTestTree()
        {
            var n1 = new ObservableTreeNode(11);
            var n11 = new ObservableTreeNode(111, false);
            var n12 = new ObservableTreeNode(112, false);
            n1.InsertChildAt(0, n11);
            n1.InsertChildAt(1, n12);

            var n2 = new ObservableTreeNode(12);
            var n21 = new ObservableTreeNode(121, false);
            var n22 = new ObservableTreeNode(122, false);
            n2.InsertChildAt(0, n21);
            n2.InsertChildAt(1, n22);

            var n3 = new ObservableTreeNode(13);

            var n311 = new ObservableTreeNode(1311, false);
            var n312 = new ObservableTreeNode(1312, false);
            var n31 = new ObservableTreeNode(131, true);
            n31.InsertChildAt(0, n311);
            n31.InsertChildAt(1, n312);

            var n32 = new ObservableTreeNode(132, false);
            n3.InsertChildAt(0, n31);
            n3.InsertChildAt(1, n32);

            var n = new ObservableTreeNode(1);
            n.InsertChildAt(0, n1);
            n.InsertChildAt(1, n2);
            n.InsertChildAt(2, n3);

            return n;
        }
    }
}
