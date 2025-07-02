/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：引用元素,如关联端和关联引用.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 17:11:05
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Query;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示为引用元素，即引用目标对象的元素，如关联引用、关联端。
    /// </summary>
    public abstract class ReferenceElement : TypeElement
    {
        /// <summary>
        ///     是否启用延迟加载，默认true
        /// </summary>
        private bool _enableLazyLoading = true;

        /// <summary>
        ///     指示是否已从数据库加载了引用。
        /// </summary>
        private bool _hasLoaded;

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        private int _loadingPriority;

        /// <summary>
        ///     加载触发器集合
        /// </summary>
        private List<IBehaviorTrigger> _loadingTriggers = new List<IBehaviorTrigger>();

        /// <summary>
        ///     创建引用元素实例
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="elementType">元素类型</param>
        protected ReferenceElement(string name, EElementType elementType)
            : base(name, elementType)
        {
        }

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        public int LoadingPriority
        {
            get => _loadingPriority;
            set => _loadingPriority = value;
        }


        /// <summary>
        ///     指示是否已从数据库加载了引用。
        /// </summary>
        public bool HasLoaded
        {
            get => _hasLoaded;
            set => _hasLoaded = value;
        }

        /// <summary>
        ///     获取或设置一个值，该值指示是否启用延迟加载，默认为true。
        ///     Set方法执行逻辑：首先调用ValidateLazyLoading方法执行验证，如果验证成功则为属性设值，否则引发异常，异常消息为ValidateLazyLoading方法返回的原因拼接”，无法启用延迟加载。“。
        /// </summary>
        public bool EnableLazyLoading
        {
            get => _enableLazyLoading;
            set =>
                //if (!ValidateLazyLoading(out var reason))
                //{
                //    throw new ArgumentException(reason, nameof(EnableLazyLoading));
                //}
                _enableLazyLoading = value;
        }

        /// <summary>
        ///     获取或设置加载触发器集合。
        /// </summary>
        public List<IBehaviorTrigger> LoadingTriggers
        {
            get => _loadingTriggers;
            set => _loadingTriggers = value;
        }

        /// <summary>
        ///     获取引用元素的类型。当引用元素为关联引用时返回AssociationType；为关联端时返回EntityType。
        /// </summary>
        [Obsolete]
        public abstract ObjectType ReferenceType { get; }

        /// <summary>
        ///     获取引用元素所承载的对象导航行为。
        /// </summary>
        public abstract ObjectNavigation Navigation { get; }

        /// <summary>
        ///     获取引用元素在对象导航中承担的功能。
        /// </summary>
        public abstract ENavigationUse NavigationUse { get; }

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
        public abstract object[] NavigationStep(object sourceObj);

        /// <summary>
        ///     验证延迟加载合法性，由派生类实现。
        /// </summary>
        /// <param name="reason">返回不能启用延迟加载的原因。</param>
        /// <returns>如果可以启用延迟加载返回true，否则返回false，同时返回原因。</returns>
        protected abstract bool ValidateLazyLoading(out string reason);

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
        public abstract Attribute[] GetReferringKey(bool defineMissing = false);

        /// <summary>
        ///     获取引用元素的引用键，并将键属性投射到以引用目标类型为终极源的视图，返回对应的视图属性。
        ///     将属性投射到视图，是指在视图上定义或搜索一个直观属性，该直观属性锚定于扩展树根节点，绑定于以该属性。
        ///     终极源是指在发生视图嵌套的情形下，最里层视图的源。
        ///     说明
        ///     defineMissing参数指示引用键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发KeyAttributeLackException。
        ///     但是，即使指示自动定义缺失的属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发CannotDefiningAttributeExcepti
        ///     on。
        /// </summary>
        /// <exception cref="KeyAttributeLackException">引用键属性没有定义</exception>
        /// <exception cref="CannotDefiningAttributeException">无法定义缺失的引用键属性</exception>
        /// <param name="targetView">指定属性投射视图。</param>
        /// <param name="defineMissing">指示是否自动定义缺失的属性。</param>
        public Attribute[] GetReferringKey(TypeView targetView, bool defineMissing = false)
        {
            //先无参获取
            var refferingAttrs = GetReferringKey(defineMissing);

            if (targetView == null) return refferingAttrs;
            //最终结果
            var resultList = new List<Attribute>();

            var stack = targetView.GetNestingStack();
            //挨个弹出
            while (stack.Count > 0)
            {
                var popedView = stack.Pop();

                var attrList = new List<Attribute>();

                foreach (var attribute in refferingAttrs)
                {
                    var intuitiveAttribute = popedView.GetIntuitiveAttribute(attribute);
                    if (intuitiveAttribute != null)
                        attrList.Add(intuitiveAttribute);
                }

                resultList.AddRange(attrList);
            }

            return resultList.ToArray();
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
        public abstract Attribute[] GetReferredKey(bool defineMissing = false);

        /// <summary>
        ///     生成引用加载查询。
        ///     引用加载是指从存储源取出引用元素所指向的对象，例如给定一个实体对象，取出该对象某一关联引用所指向的对象，或者给定一个关联对象，取出其某一关联端对象。
        ///     引用加载查询是一个查询链，执行该查询链得到结果就可以完成引用加载。
        ///     在对象系统中，对象内的引用元素（简记为R）是基于关联定义的，关联是对象间引用的根本依据。因此存在两类基本的引用加载。一类是给定关联端，加载关联实例；另一类是给定
        ///     关联对象，加载某一关联端。
        ///     如果R是从关联指向端（即从关联对象指向关联端或从关联的伴随端指向另一端），引用键为目标型的标识键，参考键为R的关联型在目标端上的外键。
        ///     如果R是从端指向关联（即从端对象，简称源端，指向关联对象或关联的伴随端），引用键为R的关联型在源端上的外键，参考键为源端的标识键。
        ///     如果R的关联是隐式独立关联，则目标型上不存在R的引用键，源类型上也不存在R的参考键。在这种情况下，如果要构造引用加载查询，可采用两步运算，首先查询关联，然后投影
        ///     到目标型。这实质上是将R分解为两个引用R1和R2，R1为“从端指向关联”的引用，R2为“从关联指向端”的引用。可以将R的关联显式化（即定义一个关联类），在显式化
        ///     的关联类上定义R1的引用键和R2的参考键。
        /// </summary>
        /// <param name="sourceObjs">引用源对象。</param>
        /// <param name="nextOp">后续运算。将串联在引用加载查询之后。</param>
        public QueryOp GenerateLoadingQuery(object[] sourceObjs, QueryOp nextOp = null)
        {
            //类型
            var initialType = GetLoadingQueryInitalType();
            //引用键
            var referingKey = GetReferringKey(true);
            //参考键
            var referedKey = GetReferredKey();

            //过滤器
            var expression = GenerateFilter(initialType, referingKey, referedKey, sourceObjs);
            //构造查询
            var queryOp = QueryOp.Where(expression, HostType.Model, nextOp);

            return queryOp;
        }

        /// <summary>
        ///     为引用加载查询生成过滤器（查询条件）。
        /// </summary>
        /// <param name="targetType">要查询的目标类型。</param>
        /// <param name="referringKey">引用键。</param>
        /// <param name="referredKey">参考键。</param>
        /// <param name="sourceObjs">引用源对象。</param>
        private LambdaExpression GenerateFilter(ObjectType targetType, Attribute[] referringKey,
            Attribute[] referredKey, object[] sourceObjs)
        {
            //重建类型
            var rebuildingType = targetType.RebuildingType;
            var parameterExp = Expression.Parameter(rebuildingType, "o");

            Expression body = null;
            foreach (var obj in sourceObjs)
            {
                Expression eachObj = null;
                foreach (var referring in referringKey)
                foreach (var referred in referredKey)
                {
                    var prop = rebuildingType.GetMember(referring.Name).FirstOrDefault();
                    if (prop == null)
                        prop = rebuildingType.GetMember(referring.Name, BindingFlags.NonPublic | BindingFlags.Instance)
                            .FirstOrDefault();

                    //构造一个形如 引用键==参考键.值的表达式
                    var left = Expression.MakeMemberAccess(parameterExp, prop);
                    var value = referred.GetValue(obj);
                    var right = Expression.Constant(value, referred.DataType);
                    var segment = Expression.Equal(left, right);
                    //拼接
                    eachObj = eachObj == null ? segment : Expression.AndAlso(eachObj, segment);
                }

                //拼接
                body = body == null ? eachObj : Expression.OrElse(body, eachObj);
            }

            return Expression.Lambda(body, parameterExp);
        }

        /// <summary>
        ///     获取引用加载查询的基点类型，即Where运算的SourceType。
        ///     实施说明
        ///     如果NavigationUse == DirectlyReference，基点类型为Navigation.TargetType；否则：
        ///     （1）如果NavigationUse == EmitReference，基点类型为Navigation.AssociationType；
        ///     （2）如果NavigationUse == ArrivingReference，基点类型为TargetType。
        /// </summary>
        protected virtual ObjectType GetLoadingQueryInitalType()
        {
            switch (NavigationUse)
            {
                case ENavigationUse.DirectlyReference:
                case ENavigationUse.ArrivingReference:
                    return Navigation.TargetType;
                case ENavigationUse.EmittingReference:
                    return Navigation.AssociationType;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NavigationUse), $"未知的导航功能{NavigationUse}");
            }
        }

        /// <summary>
        ///     从指定的对象集合中筛选出引用目标对象或以引用目标类型为终极源的视图的实例。
        /// </summary>
        /// <param name="objects">引用目标对象的候选集。</param>
        /// <param name="sourceObj">作为引用源的对象。</param>
        /// <param name="targetView">以引用目标类型为终极源的视图。</param>
        /// <param name="removing">是否移除并集</param>
        public object[] FilterTarget(ref object[] objects, object sourceObj, TypeView targetView = null,
            bool removing = false)
        {
            var result = new List<object>();

            //要移除的
            var removed = new List<object>();

            if (targetView != null)
            {
                //要比较的参考标识
                var reffered = GetReferredId(sourceObj);

                //逐一处理
                foreach (var obj in objects)
                {
                    if (obj == null) continue;

                    //要比较的属性
                    var attributes = GetReferringKey(targetView);

                    //序列比较
                    if (reffered.Count == attributes.Length)
                    {
                        //如果序列内对位值不相等 此值为true
                        var flag = false;
                        for (var i = 0; i < reffered.Count; i++)
                        {
                            //引用值
                            var referringValue = attributes[i].GetValue(obj);
                            if (reffered[i].ToString() != referringValue.ToString())
                                flag = true;
                        }

                        if (flag)
                            continue;
                        result.Add(obj);
                        removed.Add(obj);
                    }
                }
            }
            else
            {
                //没有目标视图 根据sourceObj筛选 去掉关联型对象
                //逐一处理
                foreach (var obj in objects)
                    if (obj != sourceObj)
                        if (HostType is ObjectType objectType)
                        {
                            //如果这个空引用的关联型与目标集合中的关联类型有关系
                            //才移除掉
                            var targetAssociationType = HostType.Model.GetAssociationType(obj.GetType());
                            foreach (var referenceElement in objectType.ReferenceElements)
                                if (referenceElement is AssociationReference associationReference &&
                                    associationReference.AssociationType == targetAssociationType)
                                {
                                    result.Add(obj);
                                    removed.Add(obj);
                                }
                        }
            }

            if (removing) objects = objects.Except(removed).ToArray();

            return result.ToArray();
        }

        /// <summary>
        ///     获取引用元素在指定对象上的引用标识，即引用键属性在指定对象上的值构成的序列。
        /// </summary>
        /// <param name="targetObj">要从其获取引用标识的对象。</param>
        public IdentityArray GetReferringId(object targetObj)
        {
            //引用键
            var referring = GetReferringKey();

            //对象标志
            var identityArray = new IdentityArray();

            foreach (var attribute in referring) identityArray.Add(attribute.GetValue(targetObj));

            return identityArray;
        }

        /// <summary>
        ///     获取引用元素在指定对象上的参考标识，即参考键属性在指定对象上的值构成的序列。
        /// </summary>
        /// <param name="targetObj">要从其获取参考标识的对象。</param>
        public IdentityArray GetReferredId(object targetObj)
        {
            //参考键
            var referred = GetReferredKey();
            //对象标志
            var identityArray = new IdentityArray();

            foreach (var attribute in referred) identityArray.Add(attribute.GetValue(targetObj));

            return identityArray;
        }
    }
}