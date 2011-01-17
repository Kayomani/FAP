using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.Expressions;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ClosureToStaticExpressionFactoryTest
    {
        [Test]
        public void CreateMetaClosure_OneClosureArgument_WrapsFunctionAndEvaluatesToCorrectResult()
        {
            Func<int, int, int, int> multiplierByClosureConstant = (x, y, z) => x * (y + z);

            List<object> constants = new List<object>() { 2 };
            Func<int, int, int> partialFunction = ClosedToOpenExpressionFactory.CreateMetaClosure<Func<int, int, int>>(
                            multiplierByClosureConstant,
                            constants);

            int result = partialFunction(3, 5);
            Assert.AreEqual(16, result);
        }

        [Test]
        [Ignore("Performance metrics")]
        public void ComparePerformanceOfExpressionCompileVersusCreateMetaClosure()
        {
            int xx = 2;
            System.Linq.Expressions.Expression<Func<int, int, int>> multiplierByClosureConstantExpression = (y, z) => xx * (y + z);
            DateTime start;
            TimeSpan duration;

            start = DateTime.Now;
            for (int i = 0; i < 100000; i++)
            {
                multiplierByClosureConstantExpression.Compile();
            }
            duration = DateTime.Now - start;
            Console.WriteLine(duration);

            Func<int, int, int, int> multiplierByClosureConstant = (x, y, z) => x * (y + z);

            start = DateTime.Now;
            for (int i = 0; i < 100000; i++)
            {
                List<object> constants = new List<object>() { 2 };
                Func<int, int, int> curriedFunction = ClosedToOpenExpressionFactory.CreateMetaClosure<Func<int, int, int>>(
                                multiplierByClosureConstant,
                                constants);
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration);
        }
    }
}
