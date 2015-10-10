using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabang.Collection;

namespace Gabang.Test
{
    class TestTree : IEnumerable<ITreeNode<string>>
    {
        ITreeNode<string> _root;

        public string Expectation = "root,0,1,100,2,200,201";

        public TestTree()
        {
            _root = CreateTestTree();
        }

        public IEnumerator<ITreeNode<string>> GetEnumerator()
        {
            return new TreeEnumerator<string>(_root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static ITreeNode<string> CreateTestTree()
        {
            var root = new TestTreeNode("root");

            for (int i = 0; i < 3; i++)
            {
                var child = new TestTreeNode(i.ToString());
                for (int j = 0; j < i; j++)
                {
                    child.Children.Add(new TestTreeNode((100 * i + j).ToString()));
                }
                root.Children.Add(child);
            }

            return root;
        }
    }
}
