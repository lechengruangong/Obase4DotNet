/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图属性求值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:14:35
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     为视图属性求值器提供基础实现。
    ///     视图属性求值器基于代理表达式计算属性的值。
    ///     代理表达式是基于属性源代理计算属性值的表达式。
    /// </summary>
    public abstract class ViewAttributeEvaluator
    {
        /// <summary>
        ///     使用代表执行代理表达式的方法的委托创建视图属性求值器。
        /// </summary>
        /// <returns>视图属性求值器实例。</returns>
        /// <param name="delegate">代表执行代理表达式的方法的委托。</param>
        /// 实施说明:
        /// 分析委托代表方法的参数个数及类型，选择相应的具体类型。动态绑定类型参数。
        public static ViewAttributeEvaluator Create(Delegate @delegate)
        {
            var parameters = @delegate.Method.GetParameters();
            Type type;
            //获取具体的求值器
            switch (parameters.Length)
            {
                case 1:
                    type = typeof(ViewAttributeEvaluator<>).MakeGenericType(parameters[0].ParameterType);
                    break;
                case 2:
                    type = typeof(ViewAttributeEvaluator<,>).MakeGenericType(parameters[0].ParameterType,
                        parameters[1].ParameterType);
                    break;
                case 3:
                    type = typeof(ViewAttributeEvaluator<,,>).MakeGenericType(parameters[0].ParameterType,
                        parameters[1].ParameterType, parameters[2].ParameterType);
                    break;
                case 4:
                    type = typeof(ViewAttributeEvaluator<,,>).MakeGenericType(parameters[0].ParameterType,
                        parameters[1].ParameterType, parameters[2].ParameterType, parameters[3].ParameterType);
                    break;
                default:
                    throw new ArgumentException("创建视图属性求值器失败。委托参数个数不支持。");
            }

            return (ViewAttributeEvaluator)Activator.CreateInstance(type, @delegate);
        }

        /// <summary>
        ///     根据属性源代理的值计算视图属性的值。
        /// </summary>
        /// <param name="agentValues">属性源代理的值构成的序列。</param>
        public abstract object Evaluate(object[] agentValues);
    }


    /// <summary>
    ///     有一个源的视图属性的求值器。
    /// </summary>
    /// <typeparam name="TAgent">属性源代理的类型。</typeparam>
    public class ViewAttributeEvaluator<TAgent> : ViewAttributeEvaluator
    {
        /// <summary>
        ///     代表执行代理表达式的方法的委托。
        /// </summary>
        private readonly Func<TAgent, object> _delegate;

        /// <summary>
        ///     创建ViewAttributeEvaluator`1'TAgent'实例。
        /// </summary>
        /// <param name="delegate">代表执行代理表达式的方法的委托。</param>
        public ViewAttributeEvaluator(Func<TAgent, object> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     根据属性源代理的值计算视图属性的值。
        /// </summary>
        /// <param name="agentValues">属性源代理的值构成的序列。</param>
        public override object Evaluate(object[] agentValues)
        {
            return _delegate((TAgent)agentValues[0]);
        }
    }


    /// <summary>
    ///     有两个源的视图属性的求值器。
    /// </summary>
    /// <typeparam name="TAgent1">第一个源的代理属性的类型。</typeparam>
    /// <typeparam name="TAgent2">第二个源的代理属性的类型。</typeparam>
    public class ViewAttributeEvaluator<TAgent1, TAgent2> : ViewAttributeEvaluator
    {
        /// <summary>
        ///     代表执行代理表达式的方法的委托。
        /// </summary>
        private readonly Func<TAgent1, TAgent2, object> _delegate;

        /// <summary>
        ///     创建ViewAttributeEvaluator`2‘TAgent1, TAgent2’实例。
        /// </summary>
        /// <param name="delegate">代表执行代理表达式的方法的委托。</param>
        public ViewAttributeEvaluator(Func<TAgent1, TAgent2, object> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     根据属性源代理的值计算视图属性的值。
        /// </summary>
        /// <param name="agentValues">属性源代理的值构成的序列。</param>
        public override object Evaluate(object[] agentValues)
        {
            return _delegate((TAgent1)agentValues[0], (TAgent2)agentValues[1]);
        }
    }


    /// <summary>
    ///     有三个源的视图属性的求值器。
    /// </summary>
    /// <typeparam name="TAgent1">第一个源的代理属性的类型。</typeparam>
    /// <typeparam name="TAgent2">第二个源的代理属性的类型。</typeparam>
    /// <typeparam name="TAgent3">第三个源的代理属性的类型。</typeparam>
    public class ViewAttributeEvaluator<TAgent1, TAgent2, TAgent3> : ViewAttributeEvaluator
    {
        /// <summary>
        ///     代表执行代理表达式的方法的委托。
        /// </summary>
        private readonly Func<TAgent1, TAgent2, TAgent3, object> _delegate;

        /// <summary>
        ///     创建ViewAttributeEvaluator`3'TAgent1, TAgent2, TAgent3'实例。
        /// </summary>
        /// <param name="delegate">代表执行代理表达式的方法的委托。</param>
        public ViewAttributeEvaluator(Func<TAgent1, TAgent2, TAgent3, object> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     根据属性源代理的值计算视图属性的值。
        /// </summary>
        /// <param name="agentValues">属性源代理的值构成的序列。</param>
        public override object Evaluate(object[] agentValues)
        {
            return _delegate((TAgent1)agentValues[0], (TAgent2)agentValues[1], (TAgent3)agentValues[2]);
        }
    }


    /// <summary>
    ///     有四个源的视图属性的求值器。
    /// </summary>
    /// <typeparam name="TAgent1">第一个源的代理属性的类型。</typeparam>
    /// <typeparam name="TAgent2">第二个源的代理属性的类型。</typeparam>
    /// <typeparam name="TAgent3">第三个源的代理属性的类型。</typeparam>
    /// <typeparam name="TAgent4">第四个源的代理属性的类型。</typeparam>
    public class ViewAttributeEvaluator<TAgent1, TAgent2, TAgent3, TAgent4> : ViewAttributeEvaluator
    {
        /// <summary>
        ///     代表执行代理表达式的方法的委托。
        /// </summary>
        private readonly Func<TAgent1, TAgent2, TAgent3, TAgent4, object> _delegate;

        /// <summary>
        ///     创建ViewAttributeEvaluator`4'TAgent1, TAgent2, TAgent3, TAgent4'实例。
        /// </summary>
        /// <param name="delegate">代表执行代理表达式的方法的委托。</param>
        public ViewAttributeEvaluator(Func<TAgent1, TAgent2, TAgent3, TAgent4, object> @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     根据属性源代理的值计算视图属性的值。
        /// </summary>
        /// <param name="agentValues">属性源代理的值构成的序列。</param>
        /// <returns></returns>
        public override object Evaluate(object[] agentValues)
        {
            return _delegate((TAgent1)agentValues[0], (TAgent2)agentValues[1], (TAgent3)agentValues[2],
                (TAgent4)agentValues[3]);
        }
    }
}