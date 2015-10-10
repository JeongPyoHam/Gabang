using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Gabang.Collection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gabang.Test
{
    [TestClass]
    public class TreeEnumeratorTest
    {
        [TestMethod]
        public void TreeEnumeratorForeachTest()
        {
            List<string> output = new List<string>();
            var tree = new TestTree();

            foreach (var node in tree)
            {
                output.Add(node.Value);
            }
            string target = string.Join(",", output);

            Assert.AreEqual(tree.Expectation, target);
        }
    }




    
}
