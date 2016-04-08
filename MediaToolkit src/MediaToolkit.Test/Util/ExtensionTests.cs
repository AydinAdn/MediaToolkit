using System;
using System.Collections.Generic;
using MediaToolkit.Util;
using Moq;
using NUnit.Framework;

namespace MediaToolkit.Test.Util
{
    [TestFixture]
    public class ExtensionTests
    {
        public class ForEach
        {
            [Test]
            public void Will_Iterate_Through_EachItem_InCollection()
            {
                IEnumerable<string> collectionUnderTest = new[] { "Foo", "Bar" };
                int expectedIterations = 2;
                int iterations = 0;

                collectionUnderTest.ForEach(item => iterations++);

                Assert.That(iterations == expectedIterations);
            }
        }
    }
}
