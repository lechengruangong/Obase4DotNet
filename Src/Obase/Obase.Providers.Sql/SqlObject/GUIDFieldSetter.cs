/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：GUID字段设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:12:25
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     GUID字段设值器
    /// </summary>
    public class GuidFieldSetter : FieldSetter<Guid>
    {
        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        public GuidFieldSetter(string field, Guid value) : base(field, value)
        {
        }

        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        public GuidFieldSetter(string source, string field, Guid value) : base(source, field, value)
        {
        }

        /// <summary>
        ///     构造字段设值器
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        public GuidFieldSetter(Field field, Guid value) : base(field, value)
        {
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            return $"{_field.ToString(sourceType)} ='{_value.ToString("N".ToUpper())}'";
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="field">返回字段名称</param>
        public override string ToString(out string field)
        {
            return ToString(out field, EDataSource.SqlServer);
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="parameters">参数化参数集合</param>
        /// <param name="creator">数据源类型</param>
        /// <returns></returns>
        public override string ToString(out string field, out IDataParameter parameters, IParameterCreator creator)
        {
            return ToString(out parameters, out field, EDataSource.SqlServer, creator);
        }

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数化参数集合</param>
        /// <param name="field">字段</param>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="creator">参数化参数建造器</param>
        /// <returns></returns>
        public override string ToString(out IDataParameter parameters, out string field, EDataSource sourceType,
            IParameterCreator creator)
        {
            field = GetFiledString(sourceType);

            var valueStr = _value.ToString("D").ToUpper();

            return GetParameters(out parameters, sourceType, valueStr, creator);
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public override string ToString(out string field, EDataSource sourceType)
        {
            field = GetFiledString(sourceType);
            return _value.ToString("D").ToUpper();
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="parameters">参数化参数集合</param>
        /// <param name="creator">参数化参数建造器</param>
        /// <returns></returns>
        public override string ToString(out IDataParameter parameters, IParameterCreator creator)
        {
            return ToString(out parameters, EDataSource.SqlServer, creator);
        }

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="sourceType">数据源</param>
        /// <param name="creator">参数化参数建造器</param>
        /// <returns></returns>
        public override string ToString(out IDataParameter parameters, EDataSource sourceType,
            IParameterCreator creator)
        {
            var valueStr = _value.ToString("D").ToUpper();

            var parameter = GetParameters(out parameters, sourceType, valueStr, creator);

            return $"{_field.ToString(sourceType)} = {parameter}";
        }
    }
}