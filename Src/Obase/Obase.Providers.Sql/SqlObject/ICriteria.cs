/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示条件，如筛选条件、连接条件等.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:04:23
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示条件，如筛选条件、连接条件等。
    /// </summary>
    public interface ICriteria
    {
        /// <summary>
        ///     将当前条件与另一条件执行逻辑与运算，得出一个新条件。
        /// </summary>
        /// <param name="other">另一个条件</param>
        ICriteria And(ICriteria other);

        /// <summary>
        ///     将当前条件与另一条件执行逻辑或运算，得出一个新条件。
        /// </summary>
        /// <param name="other">另一个条件</param>
        ICriteria Or(ICriteria other);

        /// <summary>
        ///     对当前条件执行逻辑非运算，得出一个新条件。
        /// </summary>
        ICriteria Not();

        /// <summary>
        ///     将表达式访问者引导至条件内部的表达式。
        ///     特别约定：仅引导至直接包含的表达式，规避通过其它对象间接包含的表达式（如InSelectCriteria中作为值域的子查询所包含的表达式）。
        /// </summary>
        /// <param name="visitor">要引导的表达式访问者。</param>
        void GuideExpressionVisitor(ExpressionVisitor visitor);

        /// <summary>
        ///     针对指定的数据源类型，生成条件实例的字符串表示形式。
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        string ToString(EDataSource sourceType);

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters, IParameterCreator creator);

        /// <summary>
        ///     使用默认数据源和参数化的方式将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sqlParameters">参数</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        string ToString(out List<IDataParameter> sqlParameters, IParameterCreator creator);
    }
}