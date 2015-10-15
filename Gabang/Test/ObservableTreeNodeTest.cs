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
        [TestMethod]
        public void ObservableTreeNodeConstructorTest()
        {
            var target = new ObservableTreeNode<int>(1234);
            Assert.AreEqual(true, target.CanHaveChildren, "Default CanHaveChildren value");
            Assert.AreEqual(1234, target.Value);
            Assert.AreEqual(1, target.TotalNodeCount);
            Assert.AreEqual(0, target.Children.Count);
        }

        [TestMethod]
        public void ObservableTreeNodeAddChildTest()
        {
            RunLinearizeTest((target) =>
            {
                target.InsertChildAt(0, GetTestTree());

                int[] expected = { 0, 1, 11, 111, 112, 12, 121, 122, 13, 131, 1311, 1312, 132 };
                AssertLinearized(expected, _linearized, target);
            });
        }

        [TestMethod]
        public void ObservableTreeNodeRemoveChildTest()
        {
            RunLinearizeTest((target) =>
            {
                target.InsertChildAt(0, GetTestTree());

                target.Children[0].Children[2].RemoveChild(0);

                int[] expected = { 0, 1, 11, 111, 112, 12, 121, 122, 13, 132 };
                AssertLinearized(expected, _linearized, target);
            });
        }

        [TestMethod]
        public void AddLeafChildTest()
        {
            RunLinearizeTest((target) =>
            {
                target.AddChild(new ObservableTreeNode<int>(10));
                target.AddChild(new ObservableTreeNode<int>(11));
                target.AddChild(new ObservableTreeNode<int>(12));

                int[] expected = { 0, 10, 11, 12 };
                AssertLinearized(expected, _linearized, target);
            });
        }

        [TestMethod]
        public void AddChildTest()
        {
            RunLinearizeTest((target) =>
            {
                target.AddChild(new ObservableTreeNode<int>(10));

                var added = new ObservableTreeNode<int>(11);
                added.AddChild(new ObservableTreeNode<int>(111));
                added.AddChild(new ObservableTreeNode<int>(112));

                target.AddChild(added);
                target.AddChild(new ObservableTreeNode<int>(12));

                int[] expected = { 0, 10, 11, 111, 112, 12 };

                AssertLinearized(expected, _linearized, target);
            });
        }

        [TestMethod]
        public void InsertChildOrderTest()
        {
            RunLinearizeTest((target) =>
            {
                target.InsertChildAt(0, new ObservableTreeNode<int>(12));
                target.InsertChildAt(0, new ObservableTreeNode<int>(10));
                target.InsertChildAt(1, new ObservableTreeNode<int>(11));

                int[] expected = { 0, 10, 11, 12 };
                AssertLinearized(expected, _linearized, target);
            });
        }

        [TestMethod]
        public void InsertAnoterTreeTest()
        {
            RunLinearizeTest((target) =>
            {
                target.InsertChildAt(0, new ObservableTreeNode<int>(12));

                target.InsertChildAt(0, new ObservableTreeNode<int>(10));

                var added = new ObservableTreeNode<int>(11);
                added.AddChild(new ObservableTreeNode<int>(111));
                added.AddChild(new ObservableTreeNode<int>(112));

                target.InsertChildAt(1, added);

                int[] expected = { 0, 10, 11, 111, 112, 12 };
                AssertLinearized(expected, _linearized, target);
            });
        }

        [TestMethod]
        public void InsertChildTest()
        {
            RunLinearizeTest((target) =>
            {
                target.InsertChildAt(0, new ObservableTreeNode<int>(10));
                target.InsertChildAt(1, new ObservableTreeNode<int>(11));
                target.InsertChildAt(2, new ObservableTreeNode<int>(12));

                target.Children[1].InsertChildAt(0, new ObservableTreeNode<int>(111));

                int[] expected = { 0, 10, 11, 111, 12 };
                AssertLinearized(expected, _linearized, target);
            });
        }

        [TestMethod]
        public void RemoveLeafChildTest()
        {
            RunLinearizeTest((target) =>
            {
                target.InsertChildAt(0, new ObservableTreeNode<int>(10));
                target.InsertChildAt(1, new ObservableTreeNode<int>(11));
                target.InsertChildAt(2, new ObservableTreeNode<int>(12));

                target.RemoveChild(1);

                int[] expected = { 0, 10, 12 };
                AssertLinearized(expected, _linearized, target);
            });
        }

        private void AssertLinearized(int[] expected, IList<int> target, ObservableTreeNode<int> targetTree)
        {
            Assert.AreEqual(expected.Length, targetTree.TotalNodeCount);
            Assert.AreEqual(expected.Length, target.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], target[i], string.Format("{0}th item is different", i));
            }
        }

        private void RunLinearizeTest(Action<ObservableTreeNode<int>> testAction)
        {
            _linearized.Clear();
            var target = new ObservableTreeNode<int>(0, true);
            target.CollectionChanged += Target_CollectionChanged;

            _linearized.Add(target.Value);

            try
            {
                testAction(target);
            }
            finally
            {
                target.CollectionChanged -= Target_CollectionChanged;
            }
        }

        private List<int> _linearized = new List<int>();
        private void Target_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int insertIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems)
                    {
                        _linearized.Insert(insertIndex, (int)item);
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

        private ObservableTreeNode<int> GetTestTree()
        {
            var n1 = new ObservableTreeNode<int>(11);
            var n11 = new ObservableTreeNode<int>(111, false);
            var n12 = new ObservableTreeNode<int>(112, false);
            n1.InsertChildAt(0, n11);
            n1.InsertChildAt(1, n12);

            var n2 = new ObservableTreeNode<int>(12);
            var n21 = new ObservableTreeNode<int>(121, false);
            var n22 = new ObservableTreeNode<int>(122, false);
            n2.InsertChildAt(0, n21);
            n2.InsertChildAt(1, n22);

            var n3 = new ObservableTreeNode<int>(13);

            var n311 = new ObservableTreeNode<int>(1311, false);
            var n312 = new ObservableTreeNode<int>(1312, false);
            var n31 = new ObservableTreeNode<int>(131, true);
            n31.InsertChildAt(0, n311);
            n31.InsertChildAt(1, n312);

            var n32 = new ObservableTreeNode<int>(132, false);
            n3.InsertChildAt(0, n31);
            n3.InsertChildAt(1, n32);

            var n = new ObservableTreeNode<int>(1);
            n.InsertChildAt(0, n1);
            n.InsertChildAt(1, n2);
            n.InsertChildAt(2, n3);

            return n;
        }
    }
}
