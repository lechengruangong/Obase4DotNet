/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：简单查询源.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:20:41
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     简单查询源，一般是表或者视图。
    /// </summary>
    public class SimpleSource : MonomerSource
    {
        /// <summary>
        ///     名称
        /// </summary>
        private string _name;

        /// <summary>
        ///     指代符，该指代符用于在Sql语句的其它部分引用源。
        /// </summary>
        private string _symbol;

        /// <summary>
        ///     创建简单查询源实例。
        /// </summary>
        /// <param name="name">名称</param>
        public SimpleSource(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     创建简单查询源实例。
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="alias">别名</param>
        public SimpleSource(string name, string alias)
        {
            _name = name;
            _symbol = alias;
        }

        /// <summary>
        ///     排序顺序
        /// </summary>
        public List<Order> StoringOrder { get; set; }

        /// <summary>
        ///     获取或设置源名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }


        /// <summary>
        ///     获取一个值，该值指示源是否支持排序冒泡
        /// </summary>
        public override bool CanBubbleOrder => true;

        /// <summary>
        ///     获取指代符，该指代符用于在Sql语句的其它部分引用源。
        /// </summary>
        public override string Symbol => _symbol ?? _name;

        /// <summary>
        ///     获取源的别名 同指代符
        /// </summary>
        public string Alias => _symbol ?? _name;

        /// <summary>
        ///     将简单源的存储顺序（StoringOrder）提升为指定查询的排序规则。
        ///     注：如果指定查询已设置排序规则，引发异常：“查询Sql语句已设置排序规则。“
        /// </summary>
        /// <param name="query">指定的查询。</param>
        public override void BubbleOrder(QuerySql query)
        {
            if (query.Orders != null && query.Orders.Count > 0)
                throw new InvalidOperationException("查询Sql语句已设置排序规则");

            foreach (var order in StoringOrder) query.Orders?.Add(order);
        }

        /// <summary>
        ///     仅供Sqlite使用的无指代符ToSting 用于Delete语句
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public string ToNoSymbolString(EDataSource sourceType)
        {
            if (sourceType != EDataSource.Sqlite && sourceType != EDataSource.PostgreSql)
                throw new ArgumentException("此方法仅供Sqlite和PostgreSQL使用");

            return sourceType == EDataSource.Sqlite ? $"`{_name}`" : $"\"{_name}\"";
        }

        /// <summary>
        ///     针对指定的数据源类型，生成数据源实例的字符串表示形式，该字符串可用于From子句、Update子句和Insert Into子句。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public override string ToString(EDataSource sourceType)
        {
            //有符号 组合名称和符号
            if (!string.IsNullOrEmpty(Symbol))
                switch (sourceType)
                {
                    case EDataSource.SqlServer:
                    {
                        return $"[{_name}] [{Symbol}]";
                    }
                    case EDataSource.PostgreSql:
                    {
                        return $"\"{_name}\" {Symbol}";
                    }
                    case EDataSource.Oracle:
                    {
                        return $"{_name} {Symbol}";
                    }
                    case EDataSource.MySql:
                    {
                        return $"`{_name}` `{Symbol}`";
                    }
                    case EDataSource.Sqlite:
                    {
                        return $"`{_name}` `{Symbol}`";
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                    }
                }

            //没有符号 直接返回名称
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    return $"[{_name}]";
                }
                case EDataSource.PostgreSql:
                {
                    return $"{_name}";
                }
                case EDataSource.Oracle:
                {
                    return $"{_name}";
                }
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                {
                    return $"`{_name}`";
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }
        }


        /// <summary>
        ///     使用参数化的方式 默认的用途 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //简单源 无参数化
            sqlParameters = new List<IDataParameter>();
            return ToString(sourceType);
        }

        /// <summary>
        ///     为源设置别名根。
        /// </summary>
        /// <param name="aliasRoot">要设置的别名根。</param>
        internal override void SetAliasRoot(string aliasRoot)
        {
            _symbol = aliasRoot;
        }

        /// <summary>
        ///     为源的指代符设置前缀，设置前缀后源的指代符变更为该前缀串联原指代符。
        /// </summary>
        /// <param name="prefix">前缀</param>
        public override void SetSymbolPrefix(string prefix)
        {
            //设置指代符前缀即在别名前加上前缀。
            _symbol = prefix + _symbol;
        }

        /// <summary>
        ///     别称设为NULL
        /// </summary>
        public override void ResetSymbol()
        {
            _symbol = null;
        }

        /// <summary>
        ///     重写==运算符
        /// </summary>
        /// <param name="simpleSource1">一个简单源</param>
        /// <param name="simpleSource2">另一个简单源</param>
        /// <returns></returns>
        public static bool operator ==(SimpleSource simpleSource1, SimpleSource simpleSource2)
        {
            if (Equals(simpleSource1, null) && Equals(simpleSource2, null)) return true;
            return !Equals(simpleSource1, null) && simpleSource1.Equals(simpleSource2);
        }

        /// <summary>
        ///     重写!=运算符
        /// </summary>
        /// <param name="simpleSource1">一个简单源</param>
        /// <param name="simpleSource2">另一个简单源</param>
        /// <returns></returns>
        public static bool operator !=(SimpleSource simpleSource1, SimpleSource simpleSource2)
        {
            return !(simpleSource1 == simpleSource2);
        }

        /// <summary>
        ///     私有Equal方法
        /// </summary>
        /// <param name="other">另一个简单源</param>
        /// <returns></returns>
        private bool Equals(SimpleSource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _name == other._name && _symbol == other._symbol;
        }

        /// <summary>
        ///     重写Equal方法
        /// </summary>
        /// <param name="obj">另一个简单源</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SimpleSource)obj);
        }

        /// <summary>
        ///     重写获取哈希
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _name != null ? _name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (_symbol != null ? _symbol.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}