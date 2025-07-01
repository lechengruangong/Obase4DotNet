/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示一个字段的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:02:02
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示一个字段的表达式。
    /// </summary>
    public class FieldExpression : Expression
    {
        /// <summary>
        ///     字段表达式所表示的字段。
        /// </summary>
        private readonly Field _field;

        /// <summary>
        ///     创建FieldExpression的实例，并设置Field属性的值。
        /// </summary>
        /// <param name="field">字段表达式所表示的字段。</param>
        internal FieldExpression(Field field)
        {
            _field = field;
        }

        /// <summary>
        ///     获取字段表达式所表示的字段。
        /// </summary>
        public Field Field => _field;


        /// <summary>
        ///     确定指定的表达式与当前表达式是否相等。
        /// </summary>
        /// <param name="other">要与当前表达式进行比较的表达式。</param>
        protected override bool ConcreteEquals(Expression other)
        {
            var fieIdOther = other as FieldExpression;
            if (fieIdOther != null && Field == fieIdOther.Field)
                return true;
            return false;
        }

        /// <summary>
        ///     针对指定的数据源类型，返回表达式的文本表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType)
        {
            return Field.ToString(sourceType);
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将表达式表示为字符串形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数对象构造器</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //字段表达式没有参数化
            sqlParameters = new List<IDataParameter>();
            return Field.ToString(sourceType);
        }
    }
}