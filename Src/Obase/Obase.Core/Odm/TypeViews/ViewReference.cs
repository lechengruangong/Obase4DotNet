/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：视图引用.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:20:28
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm.TypeViews
{
    /// <summary>
    ///     视图引用。来源于源（或源扩展）的一个引用元素。
    /// </summary>
    public class ViewReference : ReferenceElement, ITypeViewElement
    {
        /// <summary>
        ///     视图引用的锚（或称锚点）。
        ///     锚点是源扩展树上的一个节点，视图引用即来源于该节点代表类型的某个引用元素。
        /// </summary>
        private readonly AssociationTreeNode _anchor;

        /// <summary>
        ///     视图引用绑定。
        ///     绑定是一个视图引用锚所代表类型的一个引用元素，它是视图引用的来源。
        /// </summary>
        private readonly ReferenceElement _binding;

        /// <summary>
        ///     影子元素。
        /// </summary>
        private ViewReference _shadow;

        /// <summary>
        ///     创建ViewReference实例。
        /// </summary>
        /// <param name="name">元素名称。</param>
        /// <param name="binding">视图引用的绑定。</param>
        /// <param name="anchor">视图引用的锚。</param>
        /// 实施说明:
        /// 使用绑定的Multiple属性的值为视图引用的Multiple属性设值。
        public ViewReference(ReferenceElement binding, string name = null, AssociationTreeNode anchor = null) : base(
            name ?? binding.Name, binding.ElementType)
        {
            IsMultiple = binding.IsMultiple;
            _binding = binding;
            _anchor = anchor;
        }

        /// <summary>
        ///     获取视图引用的锚（或称锚点）。
        /// </summary>
        public AssociationTreeNode Anchor => _anchor;

        /// <summary>
        ///     获取视图引用绑定。
        /// </summary>
        public ReferenceElement Binding => _binding;

        /// <summary>
        ///     获取视图引用的类型。
        /// </summary>
        /// 实施说明:
        /// 返回视图引用的绑定的ReferenceType。
        [Obsolete]
        public override ObjectType ReferenceType => _binding.ReferenceType;

        /// <summary>
        ///     获取或设置视图引用的影子元素。
        /// </summary>
        internal ViewReference Shadow
        {
            get => _shadow;
            set => _shadow = value;
        }

        /// <summary>
        ///     获取视图引用所承载的对象导航行为。
        /// </summary>
        public override ObjectNavigation Navigation => _binding.Navigation;

        /// <summary>
        ///     获取视图引用在对象导航中承担的功能。
        /// </summary>
        public override ENavigationUse NavigationUse => _binding.NavigationUse;

        /// <summary>
        ///     获取元素值的类型
        /// </summary>
        public override TypeBase ValueType
        {
            get
            {
                switch (NavigationUse)
                {
                    case ENavigationUse.ArrivingReference:
                    case ENavigationUse.DirectlyReference:
                        return Navigation.TargetType;
                    case ENavigationUse.EmittingReference:
                        return Navigation.AssociationType;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(NavigationUse), "未知的导航类型");
                }
            }
        }

        /// <summary>
        ///     获取或设置影子元素
        /// </summary>
        ITypeViewElement ITypeViewElement.Shadow
        {
            get => _shadow;
            set => _shadow = (ViewReference)value;
        }

        /// <summary>
        ///     生成在视图表达式中定义视图引用的表达式，它规定了属性的锚点和绑定。
        /// </summary>
        /// <returns>定义当前视图引用的表达式。</returns>
        /// 实施说明:
        /// 使用AssociationExpressionGenerator和AttributeExpressionGenerator生成表达式。
        public Expression GenerateExpression(ParameterExpression sourcePara,
            Func<AssociationTreeNode, ParameterExpression> flatteningParaGetter)
        {
            var typeExpGenerator = new AssociationExpressionGenerator(sourcePara, flatteningParaGetter);
            var hostExp = _anchor.AsTree().Accept(typeExpGenerator);
            var hostType = hostExp.Parameters[0].Type;
            if (_anchor is ObjectTypeNode node)
            {
                var member = hostType.GetMember(node.ElementName)[0];
                //使用属性树生成器生成
                var generator = new AttributeExpressionGenerator(hostExp);
                AttributeTreeNode attrNode = new SimpleAttributeNode(new Attribute(hostType, member.Name));
                attrNode.AsTree().Accept(generator);
                return generator.Result;
            }

            throw new ArgumentException("无法为视图锚点生成表达式");
        }

        /// <summary>
        ///     穿透内嵌视图，获取视图引用的终极绑定。
        /// </summary>
        /// 实施说明
        /// 如果视图引用的绑定仍然是视图引用，则获取后者的绑定，依此规则递归调用，直到获取最终绑定的引用元素。
        public ReferenceElement GetFinalBinding()
        {
            if (Binding is ViewReference viewReference)
                //如果绑定是视图引用，则获取其终极绑定
                return viewReference.GetFinalBinding();
            return Binding;
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
            //返回绑定元素的导航结果
            return _binding == null ? Array.Empty<object>() : _binding.NavigationStep(sourceObj);
        }

        /// <summary>
        ///     验证延迟加载合法性。
        /// </summary>
        /// <param name="reason">返回不能启用延迟加载的原因。</param>
        /// <returns>如果可以启用延迟加载返回true，否则返回false，同时返回原因。</returns>
        protected override bool ValidateLazyLoading(out string reason)
        {
            reason = "视图引用无延迟加载";
            return false;
        }

        /// <summary>
        ///     获取视图引用的引用键。
        ///     说明
        ///     视图引用的引用键是其终极绑定的引用键。
        ///     defineMissing参数指示引用键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发KeyAttributeLackException。
        ///     但是，即使指示自动定义缺失的属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发CannotDefiningAttributeExcepti
        ///     on。
        /// </summary>
        /// <exception cref="KeyAttributeLackException">引用键属性没有定义</exception>
        /// <exception cref="CannotDefiningAttributeException">
        ///     无法定义缺失的引用键属性
        ///     实施说明
        ///     捕获ForeignKeyGuarantingException后引发CannotDefiningAttributeException。
        /// </exception>
        /// <param name="defineMissing">指示是否自动定义缺失的属性。</param>
        public override Attribute[] GetReferringKey(bool defineMissing = false)
        {
            //获取绑定的引用元素
            var refElement = GetFinalBinding();

            return refElement.GetReferringKey(defineMissing);
        }

        /// <summary>
        ///     获取视图引用的参考键。
        ///     说明
        ///     视图引用的参考键由其终极绑定的参考键属性在视图上的直观属性构成。参见活动图“获取视图引用的参考键”。
        ///     defineMissing参数指示参考键属性缺失时的行为，如果其值为false，引发KeyAttributeLackException。由于视图不支持自动定义属
        ///     性，所以当其值为true时引发CannotDefiningAttributeException。
        /// </summary>
        /// <exception cref="KeyAttributeLackException">引用键属性没有定义</exception>
        /// <exception cref="CannotDefiningAttributeException">无法定义缺失的参考键属性</exception>
        /// <param name="defineMissing">指示是否自动定义缺失的属性。</param>
        public override Attribute[] GetReferredKey(bool defineMissing = false)
        {
            //获取绑定的引用元素
            var refElement = GetFinalBinding();

            Attribute[] attributes;
            try
            {
                attributes = refElement.GetReferredKey();
            }
            catch (KeyAttributeLackException ex)
            {
                if (defineMissing) throw new CannotDefiningAttributeException(@"无法定义缺失的参考键属性", ex);

                throw;
            }

            var result = new List<Attribute>();
            //宿主必然为TypeView
            if (HostType is TypeView typeView)
            {
                //获取锚点
                var anchor = Anchor;
                foreach (var attribute in attributes)
                {
                    var keyAttribute = typeView.GetIntuitiveAttribute(attribute, anchor);
                    if (keyAttribute == null)
                    {
                        if (defineMissing)
                            throw new CannotDefiningAttributeException(@"无法定义缺失的参考键属性", null);
                        throw new KeyAttributeLackException(@"引用键属性没有定义");
                    }

                    result.Add(keyAttribute);
                }
            }

            return result.ToArray();
        }
    }
}