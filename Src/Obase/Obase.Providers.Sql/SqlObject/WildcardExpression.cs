/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示通配符表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 14:51:35
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示通配符表达式。通配符表达式一般用作Count函数的参数。
    /// </summary>
    public class WildcardExpression : Expression
    {
        /// <summary>
        ///     作为通配符作用范围的源。
        /// </summary>
        private readonly ISource _source;

        /// <summary>
        ///     将指定名称的源作为通配范围，创建表示通配符表达式的WildcardExpression的实例。
        /// </summary>
        /// <param name="source">源名称。</param>
        internal WildcardExpression(string source)
        {
            _source = new SimpleSource(source);
        }

        /// <summary>
        ///     将指定的源作为通配范围，创建表示通配符表达式的WildcardExpression的实例。
        /// </summary>
        /// <param name="source">源。</param>
        internal WildcardExpression(ISource source)
        {
            _source = source;
        }

        /// <summary>
        ///     获取设定通配范围的源。
        /// </summary>
        public ISource Source => _source;


        /// <summary>
        ///     派生类实现此方法以判定具体类型的表达式对象是否相等。
        /// </summary>
        /// <param name="other">要与当前表达式进行比较的表达式。</param>
        protected override bool ConcreteEquals(Expression other)
        {
            var wildcardExpressionOther = other as WildcardExpression;
            if (wildcardExpressionOther != null && Source.Equals(wildcardExpressionOther.Source))
                return true;
            return false;
        }

        /// <summary>
        ///     针对指定的数据源类型，返回表达式的文本表示形式
        /// </summary>
        /// <param name="sourceType">数据源类型。</param>
        public override string ToString(EDataSource sourceType)
        {
            return $"{_source.ToString(sourceType)}.*";
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将表达式表示为字符串形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator">参数构造器</param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            return $"{_source.ToString(sourceType, out sqlParameters, creator)}.*";
        }
    }
}