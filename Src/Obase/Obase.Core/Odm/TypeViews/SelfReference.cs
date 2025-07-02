/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：反身引用.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 16:23:40
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     反身引用，即视图对自已的源的引用。
    /// </summary>
    public class SelfReference : ReferenceElement, ITypeViewElement
    {
        /// <summary>
        ///     影子元素。
        /// </summary>
        private SelfReference _shadow;

        /// <summary>
        ///     创建SelfReference实例。
        /// </summary>
        /// <param name="name">引用元素的名称。</param>
        public SelfReference(string name) : base(name, EElementType.AssociationReference)
        {
        }

        /// <summary>
        ///     获取或设置反身引用的影子元素。
        /// </summary>
        internal SelfReference Shadow
        {
            get => _shadow;
            set => _shadow = value;
        }

        /// <summary>
        ///     获取反身引用的类型。总返回源的类型，如果源不为ObjectType，返回null。
        /// </summary>
        [Obsolete]
        public override ObjectType ReferenceType
        {
            get
            {
                if (HostType is ObjectType objectType) return objectType;
                return null;
            }
        }

        /// <summary>
        ///     获取反身引用所承载的对象导航行为。总是返回null。
        /// </summary>
        public override ObjectNavigation Navigation => null;

        /// <summary>
        ///     获取反身引用在对象导航中承担的功能。总是返回DirectlyReference。
        /// </summary>
        public override ENavigationUse NavigationUse => ENavigationUse.DirectlyReference;

        /// <summary>
        ///     获取反身引用的值的类型。总返回源的类型，如果源不为ObjectType，返回null。
        /// </summary>
        public override TypeBase ValueType
        {
            get
            {
                if (HostType is ObjectType objectType) return objectType;
                return null;
            }
        }

        ITypeViewElement ITypeViewElement.Shadow
        {
            get => _shadow;
            set => _shadow = (SelfReference)value;
        }

        /// <summary>
        ///     生成在视图表达式中定义反身引用的表达式，它规定了属性的锚点和绑定。
        /// </summary>
        /// <returns>定义当前反身引用的表达式。</returns>
        /// 实施说明:
        /// 返回指代this的表达式。
        public Expression GenerateExpression(ParameterExpression sourcePara,
            Func<AssociationTreeNode, ParameterExpression> flatteningParaGetter)
        {
            return Expression.Lambda(Expression.Constant(this));
        }

        /// <summary>
        ///     在基于当前引用元素实施关联导航的过程中，向前推进一步。
        ///     基于特定的关联，可以从一个对象转移到另一个对象，这个过程称为导航。有两种类型的导航。一种是间接导航，即借助于关联对象，先从源对象转移到关联对象，然后再转移到目标
        ///     对象。另一种是直接导航，即从源对象直接转移到目标对象。
        ///     注意，不论基于隐式关联还是显式关联，本方法实施的关联导航统一遵循间接导航路径，即如果是隐式关联，将自动实施显式化操作。
        /// </summary>
        /// <returns>
        ///     本次导航步的到达地。
        ///     实施说明
        ///     参照ObjectSystemVisitor.
        ///     AssociationNavigate方法及相应的顺序图“执行映射/Saving/访问对象系统/关联导航”。
        /// </returns>
        /// <param name="sourceObj">本次导航步的出发地。</param>
        public override object[] NavigationStep(object sourceObj)
        {
            //反身引用 自己导航自己
            return new[] { sourceObj };
        }

        /// <summary>
        ///     验证延迟加载合法性，由派生类实现。
        /// </summary>
        /// <param name="reason">返回不能启用延迟加载的原因。</param>
        /// <returns>如果可以启用延迟加载返回true，否则返回false，同时返回原因。</returns>
        protected override bool ValidateLazyLoading(out string reason)
        {
            reason = @"反身引用无延迟加载";
            return false;
        }

        /// <summary>
        ///     获取引用元素的引用键。
        ///     设S(s1, s2, ..., sn)为对象O的属性序列，R为O的一个引用元素，该引用的目标型RT存在一个属性序列T(t1, t2, ...,
        ///     tn)，将S与T的元素一一配对，即ti -> si，然后以ti为依据、以si在O上的取值为参考值构建过滤器，即
        ///     ∩ ti = vi (i = 1, 2, ..., n)，其中vi为属性si在对象O上的取值，
        ///     以该过滤器作用于RT的实例集，如果所得到的对象集刚好为R的值，则称T为R的引用键，ti为引用属性，S为参考键，si为参考属性。
        ///     引用键和参考键均不是必须的，如果没有不影响引用的加载，例如在关系数据库中可以通过联表的方式加载。
        ///     说明
        ///     defineMissing参数指示引用键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发KeyAttributeLackException。
        ///     但是，即使指示自动定义缺失的属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发CannotDefiningAttributeExcepti
        ///     on。
        /// </summary>
        /// <exception cref="KeyAttributeLackException">引用键属性没有定义</exception>
        /// <exception cref="CannotDefiningAttributeException">无法定义缺失的引用键属性</exception>
        /// <param name="defineMissing">指示是否自动定义缺失的属性。</param>
        public override Attribute[] GetReferringKey(bool defineMissing = false)
        {
            //反身引用 自己的键
            return ReferenceType?.Attributes.Where(p => ReferenceType.KeyFields.Contains(p.Name)).ToArray();
        }

        /// <summary>
        ///     获取引用元素的参考键。
        ///     设S(s1, s2, ..., sn)为对象O的属性序列，R为O的一个引用元素，该引用的目标型RT存在一个属性序列T(t1, t2, ...,
        ///     tn)，将S与T的元素一一配对，即ti -> si，然后以ti为依据、以si在O上的取值为参考值构建过滤器，即
        ///     ∩ ti = vi (i = 1, 2, ..., n)，其中vi为属性si在对象O上的取值，
        ///     以该过滤器作用于RT的实例集，如果所得到的对象集刚好为R的值，则称T为R的引用键，ti为引用属性，S为参考键，si为参考属性。
        ///     引用键和参考键均不是必须的，如果没有不影响引用的加载，例如在关系数据库中可以通过联表的方式加载。
        ///     说明
        ///     defineMissing参数指示参考键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发KeyAttributeLackException。
        ///     但是，即使指示自动定义缺失的属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发CannotDefiningAttributeExcepti
        ///     on。
        /// </summary>
        /// <exception cref="KeyAttributeLackException">引用键属性没有定义</exception>
        /// <exception cref="CannotDefiningAttributeException">无法定义缺失的参考键属性</exception>
        /// <param name="defineMissing">指示是否自动定义缺失的属性。</param>
        public override Attribute[] GetReferredKey(bool defineMissing = false)
        {
            //反身引用 自己的键
            return ReferenceType?.Attributes.Where(p => ReferenceType.KeyFields.Contains(p.Name)).ToArray();
        }
    }
}