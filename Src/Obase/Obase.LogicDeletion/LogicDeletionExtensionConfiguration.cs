/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除扩展配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:29:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除扩展配置
    /// </summary>
    /// <typeparam name="TObject">要逻辑删除的对象类型</typeparam>
    public class LogicDeletionExtensionConfiguration<TObject> : TypeExtensionConfiguration where TObject : class
    {
        /// <summary>
        ///     删除标记的映射字段
        /// </summary>
        private string _deletionField;

        /// <summary>
        ///     逻辑删除标记的属性的名称
        /// </summary>
        private string _deletionMark;

        /// <summary>
        ///     获取类型扩展的类型。
        /// </summary>
        public override Type ExtensionType => typeof(LogicDeletionExtension);

        /// <summary>
        ///     配置逻辑删除标记的属性的名称
        /// </summary>
        /// <param name="expression">配置标记的表达式</param>
        public void HasDeletionMark(Expression<Func<TObject, bool>> expression)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("请使用成员表达式配置逻辑删除标记的属性的名称");
            //解析表达式
            var member = (MemberExpression)expression.Body;
            //获取实体模型对应类型的属性
            var property = typeof(TObject).GetProperty(member.Member.Name);

            if (property == null)
                throw new ArgumentNullException(nameof(member.Member.Name),
                    $"无法在实体型{typeof(TObject).FullName}内找到到要配置为逻辑删除的属性{member.Member.Name}");

            if (property.PropertyType != typeof(bool))
                throw new ArgumentException("逻辑删除属性必须为bool类型");

            _deletionMark = member.Member.Name;
        }

        /// <summary>
        ///     配置删除标记的映射字段
        /// </summary>
        /// <param name="deletionField">映射字段</param>
        public void HasDeletionField(string deletionField)
        {
            _deletionField = deletionField;
        }

        /// <summary>
        ///     根据配置元数据生成类型扩展实例
        /// </summary>
        /// <returns></returns>
        public override TypeExtension MakeExtension()
        {
            if (string.IsNullOrEmpty(_deletionMark) && string.IsNullOrEmpty(_deletionField))
                throw new ArgumentException("逻辑删除标记DeletionMark和逻辑删除字段DeletionField不能同时为空");

            var extension = new LogicDeletionExtension
            {
                DeletionField = _deletionField,
                DeletionMark = _deletionMark
            };

            return extension;
        }
    }
}