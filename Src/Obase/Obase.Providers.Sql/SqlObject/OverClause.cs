/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Over子句.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:28:32
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示Over子句
    /// </summary>
    public class OverClause
    {
        /// <summary>
        ///     作为排序依据的排序规则序列。
        /// </summary>
        private Order[] _orderBy;

        /// <summary>
        ///     作为分区依据的表达式序列。
        /// </summary>
        private Expression[] _partitionBy;

        /// <summary>
        ///     创建OverClause实例，同时指定排序依据。
        /// </summary>
        /// <param name="orderBy">排序规则。</param>
        public OverClause(Order orderBy)
        {
            _orderBy = new[] { orderBy };
        }

        /// <summary>
        ///     创建OverClause实例，同时指定排序依据。
        /// </summary>
        /// <param name="orderBy">一个作为排序依据的排序规则序列。</param>
        public OverClause(Order[] orderBy)
        {
            _orderBy = orderBy;
        }

        /// <summary>
        ///     创建OverClause实例，同时指定分区表达式。
        /// </summary>
        /// <param name="partitionBy">分区表达式。</param>
        public OverClause(Expression partitionBy)
        {
            _partitionBy = new[] { partitionBy };
        }

        /// <summary>
        ///     创建OverClause实例，同时指定分区表达式。
        /// </summary>
        /// <param name="partitionBy">一个作为分区依据的表达式序列。</param>
        public OverClause(Expression[] partitionBy)
        {
            _partitionBy = partitionBy;
        }

        /// <summary>
        ///     创建OverClause实例，同时指定分区表达式和排序依据。
        /// </summary>
        /// <param name="partitionBy">分区表达式。</param>
        /// <param name="orderBy">排序规则。</param>
        public OverClause(Expression partitionBy, Order orderBy)
        {
            _orderBy = new[] { orderBy };
            _partitionBy = new[] { partitionBy };
        }

        /// <summary>
        ///     创建OverClause实例，同时指定分区表达式和排序依据。
        /// </summary>
        /// <param name="partitionBy">作为分区依据的表达式序列。</param>
        /// <param name="orderBy">作为排序依据的排序规则序列。</param>
        public OverClause(Expression[] partitionBy, Order[] orderBy)
        {
            _orderBy = orderBy;
            _partitionBy = partitionBy;
        }

        /// <summary>
        ///     获取作为分区依据的表达式序列。
        /// </summary>
        public Expression[] PartitionBy => _partitionBy ?? (_partitionBy = Array.Empty<Expression>());

        /// <summary>
        ///     获取作为排序依据的排序规则序列。
        /// </summary>
        public Order[] OrderBy => _orderBy ?? (_orderBy = Array.Empty<Order>());

        /// <summary>
        ///     返回Over子句的文本表示形式。
        /// </summary>
        public override string ToString()
        {
            return ToString(EDataSource.SqlServer);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public string ToString(EDataSource sourceType)
        {
            return
                $"OVER ({(PartitionBy.Length > 0 ? $"PARTITION BY {string.Join(",", PartitionBy.Select(p => p.ToString(sourceType)))}" : "")} {(OrderBy.Length > 0 ? $"ORDER BY {string.Join(",", OrderBy.Select(o => o.ToString(sourceType)))}" : "")}) ";
        }

        /// <summary>
        ///     重写==
        /// </summary>
        /// <param name="overClause1">一个OverClaus</param>
        /// <param name="overClause2">另一个OverClause</param>
        /// <returns></returns>
        public static bool operator ==(OverClause overClause1, OverClause overClause2)
        {
            if (Equals(overClause1, null) && Equals(overClause2, null)) return true;
            return !Equals(overClause1, null) && overClause1.Equals(overClause2);
        }

        /// <summary>
        ///     重写!=
        /// </summary>
        /// <param name="overClause1">一个OverClaus</param>
        /// <param name="overClause2">另一个OverClause</param>
        /// <returns></returns>
        public static bool operator !=(OverClause overClause1, OverClause overClause2)
        {
            return !(overClause1 == overClause2);
        }

        /// <summary>
        ///     私有Equal方法
        /// </summary>
        /// <param name="other">另一个OverClause</param>
        /// <returns></returns>
        private bool Equals(OverClause other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_orderBy, other._orderBy) && _partitionBy.SequenceEqual(other._partitionBy);
        }

        /// <summary>
        ///     重写Equal方法
        /// </summary>
        /// <param name="obj">另一个OverClause</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OverClause)obj);
        }

        /// <summary>
        ///     重写获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_orderBy != null ? _orderBy.GetHashCode() : 0) * 397) ^
                       (_partitionBy != null ? _partitionBy.GetHashCode() : 0);
            }
        }
    }
}