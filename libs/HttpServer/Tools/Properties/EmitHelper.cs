using System;
using System.Reflection;
using System.Reflection.Emit;

namespace HttpServer.Tools.Properties
{
    /// <summary>
    /// Credits and description: http://theinstructionlimit.com/?p=76
    /// </summary>
    /// <remarks>
    /// Converted to .Net 2.0
    /// </remarks>
    static class EmitHelper
    {
        static readonly Module Module = typeof(EmitHelper).Module;
        static readonly Type[] SingleObject = new[] { typeof(object) };
        static readonly Type[] TwoObjects = new[] { typeof(object), typeof(object) };
        //static readonly Type[] ManyObjects = new[] { typeof(object), typeof(object[]) };

        /*
        public static MethodHandler CreateMethodHandler(MethodBase method)
        {
            var dynam = new DynamicMethod(string.Empty, typeof(object), ManyObjects, Module, true);
            ILGenerator il = dynam.GetILGenerator();

            ParameterInfo[] args = method.GetParameters();

            Label argsOK = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Ldc_I4, args.Length);
            il.Emit(OpCodes.Beq, argsOK);

            il.Emit(OpCodes.Newobj, typeof(TargetParameterCountException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(argsOK);

            il.PushInstance(method.DeclaringType);

            for (int i = 0; i < args.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);

                il.UnboxIfNeeded(args[i].ParameterType);
            }

            if (method.IsConstructor)
                il.Emit(OpCodes.Newobj, method as ConstructorInfo);
            else if (method.IsFinal || !method.IsVirtual)
                il.Emit(OpCodes.Call, method as MethodInfo);
            else
                il.Emit(OpCodes.Callvirt, method as MethodInfo);

            Type returnType = method.IsConstructor ? method.DeclaringType : (method as MethodInfo).ReturnType;
            if (returnType != typeof(void))
                il.BoxIfNeeded(returnType);
            else
                il.Emit(OpCodes.Ldnull);

            il.Emit(OpCodes.Ret);

            return (MethodHandler)dynam.CreateDelegate(typeof(MethodHandler));
        }
        */
        public static Action<object, object> CreateFieldSetterHandler(FieldInfo fieldInfo)
        {
            var dynam = new DynamicMethod(string.Empty, typeof(void), TwoObjects, Module, true);
            ILGenerator il = dynam.GetILGenerator();

            if (!fieldInfo.IsStatic)
                PushInstance(il, fieldInfo.DeclaringType);

            il.Emit(OpCodes.Ldarg_1);
            UnboxIfNeeded(il, fieldInfo.FieldType);
            il.Emit(OpCodes.Stfld, fieldInfo);
            il.Emit(OpCodes.Ret);

            return (Action<object, object>)dynam.CreateDelegate(typeof(Action<object, object>));
        }

        public static Action<object, object> CreatePropertySetterHandler(PropertyInfo propertyInfo)
        {
            var dynam = new DynamicMethod(string.Empty, typeof(void), TwoObjects, Module, true);
            ILGenerator il = dynam.GetILGenerator();
            MethodInfo methodInfo = propertyInfo.GetSetMethod();

            if (!methodInfo.IsStatic)
                PushInstance(il, propertyInfo.DeclaringType);

            il.Emit(OpCodes.Ldarg_1);
            UnboxIfNeeded(il, propertyInfo.PropertyType);

            if (methodInfo.IsFinal || !methodInfo.IsVirtual)
                il.Emit(OpCodes.Call, methodInfo);
            else
                il.Emit(OpCodes.Callvirt, methodInfo);
            il.Emit(OpCodes.Ret);

            return (Action<object, object>)dynam.CreateDelegate(typeof(Action<object, object>));
        }

        public static Func<object, object> CreateFieldGetterHandler(FieldInfo fieldInfo)
        {
            var dynam = new DynamicMethod(string.Empty, typeof(object), SingleObject, Module, true);
            ILGenerator il = dynam.GetILGenerator();

            if (!fieldInfo.IsStatic)
                PushInstance(il, fieldInfo.DeclaringType);

            il.Emit(OpCodes.Ldfld, fieldInfo);
            BoxIfNeeded(il, fieldInfo.FieldType);
            il.Emit(OpCodes.Ret);

            return (Func<object, object>)dynam.CreateDelegate(typeof(Func<object, object>));
        }

        public static Func<object, object> CreatePropertyGetterHandler(PropertyInfo propertyInfo)
        {
            var dynam = new DynamicMethod(string.Empty, typeof(object), SingleObject, Module, true);
            ILGenerator il = dynam.GetILGenerator();
            MethodInfo methodInfo = propertyInfo.GetGetMethod();

            if (!methodInfo.IsStatic)
                PushInstance(il, propertyInfo.DeclaringType);

            if (methodInfo.IsFinal || !methodInfo.IsVirtual)
                il.Emit(OpCodes.Call, methodInfo);
            else
                il.Emit(OpCodes.Callvirt, methodInfo);

            BoxIfNeeded(il, propertyInfo.PropertyType);
            il.Emit(OpCodes.Ret);

            return (Func<object, object>)dynam.CreateDelegate(typeof(Func<object, object>));
        }

        public static Func<object> CreateParameterlessConstructorHandler(Type type)
        {
            var dynam = new DynamicMethod(string.Empty, typeof(object), Type.EmptyTypes, Module, true);
            ILGenerator il = dynam.GetILGenerator();

            if (type.IsValueType)
            {
                il.DeclareLocal(type);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Box, type);
            }
            else
                il.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));

            il.Emit(OpCodes.Ret);

            return (Func<object>)dynam.CreateDelegate(typeof(Func<object>));
        }

        #region Private Helpers

        static void PushInstance(ILGenerator il, Type type)
        {
            il.Emit(OpCodes.Ldarg_0);
            if (type.IsValueType)
                il.Emit(OpCodes.Unbox, type);
        }

        static void BoxIfNeeded(ILGenerator il, Type type)
        {
            if (type.IsValueType)
                il.Emit(OpCodes.Box, type);
        }

        static void UnboxIfNeeded(ILGenerator il, Type type)
        {
            if (type.IsValueType)
                il.Emit(OpCodes.Unbox_Any, type);
        }

        #endregion
    }

    internal delegate void Action<T1, T2>(T1 value1, T2 value2);

    internal delegate TResult Func<T1, TResult>(T1 value1);
    internal delegate TResult Func<TResult>();
    
}
