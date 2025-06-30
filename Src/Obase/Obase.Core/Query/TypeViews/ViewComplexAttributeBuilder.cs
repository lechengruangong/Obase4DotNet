/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：复杂属性建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:39:58
└──────────────────────────────────────────────────────────────┘
*/

using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query.TypeViews
{
    /// <summary>
    ///     复杂属性建造器。
    /// </summary>
    public class ViewComplexAttributeBuilder : ViewElementBuilder
    {
        /// <summary>
        ///     构造ViewComplexAttributeBuilder实例
        /// </summary>
        /// <param name="model">对象数据模型</param>
        public ViewComplexAttributeBuilder(ObjectDataModel model) : base(model)
        {
        }

        /// <summary>
        ///     实例化类型元素，同时根据需要扩展视图源。
        /// </summary>
        /// <param name="member">与元素对应的类成员。</param>
        /// <param name="expression">类成员绑定的表达式。</param>
        /// <param name="sourceExtension">视图源扩展树。</param>
        /// <param name="paraBindings">形参绑定。</param>
        public override void Instantiate(MemberInfo member, Expression expression, AssociationTree sourceExtension,
            ParameterBinding[] paraBindings = null)
        {
            var lastAssoNode = expression.GrowAssociationTree(sourceExtension, _model,
                out AttributeTreeNode lastAattrNode, paraBindings);
            // 创建视图复杂属性 名字就是指向的复杂属性
            _element = new ViewComplexAttribute(member.Name, lastAssoNode, lastAattrNode);
        }
    }
}