/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图引用建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 11:44:40
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
    ///     视图引用建造器。
    /// </summary>
    public class ViewReferenceBuilder : ViewElementBuilder
    {
        /// <summary>
        ///     构造ViewReferenceBuilder实例
        /// </summary>
        /// <param name="model">对象数据模型</param>
        public ViewReferenceBuilder(ObjectDataModel model) : base(model)
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
            //只能处理成员表达式
            if (expression is MemberExpression memberExpression)
            {
                var host = memberExpression.Expression;
                var name = member.Name;
                AssociationTreeNode lastNode;
                if (host is MemberExpression hostMember)
                    lastNode = hostMember.GrowAssociationTree(sourceExtension, _model, paraBindings);
                else
                    lastNode = host.GrowAssociationTree(sourceExtension, _model, paraBindings);
                var type = host.Type;
                //剥开集合取集合元素类型。
                if (type != typeof(string) && type.GetInterface("IEnumerable") != null)
                    type = type.GetGenericArguments()[0];
                var refType = _model.GetReferringType(type);
                var @ref = refType.GetReferenceElement(memberExpression.Member.Name);
                _element = new ViewReference(@ref, name, lastNode);
            }
        }
    }
}