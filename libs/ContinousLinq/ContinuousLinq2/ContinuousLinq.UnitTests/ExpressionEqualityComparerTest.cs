using System;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using ContinuousLinq.Expressions;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ExpressionEqualityComparerTest
    {
        private ExpressionEqualityComparer _target;

        private List<Expression<Func<Person, string>>> _nameGetters;

        [SetUp]
        public void Setup()
        {
            _target = new ExpressionEqualityComparer();
            _nameGetters = new List<Expression<Func<Person, string>>>()
            {
                person => person.Name,
                person => person.Name
            };
        }

        [Test]
        public void Equals_HaveSameContents_True()
        {
            Expression<Func<Person, string>> expressionZero = person => person.Name;
            Expression<Func<Person, string>> expressionOne = person => person.Name;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_DifferentContents_False()
        {
            Expression<Func<Person, string>> expressionZero = person => person.Name;
            Expression<Func<Person, string>> expressionOne = person => person.Brother.Name;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_BinaryExpressionSameOperators_True()
        {
            Expression<Func<Person, int>> expressionZero = person => person.Age + 1;
            Expression<Func<Person, int>> expressionOne = person => person.Age + 1;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_BinaryExpressionDifferentOperators_False()
        {
            Expression<Func<Person, int>> expressionZero = person => person.Age + 1;
            Expression<Func<Person, int>> expressionOne = person => person.Age - 1;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_BinaryExpressionSameOperands_True()
        {
            Expression<Func<Person, int>> expressionZero = person => person.Age + 1;
            Expression<Func<Person, int>> expressionOne = person => person.Age + 1;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_BinaryExpressionDifferentOperands_False()
        {
            Expression<Func<Person, int>> expressionZero = person => person.Age + 1;
            Expression<Func<Person, int>> expressionOne = person => person.Age + 2;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConstantSame_True()
        {
            Expression<Func<Person, int>> expressionZero = person => 1;
            Expression<Func<Person, int>> expressionOne = person => 1;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConstantDifferent_False()
        {
            Expression<Func<Person, int>> expressionZero = person => 1;
            Expression<Func<Person, int>> expressionOne = person => 2;
    
            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_UnaryExpressionSameOperator_True()
        {
            Expression<Func<Person, int>> expressionZero = person => -person.Age;
            Expression<Func<Person, int>> expressionOne = person => -person.Age;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_UnaryExpressionDifferentOperator_False()
        {
            Expression<Func<Person, int>> expressionZero = person => +person.Age;
            Expression<Func<Person, int>> expressionOne = person => -person.Age;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_UnaryExpressionSameOperand_True()
        {
            Expression<Func<Person, int>> expressionZero = person => -person.Age;
            Expression<Func<Person, int>> expressionOne = person => -person.Age;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_UnaryExpressionDifferentOperand_False()
        {
            Expression<Func<Person, int>> expressionZero = person => -person.Age;
            Expression<Func<Person, int>> expressionOne = person => -person.Brother.Age;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_MemberCallExpressionSameMember_True()
        {
            Expression<Func<Person, int>> expressionZero = person => person.SubtractYearsFromAge(1);
            Expression<Func<Person, int>> expressionOne = person => person.SubtractYearsFromAge(1);

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_MemberCallExpressionDifferentMembers_False()
        {
            Expression<Func<Person, int>> expressionZero = person => person.SubtractYearsFromAge(1);
            Expression<Func<Person, int>> expressionOne = person => person.AddYearsToAge(1);
            
            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_MemberCallExpressionSameArguments_True()
        {
            Expression<Func<Person, int>> expressionZero = person => person.AddYearsToAge(1);
            Expression<Func<Person, int>> expressionOne = person => person.AddYearsToAge(1);

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_MemberCallExpressionDifferentArguments_False()
        {
            Expression<Func<Person, int>> expressionZero = person => person.AddYearsToAge(1);
            Expression<Func<Person, int>> expressionOne = person => person.AddYearsToAge(2);
            
            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConditionalExpressionSameTest_True()
        {
            Expression<Func<Person, int>> expressionZero = person => true ? 1 : 2;
            Expression<Func<Person, int>> expressionOne = person => true ? 1 : 2;
            
            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConditionalExpressionDifferentTest_False()
        {
            Expression<Func<Person, int>> expressionZero = person => false ? 1 : 2;
            Expression<Func<Person, int>> expressionOne = person => true ? 1 : 2;
            
            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConditionalExpressionSameIfTrue_True()
        {
            Expression<Func<Person, int>> expressionZero = person => true ? 1 : 2;
            Expression<Func<Person, int>> expressionOne = person => true ? 1 : 2;
            
            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConditionalExpressionDifferentIfTrue_False()
        {
            Expression<Func<Person, int>> expressionZero = person => true ? 1 : 2;
            Expression<Func<Person, int>> expressionOne = person => true ? 3 : 2;
            
            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConditionalExpressionSameIfFalse_True()
        {
            bool test = true;
            Expression<Func<Person, int>> expressionZero = person => test ? 1 : 2;
            Expression<Func<Person, int>> expressionOne = person => test ? 1 : 2;
            
            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConditionalExpressionDifferentIfFalse_False()
        {
            bool test = true;
            Expression<Func<Person, int>> expressionZero = person => test ? 1 : 2;
            Expression<Func<Person, int>> expressionOne = person => test ? 1 : 3;
            
            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_InvocationExpressionSameExpression_True()
        {
            Func<Person, int> invocation = person => person.Age;

            Expression<Func<Person, int>> expressionZero = person => invocation(person);
            Expression<Func<Person, int>> expressionOne = person => invocation(person);

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_InvocationExpressionDifferentExpression_False()
        {
            Func<Person, int> invocationZero = person => person.Age;
            Expression<Func<Person, int>> expressionZero = person => invocationZero(person);

            Func<Person, int> invocationOne = person => person.Age;
            Expression<Func<Person, int>> expressionOne = person => invocationOne(person);
            
            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_InvocationExpressionSameArguments_True()
        {
            Func<int, int> invocation = value => value;

            Expression<Func<Person, int>> expressionZero = person => invocation(1);
            Expression<Func<Person, int>> expressionOne = person => invocation(1);

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_InvocationExpressionDifferentArguments_False()
        {
            Func<int, int> invocation = value => value;

            Expression<Func<Person, int>> expressionZero = person => invocation(1);
            Expression<Func<Person, int>> expressionOne = person => invocation(2);

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_MemberExpressionSameMember_True()
        {
            Func<int, int> invocation = value => value;

            Expression<Func<Person, int>> expressionZero = person => person.Age;
            Expression<Func<Person, int>> expressionOne = person => person.Age;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_MemberExpressionDifferentMember_False()
        {
            Func<int, int> invocation = value => value;

            Expression<Func<Person, int>> expressionZero = person => person.Age;
            Expression<Func<Person, string>> expressionOne = person => person.Name;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ParameterExpressionSameTypeButDifferentName_False()
        {
            Expression<Func<int, int, bool>> expressionZero = (one, two) => one == two;
            Expression<Func<int, int, bool>> expressionOne = (three, four) => three == four;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ParameterExpressionDifferentType_False()
        {
            Expression<Func<int, int, bool>> expressionZero = (one, two) => one == two;
            Expression<Func<int, double, bool>> expressionOne = (three, four) => three == four;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConstantExpressionSameValue_True()
        {
            Expression<Func<int>> expressionZero = () => 1;
            Expression<Func<int>> expressionOne = () => 1;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ConstantExpressionDifferentValue_False()
        {
            Expression<Func<int>> expressionZero = () => 1;
            Expression<Func<int>> expressionOne = () => 2;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_NewExpressionSameType_True()
        {
            Expression<Func<Person>> expressionZero = () => new Person();
            Expression<Func<Person>> expressionOne = () => new Person();

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_NewExpressionDifferentType_False()
        {
            Expression<Func<Person>> expressionZero = () => new Person();
            Expression<Func<object>> expressionOne = () => new object();

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test(Description = "Could actually support member inits in the future")]
        public void Equals_MemberInitExpressionSameInit_False()
        {
            Expression<Func<Person>> expressionZero = () => new Person()
                {
                    Name = "Bob"
                };
            Expression<Func<Person>> expressionOne = () => new Person()
                {
                    Name = "Bob"
                };

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_NewExpressionDifferentInits_False()
        {
            Expression<Func<Person>> expressionZero = () => new Person()
            {
                Name = "Bob"
            };
            Expression<Func<Person>> expressionOne = () => new Person()
            {
                Name = "James"
            };

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ClosureHavingSameTarget_True()
        {
            Person comparsionPerson = new Person();

            Expression<Func<Person, bool>> expressionZero = person => person.Name == comparsionPerson.Name;
            Expression<Func<Person, bool>> expressionOne = person => person.Name == comparsionPerson.Name;

            Assert.IsTrue(_target.Equals(expressionZero, expressionOne));
            Assert.AreEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }

        [Test]
        public void Equals_ClosureHavingDifferentTarget_False()
        {
            Person comparsionPersonZero = new Person();
            Expression<Func<Person, bool>> expressionZero = person => person.Name == comparsionPersonZero.Name;

            Person comparsionPersonOne = new Person();
            Expression<Func<Person, bool>> expressionOne = person => person.Name == comparsionPersonOne.Name;

            Assert.IsFalse(_target.Equals(expressionZero, expressionOne));
            Assert.AreNotEqual(_target.GetHashCode(expressionZero), _target.GetHashCode(expressionOne));
        }
    }
}
