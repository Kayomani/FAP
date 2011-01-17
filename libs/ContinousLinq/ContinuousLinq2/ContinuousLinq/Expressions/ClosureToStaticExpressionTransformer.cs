using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ContinuousLinq.Expressions
{
    public class ClosedToOpenExpressionTransformer : ExpressionVisitor
    {
        LambdaExpression _rootExpression;

        public ClosedToOpenExpressionTransformer(LambdaExpression rootExpression)
        {
            _rootExpression = rootExpression;
#if SILVERLIGHT
            this.OpenVersion = _rootExpression;
#else
            Visit(rootExpression);
#endif
        }

        public LambdaExpression OpenVersion { get; private set; }

        public bool WasAlreadyOpen
        {
            get { return _rootExpression == this.OpenVersion; }
        }

        private List<ParameterExpression> _constantParameters;
        private List<ParameterExpression> ConstantParameters
        {
            get
            {
                if (_constantParameters == null)
                    _constantParameters = new List<ParameterExpression>();

                return _constantParameters;
            }
        }

        private List<object> _constants;
        public List<object> Constants
        {
            get
            {
                if (_constants == null)
                    _constants = new List<object>();
                return _constants;
            }
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            if (lambda != _rootExpression)
            {
                return base.VisitLambda(lambda);
            }

            Expression body = this.Visit(lambda.Body);

            LambdaExpression finalOpenVersion;
            if (body != lambda.Body)
            {
                System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> lambdaParameters = lambda.Parameters;
                Type[] parameterTypes = new Type[this.ConstantParameters.Count + lambdaParameters.Count + 1];

                int lastTypeIndex = 0;

                for (int i = 0; i < this.ConstantParameters.Count; i++)
                {
                    parameterTypes[lastTypeIndex++] = this.ConstantParameters[i].Type;
                }

                for (int i = 0; i < lambda.Parameters.Count; i++)
                {
                    parameterTypes[lastTypeIndex++] = lambda.Parameters[i].Type;
                }

                parameterTypes[lastTypeIndex++] = lambda.Body.Type;

                IEnumerable<ParameterExpression> openVersionParameterList = this.ConstantParameters.Concat(lambda.Parameters);

                Type openLambdaType = GetSpecificFuncTypeFromParameters(parameterTypes);
                finalOpenVersion = Expression.Lambda(openLambdaType, body, openVersionParameterList);
            }
            else
            {
                finalOpenVersion = _rootExpression;
            }

            this.OpenVersion = finalOpenVersion;
            return finalOpenVersion;
        }

        private Type GetSpecificFuncTypeFromParameters(Type[] parameterTypesArray)
        {
            Type genericFuncType = GetGenericFuncType(parameterTypesArray.Length - 1);
            Type specificFunctionType = genericFuncType.MakeGenericType(parameterTypesArray);
            return specificFunctionType;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Type.IsPrimitive || constantExpression.Type == typeof(string))
            {
                return constantExpression;
            }

            string constantParameterName = string.Empty;
            ParameterExpression parameterExpression = Expression.Parameter(constantExpression.Type, constantParameterName);
            this.ConstantParameters.Add(parameterExpression);
            this.Constants.Add(constantExpression.Value);
            return parameterExpression;
        }

        private static Type GetGenericFuncType(int numberOfParameters)
        {
            Type matchingFuncType = null;
            switch (numberOfParameters)
            {
                case 0:
                    matchingFuncType = typeof(Func<>);
                    break;
                case 1:
                    matchingFuncType = typeof(Func<,>);
                    break;
                case 2:
                    matchingFuncType = typeof(Func<,,>);
                    break;
                case 3:
                    matchingFuncType = typeof(Func<,,,>);
                    break;
                case 4:
                    matchingFuncType = typeof(Func<,,,,>);
                    break;
                case 5:
                    matchingFuncType = typeof(Func<,,,,,>);
                    break;
                case 6:
                    matchingFuncType = typeof(Func<,,,,,,>);
                    break;
                case 7:
                    matchingFuncType = typeof(Func<,,,,,,,>);
                    break;
                case 8:
                    matchingFuncType = typeof(Func<,,,,,,,,>);
                    break;
                case 9:
                    matchingFuncType = typeof(Func<,,,,,,,,,>);
                    break;
                case 10:
                    matchingFuncType = typeof(Func<,,,,,,,,,,>);
                    break;
                case 11:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,>);
                    break;
                case 12:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,>);
                    break;
                case 13:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,,>);
                    break;
                case 14:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,,,>);
                    break;
                case 15:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,,,,>);
                    break;
                case 16:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,,,,,>);
                    break;
                case 17:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,,,,,,>);
                    break;
                case 18:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,,,,,,,>);
                    break;
                case 19:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,,,,,,,,>);
                    break;
                case 20:
                    matchingFuncType = typeof(Func<,,,,,,,,,,,,,,,,,,,,>);
                    break;
                default:
                    throw new InvalidProgramException();
            }

            return matchingFuncType;
        }
    }
}
