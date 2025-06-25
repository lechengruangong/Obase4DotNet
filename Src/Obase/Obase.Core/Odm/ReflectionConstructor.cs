/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于反射的对象构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:49:36
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     基于反射的对象构造器。
    /// </summary>
    public class ReflectionConstructor : InstanceConstructor
    {
        /// <summary>
        ///     类型的构造函数信息，用于反射调用。
        /// </summary>
        private readonly ConstructorInfo _constructorInfo;

        /// <summary>
        ///     创建ReflectionConstructor实例。
        /// </summary>
        /// <param name="constructorInfo">类型的构造函数信息。</param>
        public ReflectionConstructor(ConstructorInfo constructorInfo)
        {
            _constructorInfo = constructorInfo;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments"></param>
        public override object Construct(object[] arguments = null)
        {
            if (arguments != null && Parameters != null)
                //处理参数转换
                for (var i = 0; i < arguments.Length; i++)
                    //如果参数有值转换器，则使用转换器进行转换 否则使用默认转换
                    if (Parameters[i] != null && Parameters[i].ValueConverter != null)
                        arguments[i] = Parameters[i].ValueConverter.Invoke(arguments[i]);
                    else
                        arguments[i] = DefaultConvert(Parameters[i].GetType(), arguments[i]);
            return _constructorInfo.Invoke(arguments);
        }
    }
}