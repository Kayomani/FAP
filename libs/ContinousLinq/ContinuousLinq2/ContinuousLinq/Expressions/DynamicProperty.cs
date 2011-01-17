using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace ContinuousLinq.Expressions
{
    public class DynamicProperty
    {
        #region Fields

        private static readonly Dictionary<PropertyInfo, Func<object, object>> _getterDelegateCache;

        private static readonly Dictionary<PropertyInfo, DynamicProperty> _dynamicPropertyCache;

        private readonly PropertyInfo _property;

        private Func<object, object> _getterDelegate;

        #endregion

        #region Constructor

        static DynamicProperty()
        {
            _getterDelegateCache = new Dictionary<PropertyInfo, Func<object, object>>();
            _dynamicPropertyCache = new Dictionary<PropertyInfo, DynamicProperty>();
        }

        public DynamicProperty(PropertyInfo property)
        {
            _property = property;
            CreateDynamicGetterDelegate();
        }

        #endregion

        #region Methods

        public static DynamicProperty Create(Type sourceType, string propertyName)
        {
            PropertyInfo propertyInfo = sourceType.GetProperty(propertyName);

            return Create(propertyInfo);
        }

        public static DynamicProperty Create(PropertyInfo propertyInfo)
        {
            DynamicProperty cached;

            lock (_dynamicPropertyCache)
            {
                if (!_dynamicPropertyCache.TryGetValue(propertyInfo, out cached))
                {
                    cached = new DynamicProperty(propertyInfo);
                    _dynamicPropertyCache.Add(propertyInfo, cached);
                }
            }
            return cached;
        }

        public object GetValue(object obj)
        {
            return _getterDelegate(obj);
        }

        private void CreateDynamicGetterDelegate()
        {
            lock (_getterDelegateCache)
            {
                if (_getterDelegateCache.TryGetValue(_property, out _getterDelegate))
                {
                    return;
                }

                MethodInfo getterMethodInfo = _property.GetGetMethod();

                if (getterMethodInfo == null)
                    throw new InvalidOperationException("No getter method found on property");

                DynamicMethod dynamicMethod = new DynamicMethod(
                    string.Empty,
                    typeof(object),
                    new[] { typeof(object) });

                ILGenerator il = dynamicMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, _property.DeclaringType);

                il.EmitCall(OpCodes.Callvirt, getterMethodInfo, null);

                if (_property.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Box, _property.PropertyType);
                }

                il.Emit(OpCodes.Ret);

                _getterDelegate = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));


                _getterDelegateCache.Add(_property, _getterDelegate);
            }
        }

        internal static void ClearCaches()
        {
            _dynamicPropertyCache.Clear();
            _getterDelegateCache.Clear();
        }
        #endregion
    }
}
