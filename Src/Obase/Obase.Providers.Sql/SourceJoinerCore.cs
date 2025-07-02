/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：源联接器核心.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:18:56
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Obase.Core.Saving;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     源联接器核心。
    ///     对象之间存在关联关系，基于关联可以在对象对应的源之间执行联接操作。假设存在两个实体型A和B，其关联型为AB，也就是说，A和B分别对应关联型AB的两个关联端。如果
    ///     要联接A和B对应的源，可采取两步操作：首先基于A的源联接AB的源，然后再联接B的源。由此可见，每次执行联接操作的根本依据都是某一关联型及其某个关联端。
    ///     为表述方便，我们将关联型对应的源简称为关联源，将关联端对应的源简称为端源。通常情况下，关联源和端源分别是关联型和关联端所对应实体型的映射源（SimpleSour
    ///     ce）。
    ///     对象投影运算（Select）会生成一个类型视图，投影运算结果是一个SelectSource,
    ///     它可以视为该视图的映射源进入运算管道，参与后续运算。后续运算如果需要以视图源或某一扩展节点为依据实施联接运算，那么视图的映射源（SelectSource）就成为
    ///     关联源或端源。由于视图的映射源是对视图源及扩展的映射源实施查询运算生成的，因此可以把它称为衍生源，相对地，将视图源及其扩展的映射源称为母体源。衍生源的字段全部来
    ///     自于母源，但字段名称通常会发生变化（一般是为了规避重名），我们把这种衍生于母体又产生名称变化的形象称为遗传映射。在联接操作中，我们只需要关注标识成员映射字段的遗
    ///     传映射。
    ///     源联接的核心任务就是在关联源与端源之间执行联接操作。有两种基本的联接方式，一是从关联端联接到关联型，二是从关联型联接到关联端。
    /// </summary>
    internal class SourceJoinerCore
    {
        /// <summary>
        ///     当AssociationSource为衍生源时，指定其遗传映射器。
        /// </summary>
        private IHeredityMapper _associationHeredityMapper;

        /// <summary>
        ///     关联源，可能为映射源，也可能为其衍生源。从关联端联接关联型时，该源为目标源；从关联型联接到关联端时，该源为基源。
        /// </summary>
        private MonomerSource _associationSource;

        /// <summary>
        ///     作为连接依据的关联型。
        /// </summary>
        private AssociationType _associationType;

        /// <summary>
        ///     联接类型。
        /// </summary>
        private ESourceJoinType _joinType;

        /// <summary>
        ///     返回适用于关联源的遗传映射器。
        /// </summary>
        internal IHeredityMapper AssociationHeredityMapper => _associationHeredityMapper;

        /// <summary>
        ///     获取关联源。
        /// </summary>
        internal MonomerSource AssociationSource => _associationSource;

        /// <summary>
        ///     获取作为联接依据的关联型。
        /// </summary>
        internal AssociationType AssociationType => _associationType;

        /// <summary>
        ///     联接类型。
        /// </summary>
        internal ESourceJoinType JoinType
        {
            get => _joinType;
            set => _joinType = value;
        }

        /// <summary>
        ///     更换联接时使用的关联源，可以同时更换其遗传映射器。
        /// </summary>
        /// <param name="assoSource">新的关联源，可以是映射源也可以是衍生源。</param>
        /// <param name="heredityMapper">关联源为衍生源时指定遗传映射器。</param>
        internal void ChangeSource(MonomerSource assoSource, IHeredityMapper heredityMapper)
        {
            _associationSource = assoSource;
            _associationHeredityMapper = heredityMapper;
        }

        /// <summary>
        ///     配置源联接器核心。
        /// </summary>
        /// <param name="assoType">作为联接依据的关联型。</param>
        /// <param name="assoSource">关联源。值为null时表示不指定关联源，联接时将使用映射源且无别名。</param>
        /// <param name="heredityMapper">当关联源为衍生源时指定其遗传映射器。值为null表示不需要使用遗传映射器。</param>
        internal void Config(AssociationType assoType, MonomerSource assoSource = null,
            IHeredityMapper heredityMapper = null)
        {
            _associationType = assoType;
            _associationSource = assoSource;
            _associationHeredityMapper = heredityMapper;
        }

        /// <summary>
        ///     配置联接器核心，该核心实施联接操作时将自动使用关联型的映射源。
        /// </summary>
        /// <param name="assoType">作为联接依据的关联型。</param>
        /// <param name="sourceAlias">指定关联映射源的别名。</param>
        internal void Config(AssociationType assoType, string sourceAlias = null)
        {
            _associationType = assoType;
            _associationSource = new SimpleSource(assoType.TargetTable, sourceAlias);
            _associationHeredityMapper = null;
        }

        /// <summary>
        ///     从指定关联端源联接关联源或从关联源联接指定关联端源时，判定联接操作是否应当执行。
        ///     实施说明
        ///     当指定关联端为伴随端时不应当执行，否则应当执行。
        /// </summary>
        /// <param name="assoEnd">关联端。</param>
        internal bool ShouldJoin(AssociationEnd assoEnd)
        {
            if (_associationType == null)
                throw new ArgumentException("联接操作前需设置关联型", nameof(AssociationType));
            return !_associationType.IsCompanionEnd(assoEnd);
        }

        /// <summary>
        ///     从指定关联端源联接关联源或从关联源联接指定关联端源时，判定联接操作是否应当执行。
        ///     实施说明
        ///     当指定关联端为伴随端时不应当执行，否则应当执行。
        /// </summary>
        /// <param name="endName">关联端名称。</param>
        internal bool ShouldJoin(string endName)
        {
            if (_associationType == null)
                throw new ArgumentException("联接操作前需设置关联型", nameof(AssociationType));
            return !_associationType.IsCompanionEnd(endName);
        }

        /// <summary>
        ///     从指定关联端的源联接关联的源。
        /// </summary>
        /// <param name="endName">关系端的名称。</param>
        /// <param name="endHeredityMapper">端源的遗传映射器。</param>
        /// <param name="baseSource">基源。</param>
        /// <param name="leftSource">左操作数。</param>
        internal JoinedSource FromEnd(string endName,
            IHeredityMapper endHeredityMapper, MonomerSource baseSource = null, ISource leftSource = null)
        {
            var assoEnd = _associationType.GetAssociationEnd(endName);
            return FromEnd(assoEnd, baseSource, leftSource, endHeredityMapper);
        }

        /// <summary>
        ///     从指定关联端的源联接关联的源。
        /// </summary>
        /// <param name="assoEnd">关系端。</param>
        /// <param name="baseSource">基源。</param>
        /// <param name="leftSource">左操作数。</param>
        /// <param name="endHeredityMapper">端源的遗传映射器。</param>
        internal JoinedSource FromEnd(AssociationEnd assoEnd, MonomerSource baseSource = null,
            ISource leftSource = null, IHeredityMapper endHeredityMapper = null)
        {
            //基源为空
            if (baseSource == null)
            {
                //关联端做基源
                var entityType = assoEnd.EntityType;
                var endTable = entityType.TargetTable;
                baseSource = new SimpleSource(endTable);
            }

            //构造条件
            var criteria = GenerateCriteria(assoEnd, baseSource, endHeredityMapper);
            //无左操作数 则基源做做操作数
            if (leftSource == null)
                leftSource = baseSource;
            //联接后的源
            return new JoinedSource(leftSource, _associationSource, criteria, _joinType);
        }


        /// <summary>
        ///     从指定关联的源联接关联端的源。
        /// </summary>
        /// <param name="endName">关系端的名称。</param>
        /// <param name="targetAlias">目标源的别名。</param>
        /// <param name="targetSource">返回联接操作中生成的目标源。</param>
        /// <param name="leftSource">左操作数。</param>
        internal JoinedSource ToEnd(string endName, string targetAlias, out SimpleSource targetSource,
            ISource leftSource = null)
        {
            var assoEnd = _associationType.GetAssociationEnd(endName);
            return ToEnd(assoEnd, targetAlias, leftSource, out targetSource);
        }

        /// <summary>
        ///     从指定关联的源联接关联端的源。
        /// </summary>
        /// <param name="assoEnd">关系端。</param>
        /// <param name="targetAlias">目标源别名。</param>
        /// <param name="leftSource">左操作数。</param>
        /// <param name="targetSource">返回联接操作中生成的目标源。</param>
        internal JoinedSource ToEnd(AssociationEnd assoEnd, string targetAlias, ISource leftSource,
            out SimpleSource targetSource)
        {
            //无左操作数 则关联源做做操作数
            if (leftSource == null)
                leftSource = _associationSource;
            //根据关联端做目标源
            var entityType = assoEnd.EntityType;
            var endTable = entityType.TargetTable;
            targetSource = new SimpleSource(endTable, targetAlias);

            //构造条件
            var criteria = GenerateCriteria(assoEnd, targetSource, null);
            //联接后的源
            return new JoinedSource(leftSource, targetSource, criteria, _joinType);
        }

        /// <summary>
        ///     生成联接条件。
        /// </summary>
        /// <param name="assoEnd">关联端。</param>
        /// <param name="endSource">端源。</param>
        /// <param name="endHeredityMapper">端源的遗传映射器。</param>
        private ICriteria GenerateCriteria(AssociationEnd assoEnd, MonomerSource endSource,
            IHeredityMapper endHeredityMapper)
        {
            //此端的类型
            var endType = assoEnd.EntityType;
            //处理每个映射
            var mappings = assoEnd.Mappings;

            //最终条件
            ICriteria result = null;

            foreach (var mapping in mappings)
            {
                //映射字段
                var mappingTarget = mapping.TargetField;
                if (_associationHeredityMapper != null)
                    mappingTarget = _associationHeredityMapper.Map(mappingTarget);
                var mappingField = new Field(_associationSource, mappingTarget);
                //映射的标识属性
                var keyAttr = endType.GetAttribute(mapping.KeyAttribute);
                //映射标识属性字段
                var keyAttrTarget = keyAttr.TargetField;
                if (endHeredityMapper != null)
                    keyAttrTarget = endHeredityMapper.Map(keyAttrTarget);
                var endField = new Field(endSource, keyAttrTarget);

                //构造表达式
                var mappingFieldExp = Expression.Fields(mappingField);
                var endFieldExp = Expression.Fields(endField);
                var criteriaExp = Expression.Equal(mappingFieldExp, endFieldExp);
                //包装为表达式条件
                var segment = new ExpressionCriteria(criteriaExp);
                //与最终条件联接
                result = result == null ? segment : result.And(segment);
            }

            return result;
        }
    }
}