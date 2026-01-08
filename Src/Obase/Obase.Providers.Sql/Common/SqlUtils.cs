/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Sql通用工具.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 10:28:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Saving;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Common
{
    /// <summary>
    ///     Sql通用工具
    /// </summary>
    internal static class SqlUtils
    {
        /// <summary>
        ///     Mysql默认映射
        /// </summary>
        private static readonly Dictionary<Type, string> MySqlValueTypeDictionary = new Dictionary<Type, string>();

        /// <summary>
        ///     Sqlite默认映射
        /// </summary>
        private static readonly Dictionary<Type, string> SqliteValueTypeDictionary = new Dictionary<Type, string>();

        /// <summary>
        ///     SqlServer默认映射
        /// </summary>
        private static readonly Dictionary<Type, string> SqlServerValueTypeDictionary = new Dictionary<Type, string>();

        /// <summary>
        ///     PostgreSql默认映射
        /// </summary>
        private static readonly Dictionary<Type, string> PostgreSqlValueTypeDictionary = new Dictionary<Type, string>();

        /// <summary>
        ///     初始化一些内部使用的工具
        /// </summary>
        static SqlUtils()
        {
            //MySql默认映射
            MySqlValueTypeDictionary.Add(typeof(byte), "tinyint");
            MySqlValueTypeDictionary.Add(typeof(sbyte), "tinyint");
            MySqlValueTypeDictionary.Add(typeof(short), "smallint");
            MySqlValueTypeDictionary.Add(typeof(ushort), "smallint");
            MySqlValueTypeDictionary.Add(typeof(int), "int");
            MySqlValueTypeDictionary.Add(typeof(uint), "int");
            MySqlValueTypeDictionary.Add(typeof(long), "bigint");
            MySqlValueTypeDictionary.Add(typeof(ulong), "bigint");
            MySqlValueTypeDictionary.Add(typeof(char), "char");
            MySqlValueTypeDictionary.Add(typeof(bool), "tinyint");
            MySqlValueTypeDictionary.Add(typeof(float), "float");
            MySqlValueTypeDictionary.Add(typeof(double), "double");
            MySqlValueTypeDictionary.Add(typeof(decimal), "decimal");
            MySqlValueTypeDictionary.Add(typeof(string), "varchar");
            MySqlValueTypeDictionary.Add(typeof(DateTime), "datetime");
            MySqlValueTypeDictionary.Add(typeof(TimeSpan), "time");
            MySqlValueTypeDictionary.Add(typeof(Guid), "varchar");
            //Sqlite默认映射
            SqliteValueTypeDictionary.Add(typeof(byte), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(sbyte), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(short), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(ushort), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(int), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(uint), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(long), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(ulong), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(char), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(bool), "INTEGER");
            SqliteValueTypeDictionary.Add(typeof(float), "REAL");
            SqliteValueTypeDictionary.Add(typeof(double), "REAL");
            SqliteValueTypeDictionary.Add(typeof(decimal), "REAL");
            SqliteValueTypeDictionary.Add(typeof(string), "TEXT");
            SqliteValueTypeDictionary.Add(typeof(DateTime), "TEXT");
            SqliteValueTypeDictionary.Add(typeof(TimeSpan), "TEXT");
            SqliteValueTypeDictionary.Add(typeof(Guid), "TEXT");
            //SqlServer默认映射
            SqlServerValueTypeDictionary.Add(typeof(byte), "tinyint");
            SqlServerValueTypeDictionary.Add(typeof(sbyte), "tinyint");
            SqlServerValueTypeDictionary.Add(typeof(short), "smallint");
            SqlServerValueTypeDictionary.Add(typeof(ushort), "smallint");
            SqlServerValueTypeDictionary.Add(typeof(int), "int");
            SqlServerValueTypeDictionary.Add(typeof(uint), "int");
            SqlServerValueTypeDictionary.Add(typeof(long), "bigint");
            SqlServerValueTypeDictionary.Add(typeof(ulong), "bigint");
            SqlServerValueTypeDictionary.Add(typeof(char), "char");
            SqlServerValueTypeDictionary.Add(typeof(bool), "tinyint");
            SqlServerValueTypeDictionary.Add(typeof(float), "float");
            SqlServerValueTypeDictionary.Add(typeof(double), "real");
            SqlServerValueTypeDictionary.Add(typeof(decimal), "decimal");
            SqlServerValueTypeDictionary.Add(typeof(string), "nvarchar");
            SqlServerValueTypeDictionary.Add(typeof(DateTime), "datetime");
            SqlServerValueTypeDictionary.Add(typeof(TimeSpan), "time");
            SqlServerValueTypeDictionary.Add(typeof(Guid), "nvarchar");
            //PostgreSql默认映射
            PostgreSqlValueTypeDictionary.Add(typeof(byte), "character");
            PostgreSqlValueTypeDictionary.Add(typeof(sbyte), "character");
            PostgreSqlValueTypeDictionary.Add(typeof(short), "smallint");
            PostgreSqlValueTypeDictionary.Add(typeof(ushort), "smallint");
            PostgreSqlValueTypeDictionary.Add(typeof(int), "int");
            PostgreSqlValueTypeDictionary.Add(typeof(uint), "int");
            PostgreSqlValueTypeDictionary.Add(typeof(long), "bigint");
            PostgreSqlValueTypeDictionary.Add(typeof(ulong), "bigint");
            PostgreSqlValueTypeDictionary.Add(typeof(char), "character");
            PostgreSqlValueTypeDictionary.Add(typeof(bool), "boolean");
            PostgreSqlValueTypeDictionary.Add(typeof(float), "real");
            PostgreSqlValueTypeDictionary.Add(typeof(double), "double precision");
            PostgreSqlValueTypeDictionary.Add(typeof(decimal), "decimal");
            PostgreSqlValueTypeDictionary.Add(typeof(string), "varchar");
            PostgreSqlValueTypeDictionary.Add(typeof(DateTime), "timestamp");
            PostgreSqlValueTypeDictionary.Add(typeof(TimeSpan), "time");
            PostgreSqlValueTypeDictionary.Add(typeof(Guid), "varchar");
        }

        /// <summary>
        ///     获取MySql默认映射
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static string GetMySqlDbType(Type type)
        {
            //枚举 tinyint
            if (type.IsEnum)
                return "tinyint";

            return MySqlValueTypeDictionary.TryGetValue(type, out var dbType) ? dbType : "varchar";
        }

        /// <summary>
        ///     获取Sqlite默认映射
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static string GetSqliteDbType(Type type)
        {
            //枚举 INTEGER
            if (type.IsEnum)
                return "INTEGER";

            return SqliteValueTypeDictionary.TryGetValue(type, out var dbType) ? dbType : "varchar";
        }

        /// <summary>
        ///     获取SqlServer默认映射
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static string GetSqlServerDbType(Type type)
        {
            //枚举 tinyint
            if (type.IsEnum)
                return "tinyint";

            return SqlServerValueTypeDictionary.TryGetValue(type, out var dbType) ? dbType : "nvarchar";
        }

        /// <summary>
        ///     获取PostgreSql默认映射
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static string GetPostgreSqlDbType(Type type)
        {
            //枚举 smallint
            if (type.IsEnum)
                return "smallint";

            return PostgreSqlValueTypeDictionary.TryGetValue(type, out var dbType) ? dbType : "varchar";
        }

        /// <summary>
        ///     获取PostgreSql默认自增字段的映射
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static string GetPostgreSqlAutoIncreaseDbType(Type type)
        {
            if (type == typeof(short) || type == typeof(ushort)) return "SMALLSERIAL";
            if (type == typeof(int) || type == typeof(uint)) return "SERIAL";
            if (type == typeof(long) || type == typeof(ulong)) return "BIGSERIAL";
            return "SERIAL";
        }

        /// <summary>
        ///     根据属性类型创建设值器
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="fieId">字段名</param>
        /// <param name="value">值</param>
        /// <param name="isIncrease">是否是增量设值</param>
        /// <param name="source">源名称</param>
        /// <returns></returns>
        public static IFieldSetter GetFieldSetter(Type dataType, string fieId, object value, bool isIncrease = false,
            string source = "")
        {
            //如果是增量设值 构造IncreaseSetter
            if (isIncrease)
            {
                var type = typeof(IncreaseSetter<>).MakeGenericType(dataType);
                return Activator.CreateInstance(type, source, fieId, value) as IFieldSetter;
            }

            //检查目标类型对应的设值器
            if (dataType == null) return new NullSetter(fieId);

            if (dataType.IsEnum)
            {
                var realEnumType = dataType.GetFields(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault()
                    ?.FieldType;
                if (realEnumType != null) dataType = realEnumType;
            }

            if (dataType == typeof(string))
                return new StringFieldSetter(source, fieId, value == null ? string.Empty : value.ToString());

            if (dataType == typeof(char))
                return new CharFieldSetter(source, fieId, value == null ? new char() : Convert.ToChar(value));

            if (dataType == typeof(bool))
                return new BoolFieldSetter(source, fieId, Convert.ToBoolean(value));

            if (dataType == typeof(DateTime))
                return new DateTimeFieldSetter(source, fieId, Convert.ToDateTime(value));
            if (dataType == typeof(TimeSpan))
                return new TimeSpanFieldSetter(source, fieId, (TimeSpan)value);
            if (dataType == typeof(Guid))
                return new GuidFieldSetter(source, fieId, (Guid)value);

            if (dataType.IsPrimitive || dataType == typeof(decimal))
            {
                var type = typeof(NumericFieldSetter<>).MakeGenericType(dataType);
                return Activator.CreateInstance(type, source, fieId, value) as IFieldSetter;
            }

            throw new ArgumentException($"无法为{fieId}({dataType.Name})生成字段设值器,请检查此属性的配置属性类型和取值器设值器.");
        }

        /// <summary>
        ///     生成筛选条件：筛选单个实体对象或关联对象
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="modelType">对象的模型类型</param>
        public static ICriteria GenerateCriteria(object obj, StructuralType modelType)
        {
            if (modelType is EntityType entityType)
                return GenerateCriteria(obj, entityType);
            if (modelType is AssociationType associationType)
                return GenerateCriteria(obj, associationType);
            return null;
        }


        /// <summary>
        ///     生成筛选条件：筛选单个实体对象
        /// </summary>
        /// <param name="entityObj">目标实体对象</param>
        /// <param name="entityType">对象的实体型</param>
        private static ICriteria GenerateCriteria(object entityObj, EntityType entityType)
        {
            ICriteria result = null;
            foreach (var attr in entityType.KeyAttributes)
            {
                var att = entityType.GetAttribute(attr);
                var value = ObjectSystemVisitor.GetValue(entityObj, att);
                //构造filed == value的条件
                var segment = GetCriteria(att.DataType, att.TargetField, value,
                    entityType.TargetTable);
                //每一节都是And的关系
                if (segment != null) result = result == null ? segment : result.And(segment);
            }

            return result;
        }


        /// <summary>
        ///     生成筛选条件：筛选单个关联对象
        /// </summary>
        /// <param name="associationObj">目标关联对象</param>
        /// <param name="associationType">对象的关联型</param>
        private static ICriteria GenerateCriteria(object associationObj, AssociationType associationType)
        {
            ICriteria result = null;
            foreach (var end in associationType.AssociationEnds)
                //循环每个关联端的每个映射
            foreach (var mapping in end.Mappings)
            {
                var attr = associationType.FindAttributeByTargetField(mapping.TargetField);
                object value;
                ICriteria segment;
                //如果定义了映射字段 就根据映射字段获取值
                //否则根据关联端的标识属性获取值
                if (attr != null)
                {
                    value = ObjectSystemVisitor.GetValue(associationObj, attr);
                    segment = GetCriteria(attr.DataType, mapping.TargetField, value,
                        associationType.TargetTable);
                }
                else
                {
                    var endObj = ObjectSystemVisitor.GetValue(associationObj, end);
                    value = ObjectSystemVisitor.GetValue(endObj, end.EntityType, mapping.KeyAttribute);
                    segment = GetCriteria(end.EntityType.GetAttribute(mapping.KeyAttribute).DataType,
                        mapping.TargetField, value, associationType.TargetTable);
                }

                //每一节都是And的关系
                if (segment != null) result = result == null ? segment : result.And(segment);
            }

            return result;
        }

        /// <summary>
        ///     根据属性类型创建条件
        /// </summary>
        /// <param name="dataType">字段类型</param>
        /// <param name="targetField">目标字段</param>
        /// <param name="value">值</param>
        /// <param name="source">源</param>
        /// <returns></returns>
        private static ICriteria GetCriteria(Type dataType, string targetField, object value, string source = "")
        {
            //根据字段类型创建条件
            if (dataType == typeof(string))
                return new StringCriteria(source, targetField, ERelationOperator.Equal,
                    value == null ? string.Empty : value.ToString());

            if (dataType == typeof(char))
                return new CharCriteria(source, targetField, ERelationOperator.Equal,
                    value == null ? new char() : Convert.ToChar(value));

            if (dataType == typeof(bool))
                return new BoolCriteria(source, targetField, ERelationOperator.Equal, Convert.ToBoolean(value));

            if (dataType == typeof(DateTime))
                return new DateTimeCriteria(source, targetField, ERelationOperator.Equal, Convert.ToDateTime(value));

            if (dataType == typeof(TimeSpan))
                return new TimeSpanCriteria(source, targetField, ERelationOperator.Equal,
                    TimeSpan.Parse(value.ToString()));

            if (dataType == typeof(TimeSpan))
                return new TimeSpanCriteria(source, targetField, ERelationOperator.Equal,
                    TimeSpan.Parse(value.ToString()));

            if (dataType.IsEnum)
            {
                var realEnumType = dataType.GetFields(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault()
                    ?.FieldType;
                if (realEnumType != null) dataType = realEnumType;
            }

            if (dataType.IsPrimitive)
            {
                var type = typeof(NumericCriteria<>).MakeGenericType(dataType);
                return Activator.CreateInstance(type, source, targetField, ERelationOperator.Equal, value) as ICriteria;
            }

            throw new ArgumentException($"无法为{targetField}({dataType.Name})生成条件,请检查此属性的配置属性类型和取值器设值器.");
        }

        /// <summary>
        ///     排序字段去重
        /// </summary>
        /// <param name="orders">排序字段列表</param>
        /// <returns></returns>
        public static List<Order> DistinctOrders(List<Order> orders)
        {
            //如果存在同一个Field的仅保留一个
            var orderSet = new HashSet<string>();
            var list = new List<Order>();
            foreach (var order in orders)
            {
                var orderStr = order.ToString(EDataSource.SqlServer).Replace("Desc", "").Replace("Asc", "");
                //使用HashSet去重
                if (orderSet.Add(orderStr)) list.Add(order);
            }

            return list;
        }
    }
}