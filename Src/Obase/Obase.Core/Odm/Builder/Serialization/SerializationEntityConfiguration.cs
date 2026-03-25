/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 17:39:12
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm.Builder.Serialization
{
    /// <summary>
    ///     序列化实体基础配置
    /// </summary>
    public abstract class SerializationEntityConfiguration
    {
        /// <summary>
        ///     建模器
        /// </summary>
        private readonly ModelBuilder _modelBuilder;

        /// <summary>
        ///     初始化序列化实体基础配置
        /// </summary>
        /// <param name="modelBuilder">建模器</param>
        protected SerializationEntityConfiguration(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        /// <summary>
        ///     建模器
        /// </summary>
        public ModelBuilder ModelBuilder => _modelBuilder;

        /// <summary>
        ///     创建序列化实体方法
        /// </summary>
        /// <returns></returns>
        internal SerializationEntity Create()
        {
            //调用实现类的CreateReally方法构建模型类型
            return CreateReally();
        }

        /// <summary>
        ///     根据类型配置项中的元数据构建模型类型
        ///     本方法由派生类实现
        /// </summary>
        /// <returns>序列化实体类型</returns>
        protected abstract SerializationEntity CreateReally();
    }

    /// <summary>
    ///     序列化实体配置
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class SerializationEntityConfiguration<T> : SerializationEntityConfiguration
    {
        /// <summary>
        ///     初始化序列化实体配置
        /// </summary>
        /// <param name="modelBuilder">建模器</param>
        public SerializationEntityConfiguration(ModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        /// <summary>
        ///     根据类型配置项中的元数据构建模型类型
        ///     本方法由派生类实现
        /// </summary>
        /// <returns>序列化实体类型</returns>
        protected override SerializationEntity CreateReally()
        {
            //在这里创建和配置好所有的元素集合 包含属性和构造参数
            if (ModelBuilder.ExistSerializationEntityConfiguration(typeof(T)))
            {

            }
            return new SerializationEntity(typeof(T));
        }
    }
}
