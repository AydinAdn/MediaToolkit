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
            [SetUp]
            public void SetUp()
            {
                this.CollectionUnderTest = new[] { "Foo", "Bar" };
            }

            public IEnumerable<string> CollectionUnderTest;

            [Test]
            public void Will_Iterate_Through_EachItem_InCollection()
            {
                int expectedIterations = 2;
                int iterations = 0;

                this.CollectionUnderTest.ForEach(item => iterations++);

                Assert.That(iterations == expectedIterations);
            }

            [Test]
            public void When_ActionIsNull_Throw_ArgumentNullException()
            {
                Type expectedException = typeof (ArgumentNullException);

                TestDelegate codeUnderTest = () => this.CollectionUnderTest.ForEach(null);

                Assert.Throws(expectedException, codeUnderTest);
            }
        }
    }
}
