using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.Expressions;
using System.Linq.Expressions;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ClosedToOpenExpressionTransformerTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Transform_AlreadyOpen_ReturnsOriginalExpression()
        {
            Expression<Func<Person, string>> original = person => person.Name;
            var target = new ClosedToOpenExpressionTransformer(original);

            Assert.AreSame(original, target.OpenVersion);
        }

        [Test]
        public void WasAlreadyOpen_OriginalOpen_True()
        {
            Expression<Func<Person, string>> original = person => person.Name;
            var target = new ClosedToOpenExpressionTransformer(original);

            Assert.IsTrue(target.WasAlreadyOpen);
        }
#if !SILVERLIGHT
        [Test]
        public void WasAlreadyStatic_OriginalClosed_False()
        {
            float closedValue = 5.8f;
            Expression<Func<Person, bool>> original = person => person.Age == closedValue;
            var target = new ClosedToOpenExpressionTransformer(original);

            Assert.IsFalse(target.WasAlreadyOpen);
        }
#endif

#if !SILVERLIGHT
        [Test]
        public void OpenVersion_OriginalHasClosedVariable_OpenVersionHasTransformRootExpression()
        {
            float closedValue = 5.8f;
            Expression<Func<Person, bool>> original = person => person.Age == closedValue;

            var target = new ClosedToOpenExpressionTransformer(original);

            Assert.AreEqual(2, target.OpenVersion.Parameters.Count);

            //Can't really test that the static version of the function takes the compiler generated closure...
            //Assert.AreEqual(typeof(ContinuousLinq.UnitTests.ClosureToStaticExpressionTransformerTest+<>c__DisplayClass0), target.StaticVersion.Parameters[0].Type);
            Assert.AreEqual(typeof(Person), target.OpenVersion.Parameters[1].Type);
            Assert.AreEqual(typeof(Func<,,>), target.OpenVersion.Type.GetGenericTypeDefinition());
        }
#endif
        [Test]
        public void OpenVersion_OriginalIsOpenAndHasConstantDefinedInExpression_DoesNotTransform()
        {
            Expression<Func<Person, bool>> original = person => person.Age == 5.84f;

            var target = new ClosedToOpenExpressionTransformer(original);

            Assert.AreSame(original, target.OpenVersion);
        }

        [Test]
        [Ignore("Performance metrics")]
        public void ComparePerformanceOfExpressionCompileVersusCreateMetaClosure()
        {
            const int ITERATIONS = 300000;

            int xx = 2;
            System.Linq.Expressions.Expression<Func<int, int, int>> multiplierByClosureConstantExpression = (y, z) => xx * (y + z);
            DateTime start;
            TimeSpan duration;

            start = DateTime.Now;

            //for (int i = 0; i < ITERATIONS; i++)
            //{
            //    multiplierByClosureConstantExpression.Compile();
            //}

            duration = DateTime.Now - start;
            
            //Console.WriteLine(duration);

            ClosedToOpenExpressionTransformer target = null;
            
            start = DateTime.Now;

            Type[] types = new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) };
            for (int i = 0; i < ITERATIONS; i++)
            {
                target = new ClosedToOpenExpressionTransformer(multiplierByClosureConstantExpression);
                //Type specificFunctionType = typeof(Func<,,,>).MakeGenericType(types);
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration);
            Console.WriteLine(target);
        }
    }
}
