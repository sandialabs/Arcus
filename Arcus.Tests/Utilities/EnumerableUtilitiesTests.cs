using System.Linq;
using Arcus.Utilities;
using NUnit.Framework;

namespace Arcus.Tests.Utilities
{
    [TestFixture]
    public class EnumerableUtilitiesTests
    {
        [Test]
        public void ReverseIfPredicateFalseTest()
        {
            // Arrange
            var enumerable = new[] {"a", "b", "c"};

            // Act
            var result = enumerable.ReverseIf(false);

            // Assert
            CollectionAssert.AreEquivalent(enumerable, result);
        }

        [Test]
        public void ReverseIfPredicateTrueTest()
        {
            // Arrange
            var enumerable = new[] {"a", "b", "c"};

            // Act
            var result = enumerable.ReverseIf(true);

            // Assert
            CollectionAssert.AreEquivalent(enumerable.Reverse(), result);
        }

        [Test]
        public void ReverseIfLambdaIsFalseTest()
        {
            //Arrange
            var enumerable = new[] {"a", "b", "c"};

            //Act
            var result = enumerable.ReverseIf(x => x.Contains("d"));

            CollectionAssert.AreEquivalent(enumerable, result);
        }

        [Test]
        public void ReverseIfLambdaIsTrueTest()
        {
            //Arrange
            var enumerable = new[] {"a", "b", "c"};

            //Act
            var result = enumerable.ReverseIf(x => x.Contains("a"));

            CollectionAssert.AreEquivalent(enumerable.Reverse(), result);
        }
    }
}
