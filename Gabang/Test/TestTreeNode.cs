using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabang.Collection;

namespace Gabang.Test
{
    class TestTreeNode : ITreeNode<string>
    {
        public TestTreeNode(string value)
        {
            Value = value;
            Children = new List<TestTreeNode>();
        }

        public string Value { get; set; }

        public List<TestTreeNode> Children { get; set; }

        IEnumerable<ITreeNode<string>> ITreeNode<string>.Children
        {
            get
            {
                return Children;
            }
        }
    }
}
