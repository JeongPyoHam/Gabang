using System;
using System.Collections.Generic;
using Gabang.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControlsUnittest {
    [TestClass]
    public class GridDataParserTest {
        private const string TestInput1 = @"structure(
  list(
    row.names = c(\\""r1\\"", \\""r2\\""),
    col.names = c(\\""a\\"", \\""b\\""),
    data = structure(
      list(
        a = c(\\""1\\"", \\""2\\""),
        b = c(\\""3\\"", \\""4\\"")
      ),
      .Names = c(\\""a\\"", \\""b\\"")
    )
  ),
  .Names = c(\\""row.names\\"", \\""col.names\\"", \\""data\\"")
)";
        [TestMethod]
        public void GridDataParser() {
            GridData data = GridParser.Parse(TestInput1);

            AssertList(new List<string>() { "r1", "r2" }, data.RowNames);
            AssertList(new List<string>() { "a", "b" }, data.ColumnNames);

            List<List<string>> values = new List<List<string>>() {
                new List<string>() { "1", "2" },
                new List<string>() { "3", "4" },
            };
            AssertMatrix(values, data.Values);
        }

        private void AssertList(List<string> expected, List<string> actual) {
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++) {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        private void AssertMatrix(List<List<string>> expected, List<List<string>> actual) {
            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++) {
                AssertList(expected[i], actual[i]);
            }
        }
    }
}
