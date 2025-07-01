/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字符字段设置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:41:24
└──────────────────────────────────────────────────────────────┘
*/

using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     字符字段设置器。
    /// </summary>
    public class CharFieldSetter : FieldSetter<char>
    {
        /// <summary>
        ///     构造字符字段设置器
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public CharFieldSetter(string field, char value) : base(field, value)
        {
        }

        /// <summary>
        ///     构造字符字段设置器
        /// </summary>
        /// <param name="source"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public CharFieldSetter(string source, string field, char value) : base(source, field, value)
        {
        }

        /// <summary>
        ///     构造字符字段设置器
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public CharFieldSetter(Field field, char value) : base(field, value)
        {
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            return $"{_field.ToString(sourceType)} = '{_value}'";
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public override string ToString(out string field)
        {
            return ToString(out field, EDataSource.SqlServer);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="field"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override string ToString(out string field, EDataSource sourceType)
        {
            field = GetFiledString(sourceType);
            return $"'{_value}'";
        }


        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="creator"></param>
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
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public override string ToString(out IDataParameter parameters, EDataSource sourceType,
            IParameterCreator creator)
        {
            var valueStr = $"'{_value}'";
            //参数化替换
            var parameter = GetParameters(out parameters, sourceType, valueStr, creator);

            var result = $"{_field.ToString(sourceType)} = {parameter}";

            return result;
        }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式
        /// </summary>
        /// <param name="field"></param>
        /// <param name="parameters"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public override string ToString(out string field, out IDataParameter parameters, IParameterCreator creator)
        {
            return ToString(out parameters, out field, EDataSource.SqlServer, creator);
        }


        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="field">字段</param>
        /// <param name="sourceType">数据源</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public override string ToString(out IDataParameter parameters, out string field, EDataSource sourceType,
            IParameterCreator creator)
        {
            field = GetFiledString(sourceType);

            var valueStr = $"{_value}";
            //参数化替换
            return GetParameters(out parameters, sourceType, valueStr, creator);
        }
    }
}