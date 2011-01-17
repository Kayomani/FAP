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
    public class CompiledExpressionCacheTest
    {
        [SetUp]
        public void Setup()
        {
            CompiledExpressionCache._cache.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            CompiledExpressionCache._cache.Clear();
        }

        [Test]
        public void GetCompiledExpression_ExpressionsEqual_SameDelegate()
        {
            Expression<Func<Person, string>> expressionZero = person => person.Name;
            Expression<Func<Person, string>> expressionOne = person => person.Name;

            Func<Person, string> cachedZero = CompiledExpressionCache.CachedCompile(expressionZero);
            Func<Person, string> cachedOne = CompiledExpressionCache.CachedCompile(expressionOne);

            Assert.AreEqual(cachedZero, cachedOne);
        }
#if !SILVERLIGHT
        [Test]
        public void GetCompiledExpression_ClosureInExpression_DifferentDelegate()
        {
            Person comparsionPerson = new Person();

            Expression<Func<Person, bool>> expressionZero = person => person.Name == comparsionPerson.Name;
            Expression<Func<Person, bool>> expressionOne = person => person.Name == comparsionPerson.Name;

            Func<Person, bool> cachedZero = expressionZero.CachedCompile();
            Func<Person, bool> cachedOne = expressionOne.CachedCompile();

            Assert.AreNotEqual(cachedZero, cachedOne);
        }
#endif

        [Test]
        public void GetCompiledExpressionAndExecuteBoth_ClosureInExpression_SameResult()
        {
            Person comparsonPerson = new Person("Bob", 200);

            Expression<Func<Person, bool>> expressionZero = person => person.Name == comparsonPerson.Name;
            Expression<Func<Person, bool>> expressionOne = person => person.Name == comparsonPerson.Name;

            Func<Person, bool> cachedZero = expressionZero.CachedCompile();
            Func<Person, bool> cachedOne = expressionOne.CachedCompile();

            Person personToTestAgainstZero = new Person("Bob", 123);
            Assert.IsTrue(cachedZero(personToTestAgainstZero));

            Person personToTestAgainstOne = new Person("Bob", 4564);
            Assert.IsTrue(cachedOne(personToTestAgainstOne));
        }

        [Test]
        [Ignore("Performance metrics")]
        public void ComparePerformanceOfExpressionCompileVersusCreateMetaClosure()
        {
            const int ITERATIONS = 100000;

            int xx = 2;
            System.Linq.Expressions.Expression<Func<int, int, int>> multiplierByClosureConstantExpression = (y, z) => xx * (y + z);
            DateTime start;
            TimeSpan duration;

            //start = DateTime.Now;

            //for (int i = 0; i < ITERATIONS; i++)
            //{
            //    multiplierByClosureConstantExpression.Compile();
            //}

            //duration = DateTime.Now - start;
            //Console.WriteLine(duration);

            start = DateTime.Now;
            for (int i = 0; i < ITERATIONS; i++)
            {
                multiplierByClosureConstantExpression.CachedCompile();
            }

            duration = DateTime.Now - start;
            Console.WriteLine(duration);
        }


        [Test]
        [Ignore("Performance metrics")]
        public void ComparePerformanceOfExpressionCompileVersusOpenCachedCompile()
        {
            const int ITERATIONS = 100000;

            System.Linq.Expressions.Expression<Func<int, int, int>> multiplierByOpenConstantExpression = (y, z) => y + z;

            DateTime start;
            TimeSpan duration;

            start = DateTime.Now;

            for (int i = 0; i < ITERATIONS; i++)
            {
                multiplierByOpenConstantExpression.Compile();
            }

            duration = DateTime.Now - start;
            Console.WriteLine(PerSecond(ITERATIONS, duration));

            start = DateTime.Now;
            for (int i = 0; i < ITERATIONS; i++)
            {
                multiplierByOpenConstantExpression.CachedCompile();
            }

            duration = DateTime.Now - start;
            Console.WriteLine(PerSecond(ITERATIONS, duration));
        }

        private static double PerSecond(int count, TimeSpan duration)
        {
            return count / duration.TotalSeconds;
        }

    }
}
