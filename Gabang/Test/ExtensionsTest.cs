﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GabangCollection;

namespace Gabang.Test
{
    [TypeConverter(typeof(InterWrapTypeConverter))]
    class IntegerWrap
    {
        public IntegerWrap(int value)
        {
            Value = value;
            Updated = false;
        }

        public int Value { get; set; }
        public bool Updated { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Value, Updated);
        }
    }

    class InterWrapTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(int))
            {
                return true;
            }
            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            int intValue = (int)value;
            return new IntegerWrap(intValue);
        }
    }

    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void InplaceUpdateAddTest()
        {
            List<IntegerWrap> source = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(3) };
            List<IntegerWrap> update = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0 ;i < update.Count; i++)
            {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
            Assert.IsTrue(source[0].Updated);
            Assert.IsFalse(source[1].Updated);
            Assert.IsTrue(source[2].Updated);
            Assert.IsFalse(source[3].Updated);
        }

        [TestMethod]
        public void InplaceUpdateRemoveTest()
        {
            List<IntegerWrap> source = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };
            List<IntegerWrap> update = new List<IntegerWrap>() { new IntegerWrap(2), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++)
            {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
            Assert.IsTrue(source[0].Updated);
            Assert.IsTrue(source[1].Updated);
        }

        [TestMethod]
        public void InplaceUpdateMixedTest()
        {
            List<IntegerWrap> source = new List<IntegerWrap>() { new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };
            List<IntegerWrap> update = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++)
            {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
            Assert.IsFalse(source[0].Updated);
            Assert.IsTrue(source[1].Updated);
            Assert.IsTrue(source[1].Updated);
        }

        [TestMethod]
        public void InplaceUpdateRemoveAllTest()
        {
            List<IntegerWrap> source = new List<IntegerWrap>() { new IntegerWrap(1), new IntegerWrap(2), new IntegerWrap(3) };
            List<IntegerWrap> update = new List<IntegerWrap>() { };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++)
            {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
        }

        [TestMethod]
        public void InplaceUpdateAddToEmptyTest()
        {
            List<IntegerWrap> source = new List<IntegerWrap>() { };
            List<IntegerWrap> update = new List<IntegerWrap>() { new IntegerWrap(2), new IntegerWrap(3), new IntegerWrap(4) };

            source.InplaceUpdate(update, IntegerComparer, ElementUpdater);

            Assert.AreEqual(update.Count, source.Count);
            for (int i = 0; i < update.Count; i++)
            {
                Assert.IsTrue(IntegerComparer(source[i], update[i]));
            }
            Assert.IsFalse(source[0].Updated);
            Assert.IsFalse(source[1].Updated);
            Assert.IsFalse(source[2].Updated);
        }

        private bool IntegerComparer(IntegerWrap value1, IntegerWrap value2)
        {
            return value1.Value == value2.Value;
        }

        private void ElementUpdater(IntegerWrap source, IntegerWrap target)
        {
            source.Value = target.Value;
            source.Updated = true;
        }
    }
}
