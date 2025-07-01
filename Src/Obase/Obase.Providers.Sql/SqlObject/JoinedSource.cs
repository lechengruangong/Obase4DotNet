/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：连接查询源.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:23:59
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     连接查询源，即两个查询源通过Join运算得出的新源。
    /// </summary>
    public class JoinedSource : ISource
    {
        /// <summary>
        ///     连接条件
        /// </summary>
        private readonly ICriteria _joinCriteria;

        /// <summary>
        ///     连接源列表
        /// </summary>
        private readonly List<ISource> _sources;

        /// <summary>
        ///     连接方式，即左连接、内连接或右连接
        /// </summary>
        private ESourceJoinType _joinType;


        /// <summary>
        ///     创建连接查询源的实例，该源由两个源通过内连接运算得到。
        /// </summary>
        /// <param name="source1">第一个查询源</param>
        /// <param name="source2">第一个查询源</param>
        /// <param name="criteria">连接条件</param>
        public JoinedSource(ISource source1, ISource source2, ICriteria criteria)
        {
            _sources = new List<ISource>();
            _sources.AddRange(new[] { source1, source2 });
            _joinType = ESourceJoinType.Inner;
            _joinCriteria = criteria;
        }

        /// <summary>
        ///     创建连接查询源的实例。
        /// </summary>
        /// <param name="source1">第一个查询源</param>
        /// <param name="source2">第二个查询源</param>
        /// <param name="criteria">连接条件</param>
        /// <param name="joinType">连接方式</param>
        public JoinedSource(ISource source1, ISource source2, ICriteria criteria, ESourceJoinType joinType)
        {
            _sources = new List<ISource>();
            _sources.AddRange(new[] { source1, source2 });
            _joinCriteria = criteria;
            _joinType = joinType;
        }

        /// <summary>
        ///     获取或设置连接方式，即左连接、内连接或右连接。
        /// </summary>
        public ESourceJoinType JoinType
        {
            get => _joinType;
            set => _joinType = value;
        }

        /// <summary>
        ///     获取别名。
        /// </summary>
        [Obsolete]
        public string Alias => "";

        /// <summary>
        ///     调用此方法将引发OrderBubblingUnsuportedException。
        /// </summary>
        /// <param name="query">指定的查询。</param>
        public void BubbleOrder(QuerySql query)
        {
            throw new OrderBubblingUnsuportedException(this);
        }

        /// <summary>
        ///     获取一个值，该值指示源是否支持排序冒泡
        /// </summary>
        public bool CanBubbleOrder => false;

        /// <summary>
        ///     将当前源与另一源执行左连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        public ISource LeftJoin(ISource other, ICriteria criteria)
        {
            return new JoinedSource(this, other, criteria, ESourceJoinType.Left);
        }

        /// <summary>
        ///     将当前源与另一源执行内连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        public ISource InnerJoin(ISource other, ICriteria criteria)
        {
            return new JoinedSource(this, other, criteria);
        }

        /// <summary>
        ///     将当前源与另一源执行右连接运算，得出一个新源。
        /// </summary>
        /// <param name="other">另一个源</param>
        /// <param name="criteria">连接条件</param>
        public ISource RightJoin(ISource other, ICriteria criteria)
        {
            return new JoinedSource(this, other, criteria, ESourceJoinType.Right);
        }

        /// <summary>
        ///     针对指定的数据源类型，生成数据源实例的字符串表示形式，该字符串可用于From子句、Update子句和Insert Into子句。
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            string joinTypeStr;
            switch (JoinType)
            {
                case ESourceJoinType.Inner:
                    joinTypeStr = " inner join ";
                    break;
                case ESourceJoinType.Left:
                    joinTypeStr = " left join ";
                    break;
                case ESourceJoinType.Right:
                    joinTypeStr = " right join ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(JoinType), $"未知的连接类型{JoinType}");
            }

            var result =
                $"{_sources[0].ToString(sourceType)}{joinTypeStr}{_sources[1].ToString(sourceType)} on {_joinCriteria.ToString(sourceType)}";
            return result;
        }

        /// <summary>
        ///     使用参数化的方式 默认的用途 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            string joinTypeStr;
            switch (JoinType)
            {
                case ESourceJoinType.Inner:
                    joinTypeStr = " inner join ";
                    break;
                case ESourceJoinType.Left:
                    joinTypeStr = " left join ";
                    break;
                case ESourceJoinType.Right:
                    joinTypeStr = " right join ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(JoinType), $"未知的连接类型{JoinType}");
            }

            var result =
                $"{_sources[0].ToString(sourceType, out var leftDataParameters, creator)}{joinTypeStr}{_sources[1].ToString(sourceType, out var rightDataParameters, creator)} on {_joinCriteria.ToString(sourceType, out var criteriaDataParameters, creator)}";

            sqlParameters = new List<IDataParameter>();
            sqlParameters.AddRange(leftDataParameters);
            sqlParameters.AddRange(rightDataParameters);
            sqlParameters.AddRange(criteriaDataParameters);

            return result;
        }

        /// <summary>
        ///     仅供Sqlite使用的无指代符ToSting 用于Delete语句
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string ToNoSymbolString(EDataSource sourceType)
        {
            string joinTypeStr;
            switch (JoinType)
            {
                case ESourceJoinType.Inner:
                    joinTypeStr = " inner join ";
                    break;
                case ESourceJoinType.Left:
                    joinTypeStr = " left join ";
                    break;
                case ESourceJoinType.Right:
                    joinTypeStr = " right join ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(JoinType), $"未知的连接类型{JoinType}");
            }

            var result =
                $"{((SimpleSource)_sources[0]).ToNoSymbolString(sourceType)}{joinTypeStr}{((SimpleSource)_sources[1]).ToNoSymbolString(sourceType)} on {_joinCriteria.ToString(sourceType)}";
            return result;
        }
    }
}