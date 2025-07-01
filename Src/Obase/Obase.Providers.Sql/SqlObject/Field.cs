/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Sql语句中的字段.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:14:39
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示Sql语句中的字段，常用于条件表达式。
    /// </summary>
    public class Field
    {
        /// <summary>
        ///     字段名称
        /// </summary>
        private string _name;

        /// <summary>
        ///     构造字段
        /// </summary>
        /// <param name="name"></param>
        public Field(string name)
        {
            if (name.Contains("*")) throw new ArgumentException("列内不允许包含*号");
            _name = name;
        }

        /// <summary>
        ///     构造字段
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="name">名称</param>
        public Field(string source, string name)
        {
            if (name.Contains("*")) throw new ArgumentException("列内不允许包含*号");
            if (!string.IsNullOrEmpty(source))
                Source = new SimpleSource(source);
            _name = name;
        }

        /// <summary>
        ///     构造字段
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="name">名称</param>
        /// >
        public Field(MonomerSource source, string name)
        {
            if (name.Contains("*")) throw new ArgumentException("列内不允许包含*号");
            Source = source;
            _name = name;
        }

        /// <summary>
        ///     字段名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (value.Contains("*")) throw new ArgumentException("列内不允许包含*号");
                _name = value;
            }
        }

        /// <summary>
        ///     字段的源
        /// </summary>
        public MonomerSource Source { get; set; }


        /// <summary>
        ///     返回字段的字符串表示形式。
        ///     如果源为空，则返回字段名；否则返回“源名.字段名”。
        /// </summary>
        public override string ToString()
        {
            return ToString(EDataSource.SqlServer);
        }

        /// <summary>
        ///     针对指定的数据源类型，返回字段的字符串表示形式。
        ///     如果源为空，则返回字段名；否则返回“源名.字段名”。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public string ToString(EDataSource sourceType)
        {
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                    {
                        return Source?.Symbol != null
                            ? $"{Source.Symbol}.[{_name}]"
                            : $"[{_name}]";
                    }
                case EDataSource.PostgreSql:
                    {
                        if (Source != null && Source.Symbol != null && !string.IsNullOrEmpty(Source.Symbol))
                        {
                            if (_name.Contains("OTB"))
                            {
                                //当使用OTB生成时 此处的字段不应使用限定符
                                return $"{Source.Symbol}.{_name}";
                            }

                            return $"{Source.Symbol}.\"{_name}\"";
                        }
                        return $"\"{_name}\"";
                    }
                case EDataSource.MySql:
                case EDataSource.Sqlite:
                    {
                        return Source?.Symbol != null
                            ? $"`{Source.Symbol}`.`{_name}`"
                            : $"`{_name}`";
                    }
                case EDataSource.Oracle:
                    {
                        return Source?.Symbol != null
                            ? $"{Source.Symbol}.{_name}"
                            : $"{_name}";
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(sourceType), "不支持的数据源");
                    }
            }
        }

        /// <summary>
        ///     重写==运算符
        /// </summary>
        /// <param name="field1"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        public static bool operator ==(Field field1, Field field2)
        {
            if (Equals(field1, null) && Equals(field2, null)) return true;
            return !Equals(field1, null) && field1.Equals(field2);
        }

        /// <summary>
        ///     重写!=运算符
        /// </summary>
        /// <param name="field1"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        public static bool operator !=(Field field1, Field field2)
        {
            return !(field1 == field2);
        }

        /// <summary>
        ///     私有Equal方法
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool Equals(Field other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _name == other._name && Source.Equals(other.Source);
        }

        /// <summary>
        ///     重写Equal方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Field)obj);
        }

        /// <summary>
        ///     重写获取哈希方法
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0) * 397) ^ (Source != null ? Source.GetHashCode() : 0);
            }
        }
    }
}
