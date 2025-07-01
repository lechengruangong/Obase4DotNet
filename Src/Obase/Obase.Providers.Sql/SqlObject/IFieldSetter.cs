/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：字段设值器接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:14:42
└──────────────────────────────────────────────────────────────┘
*/

using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     字段设值器接口。
    ///     字段设值器用于指定字段的值，如：字段名=值，一般用于Update和Insert语句。
    /// </summary>
    public interface IFieldSetter
    {
        /// <summary>
        ///     获取要为其设置值的字段。
        /// </summary>
        Field Field { get; }

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式，该字符串将用于Update Sql的Set字句。
        /// </summary>
        string ToString(EDataSource sourceType);

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="field">返回字段名称</param>
        string ToString(out string field);

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="field">返回字段名称</param>
        /// <param name="parameters">返回字符串中的参数及其值。</param>
        /// <param name="creator">参数构造器</param>
        string ToString(out string field, out IDataParameter parameters, IParameterCreator creator);

        /// <summary>
        ///     将字段设值器实例转换成字符串表示形式，该字符串将用于Update Sql的Set字句。
        /// </summary>
        string ToString(out string field, EDataSource sourceType);

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        string ToString(out IDataParameter parameters, IParameterCreator creator);

        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于UpDate语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="sourceType">数据源</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        string ToString(out IDataParameter parameters, EDataSource sourceType, IParameterCreator creator);


        /// <summary>
        ///     将字段设值器实例转换成参数化的字符串表示形式，该字符串将用于Insert语句的Values字句，同时返回字段名称，用于Insert语句的字段列表。
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="field">字段</param>
        /// <param name="sourceType">数据源</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        string ToString(out IDataParameter parameters, out string field, EDataSource sourceType,
            IParameterCreator creator);
    }
}