/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联引用.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 17:25:56
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Obase.Core.Odm.Builder;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示关联引用。
    /// </summary>
    public class AssociationReference : ReferenceElement
    {
        /// <summary>
        ///     关联型
        /// </summary>
        private readonly AssociationType _associationType;

        /// <summary>
        ///     左端名
        /// </summary>
        private readonly string _leftEnd;

        /// <summary>
        ///     聚合级别
        /// </summary>
        private EAggregationLevel _aggregationLevel;

        /// <summary>
        ///     寄存 引用元素所承载的对象导航行为
        /// </summary>
        private ObjectNavigation _navigation;

        /// <summary>
        ///     右端名
        /// </summary>
        private string _rightEnd;

        /// <summary>
        ///     创建关联引用实例。
        /// </summary>
        /// <param name="name">关联引用的名称</param>
        /// <param name="associationType">关联型</param>
        /// <param name="leftEnd">左端名</param>
        /// <param name="rightEnd">右端名</param>
        public AssociationReference(string name, AssociationType associationType, string leftEnd, string rightEnd)
            : base(name, EElementType.AssociationReference)
        {
            _leftEnd = leftEnd;
            _rightEnd = rightEnd;
            _associationType = associationType;
        }

        /// <summary>
        ///     获取一个值，该值指示是否以左端表作为关联表。
        /// </summary>
        public bool LeftAsAssociationTable
        {
            get
            {
                var associationend = _associationType.GetAssociationEnd(_leftEnd);
                //左端的映射表
                return associationend != null && _associationType.IsCompanionEnd(associationend);
            }
        }

        /// <summary>
        ///     获取一个值，该值指示是否以右端表作为关联表。
        /// </summary>
        public bool RightAsAssociationTable
        {
            get
            {
                var associationend = _associationType.GetAssociationEnd(_rightEnd);
                //右端是否为伴随端
                if (associationend != null && _associationType.IsCompanionEnd(associationend))
                    return true;
                return false;
            }
        }

        /// <summary>
        ///     获取一个值，该值指示关联表是否独立。（表示有独立于左右端之外表示关系的表）
        /// </summary>
        public bool IndependentAssociationTable => !(LeftAsAssociationTable || RightAsAssociationTable);

        /// <summary>
        ///     获取或设置聚合级别。
        /// </summary>
        public EAggregationLevel AggregationLevel
        {
            get => _aggregationLevel;
            set => _aggregationLevel = value;
        }

        /// <summary>
        ///     获取左端名。
        /// </summary>
        public string LeftEnd => _leftEnd;

        /// <summary>
        ///     获取右端名。
        /// </summary>
        public string RightEnd
        {
            get => _rightEnd;
            internal set => _rightEnd = value;
        }

        /// <summary>
        ///     获取关联型。
        /// </summary>
        public AssociationType AssociationType => _associationType;

        /// <summary>
        ///     获取引用元素的类型。
        ///     当引用元素为关联引用时返回AssociationType；
        /// </summary>
        [Obsolete]
        public override ObjectType ReferenceType => _associationType;

        /// <summary>
        ///     获取引用元素所承载的对象导航行为。
        ///     实施说明：
        ///     当关联型为隐式关联时为直接导航，根据LeftEnd和RightEnd可以推断源端和目标端。
        ///     当关联型为显式关联时为间接导航，根据LeftEnd可推断源端，目标端不明确。
        /// </summary>
        public override ObjectNavigation Navigation
        {
            get
            {
                if (_navigation == null)
                {
                    string source = LeftEnd, target = RightEnd;
                    //显式关联
                    if (_associationType.Visible)
                        target = null;
                    //隐式关联 使用关联型构造导航
                    _navigation = new ObjectNavigation(_associationType, source, target);
                }

                return _navigation;
            }
        }

        /// <summary>
        ///     获取关联引用在对象导航中的用途。
        /// </summary>
        /// 实施说明： 
        /// 当关联为隐式时，为直接引用；
        /// 当关联为显式时，为发出引用。
        public override ENavigationUse NavigationUse
        {
            get
            {
                //根据关联类型是显式还是隐式判断
                if (_associationType.Visible) return ENavigationUse.EmittingReference;
                return ENavigationUse.DirectlyReference;
            }
        }

        /// <summary>
        ///     获取关联引用的值的类型。
        /// </summary>
        /// 实施说明:
        /// 对于隐式关联，值类型为右端实体型；对于显式关联，值类型为关联型。
        public override TypeBase ValueType
        {
            get
            {
                //隐式关联 右端实体型
                if (!_associationType.Visible)
                    return GetRightEnd().EntityType;
                //显式关联 关联型
                return _associationType;
            }
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
            var target = new List<object>();
            //获取关联引用对象（取出的是实体对象集合:List<文章>）
            var value = GetValue(sourceObj);
            if (value is IEnumerable iEnumerable)
            {
                var enumerator = iEnumerable.GetEnumerator();
                while (enumerator.MoveNext()) target.Add(enumerator.Current);
                if (enumerator is IDisposable disposable) disposable.Dispose();
            }
            else
            {
                target.Add(value);
            }

            if (!AssociationType.Visible) //隐式关联
            {
                var type = AssociationType;
                var assocObjs = new List<object>();
                //遍历关联对象引用
                foreach (var item in target)
                {
                    //左端对象 和 右端对象
                    var dic = new Dictionary<string, object> { { LeftEnd, sourceObj }, { RightEnd, item } };
                    //创建隐式关联对象（构造的是隐式关联对象（ImplicitAssociation<分类,文章>））
                    var assObj = BuildObject(type, dic);
                    //如果有符合关联端映射的键属性 则为其赋值
                    //找出左端
                    var leftEnd = type.AssociationEnds.FirstOrDefault(p => p.Name == LeftEnd);
                    if (leftEnd != null) SetEndMappingFiled(type, leftEnd, sourceObj, assObj);
                    //找出右端
                    var rightEnd = type.AssociationEnds.FirstOrDefault(p => p.Name == RightEnd);
                    if (rightEnd != null) SetEndMappingFiled(type, rightEnd, item, assObj);
                    //添加到关联对象集合
                    assocObjs.Add(assObj);
                }

                //隐式关联对象集合
                target = assocObjs;
            }

            //返回 显示关联直接就是引用对象 隐式则需要创建
            return target.ToArray();
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <param name="type">目标对象的类型（实体型、关联型、复杂类型）</param>
        /// <param name="referredObjects">一个字典，存储目标对象的关联引用或关联端，键为元素名称，值为关联引用或关联端对象</param>
        private object BuildObject(StructuralType type, Dictionary<string, object> referredObjects)
        {
            //使用构造器构造对象
            var target = type.Constructor.Construct();
            //遍历引用属性（关联端、关联引用）
            foreach (var re in type.Elements)
                if (referredObjects?.ContainsKey(re.Name) ?? false)
                    SetValue(target, referredObjects[re.Name]);
            return target;
        }

        /// <summary>
        ///     为隐式关联型的关联映射属性设值
        /// </summary>
        /// <param name="type">关联型</param>
        /// <param name="end">关联端</param>
        /// <param name="endObj">端对象</param>
        /// <param name="assObj">关联对象</param>
        private void SetEndMappingFiled(AssociationType type, AssociationEnd end, object endObj, object assObj)
        {
            //端的实体型
            var endType = end.EntityType;
            //端的键属性
            var endKeyAttrs = endType.Attributes.Where(p => endType.KeyAttributes.Contains(p.Name)).ToList();
            //在端内寻找符合条件的映射和属性
            foreach (var endKeyAttr in endKeyAttrs)
            foreach (var mapping in end.Mappings)
                //如果映射键属性和端对象键属性相同
                if (endKeyAttr.Name == mapping.KeyAttribute)
                {
                    //从端对象内取出键属性
                    var keyValue = endKeyAttr.ValueGetter.GetValue(endObj);
                    //为关联型内映射属性(映射属性在表内字段肯定和映射的目标属性相同)赋值
                    type.Attributes?.FirstOrDefault(p => p.TargetField == mapping.TargetField)?.ValueSetter
                        ?.SetValue(assObj, keyValue);
                }
        }

        /// <summary>
        ///     验证延迟加载合法性，由派生类实现。
        /// </summary>
        /// <returns>如果可以启用延迟加载返回true，否则返回false，同时返回原因。</returns>
        /// <param name="reason">返回不能启用延迟加载的原因。</param>
        protected override bool ValidateLazyLoading(out string reason)
        {
            reason = "";
            if (_associationType.Visible) return true;
            var leftEnd = _associationType.GetAssociationEnd(LeftEnd);
            var rightEnd = _associationType.GetAssociationEnd(RightEnd);

            foreach (var mapping in rightEnd.Mappings)
            {
                var attr = leftEnd.EntityType.FindAttributeByTargetField(mapping.TargetField);
                //验证左端实体型是否有对应的属性
                if (attr != null) continue;
                reason =
                    $"当前对象（{leftEnd.EntityType.Namespace}{leftEnd.EntityType.Name}）没有关联引用（{_associationType.Name}）右端对象的标识属性";
                return false;
            }

            return true;
        }

        /// <summary>
        ///     获取关联引用的左端。
        /// </summary>
        public AssociationEnd GetLeftEnd()
        {
            return _associationType.GetAssociationEnd(_leftEnd);
        }

        /// <summary>
        ///     获取关联引用的右端。
        ///     说明
        ///     当关联引用的关联型为显式关联或多方关联时，关联引用不直接指向关联端，而是指向关联本身，这种情况下本方法返回null。
        /// </summary>
        public AssociationEnd GetRightEnd()
        {
            //如果关联型为显式关联或多方关联，则返回null
            if (_associationType.Visible || _associationType.AssociationEnds.Count > 2)
                return null;

            return _associationType.GetAssociationEnd(_rightEnd);
        }

        /// <summary>
        ///     获取关联引用的参考键。
        ///     说明
        ///     如果左端为伴随端且关联型为隐式的，参考键为关联引用的关联型在右端上的外键；否则，为关联引用所属实体型的标识键。
        ///     defineMissing参数指示参考键属性缺失时的行为，如果其值为true，则自动定义缺失的属性，否则引发KeyAttributeLackException。
        ///     但是，即使指示自动定义缺失的属性，也不保证能定义成功。将根据现实条件判定能否定义，如果不能定义则引发CannotDefiningAttributeExcepti
        ///     on。
        /// </summary>
        /// <exception cref="KeyAttributeLackException">引用键属性没有定义</exception>
        /// <exception cref="CannotDefiningAttributeException">
        ///     无法定义缺失的参考键属性
        ///     实施说明
        ///     捕获ForeignKeyGuarantingException后引发CannotDefiningAttributeException。
        /// </exception>
        /// <param name="defineMissing">指示是否自动定义缺失的属性。</param>
        public override Attribute[] GetReferredKey(bool defineMissing = false)
        {
            var result = new List<Attribute>();

            //左端
            var leftEnd = GetLeftEnd();

            try
            {
                if (_associationType.IsCompanionEnd(leftEnd) && !_associationType.Visible)
                {
                    result.AddRange(_associationType.GetAssociationEnd(_rightEnd).GetForeignKey(defineMissing));
                }
                else
                {
                    //关联引用的宿主为实体型
                    if (HostType is EntityType entityType)
                        result.AddRange(entityType.Attributes.Where(p => entityType.KeyFields.Contains(p.Name))
                            .ToArray());
                }
            }
            catch (ForeignKeyGuarantingException ex)
            {
                throw new CannotDefiningAttributeException("无法定义缺失的参考键属性", ex);
            }

            return result.ToArray();
        }

        /// <summary>
        ///     获取关联引用的引用键。
        ///     说明
        ///     如果左端为伴随端且关联型为隐式的，引用键为右端实体型的标识键；否则，为关联引用的关联型在左端上的外键。
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
            var result = new List<Attribute>();

            //左端
            var leftEnd = GetLeftEnd();

            try
            {
                if (_associationType.IsCompanionEnd(leftEnd) && !_associationType.Visible)
                {
                    //右端实体型
                    var rightEntity = _associationType.GetAssociationEnd(_rightEnd).EntityType;
                    result.AddRange(rightEntity.Attributes.Where(p => rightEntity.KeyFields.Contains(p.Name))
                        .ToArray());
                }
                else
                {
                    result.AddRange(leftEnd.GetForeignKey(defineMissing));
                }
            }
            catch (ForeignKeyGuarantingException ex)
            {
                throw new CannotDefiningAttributeException(@"无法定义缺失的参考键属性", ex);
            }

            return result.ToArray();
        }

        /// <summary>
        ///     生成引用加载查询。
        /// </summary>
        /// <param name="sourceObjs">引用源对象。</param>
        /// <param name="through">指示是否穿透隐式独立关联。</param>
        /// <param name="nextOp">后续运算。将串联在引用加载查询之后。</param>
        public QueryOp GenerateLoadingQuery(object[] sourceObjs, bool through, QueryOp nextOp = null)
        {
            //要穿透隐式关联
            if (!AssociationType.Visible && AssociationType.Independent && through)
            {
                //构造o=>o.rightEnd形式的查询
                var paraExp = Expression.Parameter(AssociationType.ClrType, "o");
                var body = Expression.Property(paraExp, AssociationType.ClrType, _rightEnd);
                var selectionExp = Expression.Lambda(body, paraExp);
                nextOp = QueryOp.Select(selectionExp, HostType.Model, nextOp);
            }

            return GenerateLoadingQuery(sourceObjs, nextOp);
        }

        /// <summary>
        ///     获取引用加载查询的基点类型，即Where运算的SourceType。
        ///     实施说明
        ///     如果关联引用的关联型为隐式独立关联，基点类型为关联类型；否则，调用基实现。
        /// </summary>
        protected override ObjectType GetLoadingQueryInitalType()
        {
            //如果关联引用的关联型为隐式独立关联，基点类型为关联类型
            if (!AssociationType.Visible && AssociationType.Independent)
                return AssociationType;
            return base.GetLoadingQueryInitalType();
        }
    }
}