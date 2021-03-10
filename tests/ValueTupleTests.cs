using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    class ValueTupleTests
    {
        [Test]
        public void Test()
        {
            (int left, double right) blah = (3, 3.0);

            Assert.AreEqual(3, blah.left);
            Assert.AreEqual(3.0, blah.right);
        }
    }
}
