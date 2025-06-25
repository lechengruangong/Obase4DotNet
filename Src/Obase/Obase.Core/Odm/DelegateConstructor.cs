/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：委托构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:06:03
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     委托构造器，使用指定的委托构造对象。
    /// </summary>
    public class DelegateConstructor<TObject> : InstanceConstructor
    {
        /// <summary>
        ///     构造对象的委托。
        /// </summary>
        private readonly Func<TObject> _delegate;

        /// <summary>
        ///     创建DelegateConstructor实例。
        /// </summary>
        /// <param name="delegateFunction">构造对象的委托。</param>
        public DelegateConstructor(Func<TObject> delegateFunction)
        {
            _delegate = delegateFunction;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <param name="arguments">构造函数参数。</param>
        /// <returns></returns>
        public override object Construct(object[] arguments = null)
        {
            return _delegate();
        }
    }

    /// <summary>
    ///     委托构造器，使用指定的委托构造对象。
    /// </summary>
    /// <typeparam name="T">构造参数的类型。</typeparam>
    /// <typeparam name="TObject">要构造的对象的类型。</typeparam>
    public class DelegateConstructor<T, TObject> : InstanceConstructor
    {
        /// <summary>
        ///     构造对象的委托。
        /// </summary>
        private readonly Func<T, TObject> _delegate;

        /// <summary>
        ///     创建DelegateConstructor实例。
        /// </summary>
        /// <param name="delegate">构造对象的委托。</param>
        public DelegateConstructor(Func<T, TObject> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments"></param>
        public override object Construct(object[] arguments = null)
        {
            if (arguments == null || arguments.Length != 1) throw new Exception("构造对象时“参数”数量不正确");
            if (Parameters != null)
                //调用参数转换器转换参数
                for (var i = 0; i < arguments.Length; i++)
                    //如果参数有转换器则使用转换器转换，否则使用默认转换
                    if (Parameters[i] != null && Parameters[i].ValueConverter != null)
                        arguments[i] = Parameters[i].ValueConverter.Invoke(arguments[i]);
                    else
                        arguments[i] = DefaultConvert(Parameters[i].GetType(), arguments[i]);
            //如果参数为null则使用默认值
            if (arguments[0] == null)
                arguments[0] = default(T);

            return _delegate((T)arguments[0]);
        }
    }

    /// <summary>
    ///     委托构造器，使用指定的委托构造对象。
    /// </summary>
    /// <typeparam name="T1">第一个构造参数的类型。</typeparam>
    /// <typeparam name="T2">第二个构造参数的类型。</typeparam>
    /// <typeparam name="TObject">要构造的对象的类型。</typeparam>
    public class DelegateConstructor<T1, T2, TObject> : InstanceConstructor
    {
        /// <summary>
        ///     构造对象的委托。
        /// </summary>
        private readonly Func<T1, T2, TObject> _delegate;

        /// <summary>
        ///     创建DelegateConstructor实例。
        /// </summary>
        /// <param name="delegate">构造对象的委托。</param>
        public DelegateConstructor(Func<T1, T2, TObject> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments"></param>
        public override object Construct(object[] arguments = null)
        {
            if (arguments == null || arguments.Length < 2) throw new Exception("构造对象时“参数”数量不正确");
            if (Parameters != null)
                //调用参数转换器转换参数
                for (var i = 0; i < arguments.Length; i++)
                    //如果参数有转换器则使用转换器转换，否则使用默认转换
                    if (Parameters[i] != null && Parameters[i].ValueConverter != null)
                        arguments[i] = Parameters[i].ValueConverter.Invoke(arguments[i]);
                    else
                        arguments[i] = DefaultConvert(Parameters[i].GetType(), arguments[i]);

            //如果参数为null则使用默认值
            if (arguments[0] == null)
                arguments[0] = default(T1);

            if (arguments[1] == null)
                arguments[1] = default(T2);

            return _delegate((T1)arguments[0], (T2)arguments[1]);
        }
    }


    /// <summary>
    ///     委托构造器，使用指定的委托构造对象。
    /// </summary>
    /// <typeparam name="T1">第一个构造参数的类型。</typeparam>
    /// <typeparam name="T2">第二个构造参数的类型。</typeparam>
    /// <typeparam name="T3">第三个构造参数的类型。</typeparam>
    /// <typeparam name="TObject">要构造的对象的类型。</typeparam>
    public class DelegateConstructor<T1, T2, T3, TObject> : InstanceConstructor
    {
        /// <summary>
        ///     构造对象的委托。
        /// </summary>
        private readonly Func<T1, T2, T3, TObject> _delegate;

        /// <summary>
        ///     创建DelegateConstructor实例。
        /// </summary>
        /// <param name="delegate">构造对象的委托。</param>
        public DelegateConstructor(Func<T1, T2, T3, TObject> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments"></param>
        public override object Construct(object[] arguments = null)
        {
            if (arguments == null || arguments.Length < 3) throw new Exception("构造对象时“参数”数量不正确");

            if (Parameters != null)
                //调用参数转换器转换参数
                for (var i = 0; i < arguments.Length; i++)
                    //如果参数有转换器则使用转换器转换，否则使用默认转换
                    if (Parameters[i] != null && Parameters[i].ValueConverter != null)
                        arguments[i] = Parameters[i].ValueConverter.Invoke(arguments[i]);
                    else
                        arguments[i] = DefaultConvert(Parameters[i].GetType(), arguments[i]);

            //如果参数为null则使用默认值
            if (arguments[0] == null)
                arguments[0] = default(T1);

            if (arguments[1] == null)
                arguments[1] = default(T2);

            if (arguments[2] == null)
                arguments[2] = default(T3);

            return _delegate((T1)arguments[0], (T2)arguments[1], (T3)arguments[2]);
        }
    }


    /// <summary>
    ///     委托构造器，使用指定的委托构造对象。
    /// </summary>
    /// <typeparam name="T1">第一个构造参数的类型。</typeparam>
    /// <typeparam name="T2">第二个构造参数的类型。</typeparam>
    /// <typeparam name="T3">第三个构造参数的类型。</typeparam>
    /// <typeparam name="T4">第四个构造参数的类型。</typeparam>
    /// <typeparam name="TObject">要构造的对象的类型。</typeparam>
    public class DelegateConstructor<T1, T2, T3, T4, TObject> : InstanceConstructor
    {
        /// <summary>
        ///     构造对象的委托。
        /// </summary>
        private readonly Func<T1, T2, T3, T4, TObject> _delegate;

        /// <summary>
        ///     创建DelegateConstructor实例。
        /// </summary>
        /// <param name="delegate">构造对象的委托。</param>
        public DelegateConstructor(Func<T1, T2, T3, T4, TObject> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments"></param>
        public override object Construct(object[] arguments = null)
        {
            if (arguments == null || arguments.Length < 4) throw new Exception("构造对象时“参数”数量不正确");
            if (Parameters != null)
                //调用参数转换器转换参数
                for (var i = 0; i < arguments.Length; i++)
                    //如果参数有转换器则使用转换器转换，否则使用默认转换
                    if (Parameters[i] != null && Parameters[i].ValueConverter != null)
                        arguments[i] = Parameters[i].ValueConverter.Invoke(arguments[i]);
                    else
                        arguments[i] = DefaultConvert(Parameters[i].GetType(), arguments[i]);

            // 如果参数为null则使用默认值
            if (arguments[0] == null)
                arguments[0] = default(T1);

            if (arguments[1] == null)
                arguments[1] = default(T2);

            if (arguments[2] == null)
                arguments[2] = default(T3);

            if (arguments[3] == null)
                arguments[3] = default(T4);

            return _delegate((T1)arguments[0], (T2)arguments[1], (T3)arguments[2], (T4)arguments[3]);
        }
    }
}