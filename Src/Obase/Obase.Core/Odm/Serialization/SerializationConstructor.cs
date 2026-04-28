/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体类型构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 15:36:15
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Reflection;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化实体类型构造器
    /// </summary>
    public class SerializationConstructor
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        private readonly ConstructorInfo _constructorInfo;

        /// <summary>
        ///     初始化序列化实体类型构造器
        /// </summary>
        /// <param name="constructor">构造函数</param>
        public SerializationConstructor(ConstructorInfo constructor)
        {
            _constructorInfo = constructor;
            RealParameterCount = constructor.GetParameters().Length;
            Parameters = new Dictionary<string, SerializationConstructorParameter>();
        }

        /// <summary>
        ///     构造器的真实参数个数
        /// </summary>
        public int RealParameterCount { get; }

        /// <summary>
        ///     获取构造函数的形式参数。
        /// </summary>
        public Dictionary<string, SerializationConstructorParameter> Parameters { get; }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments">构造函数参数。</param>
        public object Construct(object[] arguments = null)
        {
            return _constructorInfo.Invoke(arguments);
        }

        /// <summary>
        ///     字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"SerializationConstructor:{{ConstructorInfo-\"{_constructorInfo.Name}\",RealParameterCount-\"{RealParameterCount}\"}}";
        }
    }
}