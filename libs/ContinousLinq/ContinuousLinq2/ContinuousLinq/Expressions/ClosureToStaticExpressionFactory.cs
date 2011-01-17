using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ContinuousLinq.Expressions
{
    public static class ClosedToOpenExpressionFactory
    {
        private static Dictionary<Type, Dictionary<int, MethodInfo>> _partialMethodLookupTable;

        static ClosedToOpenExpressionFactory()
        {
            _partialMethodLookupTable = new Dictionary<Type, Dictionary<int, MethodInfo>>();

            var allCurryMethods = typeof(Partials).GetMethods();

            foreach (var method in allCurryMethods)
            {
                var parameters = method.GetParameters();
                if (!method.IsGenericMethod)
                    continue;

                var parameterGenericTypeDefinition = parameters[0].ParameterType.GetGenericTypeDefinition();

                Dictionary<int, MethodInfo> matchingMethods;
                if (!_partialMethodLookupTable.TryGetValue(parameterGenericTypeDefinition, out matchingMethods))
                {
                    matchingMethods = new Dictionary<int, MethodInfo>();
                    _partialMethodLookupTable.Add(parameterGenericTypeDefinition, matchingMethods);
                }
                matchingMethods.Add(parameters.Length, method);
            }
        }

        public static T CreateMetaClosure<T>(
            Delegate compiledOpenExpression,
            List<object> constants)
        {
            Type compiledOpenExpressionType = compiledOpenExpression.GetType();
            Type compiledOpenExpressionGenericTypeDefinition = compiledOpenExpressionType.GetGenericTypeDefinition();

            MethodInfo appropriatePartialMethod = _partialMethodLookupTable[compiledOpenExpressionGenericTypeDefinition][constants.Count + 1];
            MethodInfo generatedPartialMethod = appropriatePartialMethod.MakeGenericMethod(compiledOpenExpressionType.GetGenericArguments());

            object[] arguments = ConstructArgumentsForCallToPartial(compiledOpenExpression, constants);
            object partialMethod = generatedPartialMethod.Invoke(null, arguments);
            
            return (T)partialMethod;
        }

        private static object[] ConstructArgumentsForCallToPartial(Delegate compiledOpenExpression, List<object> constants)
        {
            object[] arguments = new object[constants.Count + 1];
            arguments[0] = compiledOpenExpression;
            for (int i = 0; i < constants.Count; i++)
            {
                arguments[i + 1] = constants[i];
            }
            return arguments;
        }
    }
}
