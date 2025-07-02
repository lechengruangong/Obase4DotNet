/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：源联接器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:20:39
└──────────────────────────────────────────────────────────────┘
*/


using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Saving;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     源联接器，以关联树的某一节点为基础向其某一子节点发起源联接操作。
    ///     该节点称为基节点，其代表的类型称为基型，基型对应的源称为基源。子节点称为目标节点，其代表的类型称为目标型，目标型的源称为目标源。
    ///     如果子节点代表关联引用，由于关联引用总是指向关联型，而其宿主类型作为该关联型的一端，所以这种联接属于从关联端联接到关联型。其中，关联端的实体型为基型，关联型称为
    ///     目标型。
    ///     如果子节点代表关联端，由于关联端的宿主类型总是关联型，所以这种联接属于从关联型联接到关联端。其中，关联型为基型，关联端指向实体型称为目标型。
    ///     如果子节点代表视图引用，该依据此引用最终绑定的类型元素确定联接方案。
    ///     默认使用基源作为联接操作的左操作数。调用方亦可显式指定左操作数源，但该源必须等效于或逻辑蕴含基源。
    /// </summary>
    public class SourceJoiner
    {
        /// <summary>
        ///     基源，可能为对象型的映射源，也可能为其衍生源。
        /// </summary>
        private readonly MonomerSource _baseSource;

        /// <summary>
        ///     源联接器内核。
        /// </summary>
        private readonly SourceJoinerCore _core = new SourceJoinerCore();

        /// <summary>
        ///     基型，即关联引用或关联端所属的类型。
        /// </summary>
        private readonly ReferringType _hostType;

        /// <summary>
        ///     基节点的别名。
        /// </summary>
        private readonly string _nodeAlias;

        /// <summary>
        ///     当基型为类型视图时，为其映射源提供遗传映射机制。
        /// </summary>
        private readonly TypeViewHeredityMapper _typeViewHeredityMapper = new TypeViewHeredityMapper();

        /// <summary>
        ///     作为联接操作左操作数的源。
        /// </summary>
        private ISource _leftSource;


        /// <summary>
        ///     创建SourceJoiner实例，联接时使用指定的基源和基节点别名。
        /// </summary>
        /// <param name="hostType">关联引用或关联端所属的对象型。</param>
        /// <param name="baseSource">基源。</param>
        /// <param name="nodeAlias">宿主类型对应的关联树节点的别名。</param>
        /// <param name="leftSource">作为左操作数的源。</param>
        public SourceJoiner(ReferringType hostType, MonomerSource baseSource = null, string nodeAlias = null,
            ISource leftSource = null)
        {
            _hostType = hostType;
            _nodeAlias = nodeAlias;
            _leftSource = leftSource;
            //如果基源为空 则构造基源
            if (baseSource == null)
            {
                if (_hostType is ObjectType objectType)
                    baseSource = new SimpleSource(objectType.TargetTable, _nodeAlias);
                else if (_hostType is TypeView typeView) baseSource = new SimpleSource(typeView.TargetName, _nodeAlias);
            }

            _baseSource = baseSource;
        }

        /// <summary>
        ///     获取基型。
        /// </summary>
        public ReferringType HostType => _hostType;

        /// <summary>
        ///     获取或设置作为联接操作左操作数的源。
        /// </summary>
        public ISource LeftSource
        {
            get => _leftSource;
            set => _leftSource = value;
        }

        /// <summary>
        ///     获取基节点的别名。
        /// </summary>
        public string NodeAlias => _nodeAlias;

        /// <summary>
        ///     生成目标节点的别名。
        ///     实施说明
        ///     调用AssociationTreeNodeAliasGenerator.GenerateAlias方法。
        /// </summary>
        /// <param name="element">指向目标节点的引用元素。</param>
        private string GenerateAlias(ReferenceElement element)
        {
            return AssociationTreeNodeAliasGenerator.GenerateAlias(element, NodeAlias);
        }

        /// <summary>
        ///     向引用元素指向的目标型发起源联接操作。
        /// </summary>
        /// <param name="elementName">指向目标型的引用元素的名称。</param>
        /// <param name="joinType">Join运算类型。</param>
        public ISource Join(string elementName, ESourceJoinType joinType = ESourceJoinType.Left)
        {
            var element = _hostType.GetReferenceElement(elementName);
            return Join(element, joinType);
        }

        /// <summary>
        ///     向引用元素指向的目标型发起源联接操作。
        /// </summary>
        /// <param name="element">指向目标型的引用元素。</param>
        /// <param name="joinType">Join运算类型。</param>
        public ISource Join(ReferenceElement element, ESourceJoinType joinType = ESourceJoinType.Left)
        {
            return Join(element, out _, out _, joinType);
        }

        /// <summary>
        ///     向引用元素指向的目标型发起源联接操作。
        /// </summary>
        /// <param name="elementName">指向目标型的引用元素的名称。</param>
        /// <param name="targetSource">返回联接操作生成的目标源。如果不应当联接，则返回基源。</param>
        /// <param name="targetNodeAlias">返回目标节点的别名。</param>
        /// <param name="joinType">Join运算类型。</param>
        public ISource Join(string elementName, out MonomerSource targetSource, out string targetNodeAlias,
            ESourceJoinType joinType = ESourceJoinType.Left)
        {
            var element = _hostType.GetReferenceElement(elementName);
            return Join(element, out targetSource, out targetNodeAlias, joinType);
        }

        /// <summary>
        ///     向引用元素指向的目标型发起源联接操作。
        /// </summary>
        /// <param name="element">指向目标型的引用元素。</param>
        /// <param name="targetSource">返回联接操作生成的目标源。如果不应当联接，则返回基源。</param>
        /// <param name="targetNodeAlias">返回目标节点的别名。</param>
        /// <param name="joinType">Join运算类型。</param>
        public ISource Join(ReferenceElement element, out MonomerSource targetSource, out string targetNodeAlias,
            ESourceJoinType joinType = ESourceJoinType.Left)
        {
            if (element is AssociationEnd associationEnd)
            {
                targetNodeAlias = null;
                return Join(associationEnd, out targetSource, ref targetNodeAlias, null, joinType);
            }

            if (element is AssociationReference associationReference)
            {
                targetNodeAlias = null;
                return Join(associationReference, out targetSource, ref targetNodeAlias, null, joinType);
            }

            if (element is SelfReference selfReference)
                return Join(selfReference, out targetSource, out targetNodeAlias, joinType);
            if (element is ViewReference viewReference)
                return Join(viewReference, out targetSource, out targetNodeAlias, joinType);

            //保底 不可能走到这
            targetSource = null;
            targetNodeAlias = null;
            return null;
        }

        /// <summary>
        ///     向引用元素指向的目标型发起源联接操作。
        /// </summary>
        /// <param name="assoRef">指向目标型的关联引用。</param>
        /// <param name="joinType">Join运算类型。</param>
        /// <param name="baseHeredityMapper">适用于基源的遗传映射器。</param>
        /// <param name="targetSource">返回联接操作生成的目标源。如果不应当联接，则返回基源。</param>
        /// <param name="targetNodeAlias">返回目标节点的别名。</param>
        private ISource Join(AssociationReference assoRef, out MonomerSource targetSource, ref string targetNodeAlias,
            IHeredityMapper baseHeredityMapper = null, ESourceJoinType joinType = ESourceJoinType.Left)
        {
            //别名
            if (string.IsNullOrWhiteSpace(targetNodeAlias)) targetNodeAlias = GenerateAlias(assoRef);
            //配置连接核心
            _core.Config(assoRef.AssociationType, targetNodeAlias);
            //目标源
            targetSource = _core.AssociationSource;
            //关联左端
            var assoEnd = assoRef.AssociationType.GetAssociationEnd(assoRef.LeftEnd);
            //联接
            if (_core.ShouldJoin(assoEnd))
            {
                _core.JoinType = joinType;
                return _core.FromEnd(assoEnd, _baseSource, _leftSource, baseHeredityMapper);
            }

            targetSource = _baseSource;
            return _leftSource ?? _baseSource;
        }

        /// <summary>
        ///     向引用元素指向的目标型发起源联接操作。
        /// </summary>
        /// <param name="assoEnd">指向目标型的关联端。</param>
        /// <param name="targetSource">返回联接操作生成的目标源。如果不应当联接，则返回基源。</param>
        /// <param name="targetNodeAlias">返回目标节点的别名。</param>
        /// <param name="baseHeredityMapper">适用于基源的遗传映射器。</param>
        /// <param name="joinType">Join运算类型</param>
        private ISource Join(AssociationEnd assoEnd,
            out MonomerSource targetSource, ref string targetNodeAlias,
            IHeredityMapper baseHeredityMapper = null, ESourceJoinType joinType = ESourceJoinType.Left)
        {
            //别名
            if (string.IsNullOrWhiteSpace(targetNodeAlias)) targetNodeAlias = GenerateAlias(assoEnd);
            //检查关联端
            if (assoEnd.HostType is AssociationType associationType)
            {
                _core.Config(associationType, _baseSource, baseHeredityMapper);
                if (_core.ShouldJoin(assoEnd))
                {
                    _core.JoinType = joinType;
                    var result = _core.ToEnd(assoEnd, targetNodeAlias, _leftSource, out var simpleSource);
                    targetSource = simpleSource;
                    return result;
                }

                targetSource = _baseSource;
                return _leftSource ?? _baseSource;
            }


            //保底 不可能走到这
            targetSource = null;
            targetNodeAlias = null;
            return null;
        }

        /// <summary>
        ///     向引用元素指向的目标型发起源联接操作。
        /// </summary>
        /// <param name="viewRef">指向目标型的视图引用。</param>
        /// <param name="targetSource">返回联接操作生成的目标源。如果不应当联接，则返回基源。</param>
        /// <param name="targetNodeAlias">返回目标节点的别名。</param>
        /// <param name="joinType">Join运算类型。</param>
        private ISource Join(ViewReference viewRef,
            out MonomerSource targetSource, out string targetNodeAlias, ESourceJoinType joinType = ESourceJoinType.Left)
        {
            var @ref = viewRef.GetFinalBinding();
            targetNodeAlias = GenerateAlias(viewRef);
            //配置遗传映射机制
            _typeViewHeredityMapper.JoinReference = viewRef;

            //关联端和关联引用
            if (@ref is AssociationReference associationReference)
                return Join(associationReference, out targetSource, ref targetNodeAlias, _typeViewHeredityMapper,
                    joinType);

            if (@ref is AssociationEnd associationEnd)
                return Join(associationEnd, out targetSource, ref targetNodeAlias, _typeViewHeredityMapper, joinType);

            //保底 不可能走到这
            targetSource = null;
            targetNodeAlias = null;
            return null;
        }

        /// <summary>
        ///     向引用元素指向的目标型发起源联接操作。
        ///     实施说明：
        ///     本方法不实际执行源联接操作。
        /// </summary>
        /// <param name="selfRef">指向目标型的反身引用。</param>
        /// <param name="targetSource">返回联接操作生成的目标源。（总是返回基源）</param>
        /// <param name="targetNodeAlias">返回目标节点的别名。（总是返回基节点的别名）</param>
        /// <param name="joinType">Join运算类型。</param>
        private ISource Join(SelfReference selfRef,
            out MonomerSource targetSource, out string targetNodeAlias, ESourceJoinType joinType = ESourceJoinType.Left)
        {
            targetSource = _baseSource;
            targetNodeAlias = _nodeAlias;
            return _baseSource;
        }

        /// <summary>
        ///     向引用元素指向的目标型发起联接时，判定源联接操作是否应当执行。
        /// </summary>
        /// <returns>应当执行返回true，否则返回false。</returns>
        /// <param name="elementName">指向目标型的引用元素的名称。</param>
        public bool ShouldJoin(string elementName)
        {
            var element = _hostType.GetReferenceElement(elementName);
            return ShouldJoin(element);
        }

        /// <summary>
        ///     向引用元素指向的目标型发起联接时，判定源联接操作是否应当执行。
        /// </summary>
        /// <returns>应当执行返回true，否则返回false。</returns>
        /// <param name="element">指向目标型的引用元素。</param>
        public bool ShouldJoin(ReferenceElement element)
        {
            if (element is AssociationEnd associationEnd)
                return ShouldJoin(associationEnd);
            if (element is AssociationReference associationReference)
                return ShouldJoin(associationReference);
            if (element is SelfReference selfReference)
                return ShouldJoin(selfReference);
            if (element is ViewReference viewReference)
                return ShouldJoin(viewReference);

            return false;
        }

        /// <summary>
        ///     向关联引用指向的目标型发起联接时，判定源联接操作是否应当执行。
        /// </summary>
        /// <returns>应当执行返回true，否则返回false。</returns>
        /// <param name="assoRef">指向目标型的关联引用。</param>
        private bool ShouldJoin(AssociationReference assoRef)
        {
            var assoType = assoRef.AssociationType;
            _core.Config(assoType, null);
            var assoEnd = assoType.GetAssociationEnd(assoRef.LeftEnd);
            return _core.ShouldJoin(assoEnd);
        }

        /// <summary>
        ///     向关联端指向的目标型发起联接时，判定源联接操作是否应当执行。
        /// </summary>
        /// <returns>应当执行返回true，否则返回false。</returns>
        /// <param name="assoEnd">指向目标型的关联端。</param>
        private bool ShouldJoin(AssociationEnd assoEnd)
        {
            if (assoEnd.HostType is AssociationType associationType)
            {
                _core.Config(associationType, null);
                return _core.ShouldJoin(assoEnd);
            }

            return false;
        }

        /// <summary>
        ///     向视图引用指向的目标型发起联接时，判定源联接操作是否应当执行。
        /// </summary>
        /// <returns>应当执行返回true，否则返回false。</returns>
        /// <param name="viewRef">指向目标型的视图引用。</param>
        private bool ShouldJoin(ViewReference viewRef)
        {
            return true;
        }

        /// <summary>
        ///     向反身引用指向的目标型发起联接时，判定源联接操作是否应当执行。（总是返回false）
        /// </summary>
        /// <returns>应当执行返回true，否则返回false。</returns>
        /// <param name="selfRef">指向目标型的反身引用。</param>
        private bool ShouldJoin(SelfReference selfRef)
        {
            return false;
        }
    }
}