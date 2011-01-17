using System.ComponentModel;
using ContinuousLinq.Aggregates;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ContinuousAggregationExtensionTest
    {
        private ContinuousCollection<PropertyChangedClass> _list;
        private ReadOnlyContinuousCollection<PropertyChangedClass> _target;

        [SetUp]
        public void Initialize()
        {
            _list = new ContinuousCollection<PropertyChangedClass>
                          {
                              new PropertyChangedClass { DecimalTargetValue = 10, TargetValue = 10 },
                              new PropertyChangedClass { DecimalTargetValue = 23, TargetValue = 23 },
                              new PropertyChangedClass { DecimalTargetValue = null, TargetValue = null },
                              new PropertyChangedClass { DecimalTargetValue = 2, TargetValue = 2 },
                              new PropertyChangedClass { DecimalTargetValue = null, TargetValue = null },
                              new PropertyChangedClass { DecimalTargetValue = null, TargetValue = null },
                              new PropertyChangedClass { DecimalTargetValue = 1, TargetValue = 1 },                              
                          };
            _target = _list.AsReadOnly();
        }

        [TearDown]
        public void Cleanup()
        {
            
        }

        [Test]
        public void ContinuousSum_IfNullableDouble_SumsNullsAsNull()
        {
            var value = _target.ContinuousSum(item => item.TargetValue);
            Assert.IsNull(value.CurrentValue);
        }

        [Test]
        public void ContinuousSum_IfNullableDoubleWithAfterEffect_SumsNullsAsNull()
        {
            double? maxValue = 0;
            var value = _target.ContinuousSum(item => item.TargetValue, max => maxValue = max);
            Assert.IsNull(value.CurrentValue);
            Assert.IsNull(maxValue);
        }

        [Test]
        public void ContinuousSum_IfNullableDouble_SumsNonNullAsValue()
        {
            foreach (var item in _list)
            {
                item.TargetValue = 10;
            }
            var value = _target.ContinuousSum(item => item.TargetValue);
            Assert.AreEqual(70, value.CurrentValue);
        }

        [Test]
        public void ContinuousMax_IfNullableDouble_TreatsNullsAsZero()
        {
            var value = _target.ContinuousMax(item => item.TargetValue);
            Assert.AreEqual(23, value.CurrentValue);
        }

        [Test]
        public void ContinuousMax_IfNullableDoubleWithAfterEffect_TreatsNullsAsZero()
        {
            double maxValue = 0;
            var value = _target.ContinuousMax(item => item.TargetValue, max => maxValue = max);
            Assert.AreEqual(23, value.CurrentValue);
            Assert.AreEqual(23, maxValue);
        }

        [Test]
        public void ContinuousMax_IfNullableDecimal_TreatsNullsAsZero()
        {
            ContinuousValue<decimal> value = _target.ContinuousMax(item => item.DecimalTargetValue);
            Assert.AreEqual(23, value.CurrentValue);
        }
        
        [Test]
        public void ContinuousMin_IfNullableDouble_ReturnsMinValueThatsNotNull()
        {
            var value = _target.ContinuousMin(item => item.TargetValue);
            Assert.AreEqual(1d, value.CurrentValue);
        }

        [Test]
        public void ContinuousMin_IfNullableDecimal_ReturnsMinValueThatIsNotNull()
        {
            ContinuousValue<decimal> value = _target.ContinuousMin(item => item.DecimalTargetValue);
            Assert.AreEqual(1, value.CurrentValue);
        }

        private class PropertyChangedClass : INotifyPropertyChanged
        {
            #region Fields

            private double? _targetValue;
            private decimal? _decimalTargetValue;

            #endregion

            #region Properties

            public double? TargetValue
            {
                get { return _targetValue; }
                set
                {
                    if (value == _targetValue)
                        return;

                    _targetValue = value;

                    if (this.PropertyChanged == null)
                        return;

                    PropertyChanged(this, new PropertyChangedEventArgs("TargetValue"));
                }
            }

            public decimal? DecimalTargetValue
            {
                get { return _decimalTargetValue; }
                set
                {
                    if (value == _decimalTargetValue)
                        return;

                    _decimalTargetValue = value;

                    if (this.PropertyChanged == null)
                        return;

                    PropertyChanged(this, new PropertyChangedEventArgs("DecimalTargetValue"));
                }
            }

            #endregion

            #region Members

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion
        }
    }
}
