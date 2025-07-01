/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示LIKE运算的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:24:41
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示LIKE运算的表达式。
    /// </summary>
    public class LikeExpression : BinaryExpression
    {
        /// <summary>
        ///     Like的类型
        /// </summary>
        private readonly ELikeType _likeType;

        /// <summary>
        ///     创建LikeExpression的实例，并设置Left属性和Pattern属性的值。
        /// </summary>
        /// <param name="left">左操作数。</param>
        /// <param name="pattern">匹配模式。</param>
        /// <param name="likeType">Like的类型</param>
        internal LikeExpression(Expression left, Expression pattern, ELikeType likeType = ELikeType.Contains)
            : base(left, null)
        {
            _likeType = likeType;
            Pattern = pattern;
        }

        /// <summary>
        ///     获取匹配模式。
        /// </summary>
        public Expression Pattern { get; }


        /// <summary>
        ///     判定具体类型的表达式对象是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected override bool ConcreteEquals(Expression other)
        {
            var likeOther = other as LikeExpression;
            if (likeOther != null && Pattern == likeOther.Pattern)
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
            //模式匹配字符串 将单引号转义
            var patternStr = Pattern.ToString(sourceType).Replace("'", "\'");

            string pattern;
            switch (sourceType)
            {
                case EDataSource.MySql:
                case EDataSource.PostgreSql:
                case EDataSource.SqlServer:
                    pattern = $"'{patternStr}'";
                    break;
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                    pattern = patternStr;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, $"未知的数据源{sourceType}");
            }

            return $"{Left.ToString(sourceType)} LIKE {pattern}";
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将表达式表示为字符串形式
        /// </summary>
        /// <param name="sourceType">数据源类型</param>
        /// <param name="sqlParameters">参数列表</param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public override string ToString(EDataSource sourceType, out List<IDataParameter> sqlParameters,
            IParameterCreator creator)
        {
            //此处模式匹配仅为代号
            var patternStr = Pattern.ToString(sourceType, out var patternSqlParameter, creator);


            string result;
            List<IDataParameter> leftSqlParameter;
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    //处理patternSqlParameter 此集合内仅一条
                    var pattenPara = patternSqlParameter.FirstOrDefault();
                    if (pattenPara != null)
                    {
                        //匹配值 四种可能 前% 后% 全% 中间
                        var pattenValue = pattenPara.Value.ToString();
                        if (pattenValue.StartsWith("%") && !pattenValue.EndsWith("%"))
                        {
                            pattenPara.Value = pattenValue.TrimStart('%');
                            result =
                                $"{Left.ToString(sourceType, out leftSqlParameter, creator)} LIKE '%'+ {patternStr}";
                        }
                        else if (pattenValue.StartsWith("%") && pattenValue.EndsWith("%"))
                        {
                            pattenPara.Value = pattenValue.TrimStart('%').TrimEnd('%');
                            result =
                                $"{Left.ToString(sourceType, out leftSqlParameter, creator)} LIKE '%' + {patternStr} + '%'";
                        }
                        else if (!pattenValue.StartsWith("%") && pattenValue.EndsWith("%"))
                        {
                            pattenPara.Value = pattenValue.TrimEnd('%');
                            result =
                                $"{Left.ToString(sourceType, out leftSqlParameter, creator)} LIKE {patternStr} + '%'";
                        }
                        else
                        {
                            throw new ArgumentException("Like 表达式错误:未找到模式匹配部分中的头部或尾部%.");
                        }

                        break;
                    }

                    //匹配值 四种可能 前% 后% 全% 中间
                    var realValuePlaceHolder = Left.ToString(sourceType, out leftSqlParameter, creator);

                    if (_likeType == ELikeType.EndWith)
                        result = realValuePlaceHolder + " LIKE '%' + " + patternStr + "";
                    else if (_likeType == ELikeType.Contains)
                        result = realValuePlaceHolder + " LIKE '%' + " + patternStr + ",'%'";
                    else if (_likeType == ELikeType.StartWith)
                        result = realValuePlaceHolder + " LIKE " + patternStr + "+ '%'";
                    else
                        throw new ArgumentException("Like 表达式错误:未找到模式匹配部分.");
                    break;
                }
                case EDataSource.PostgreSql:
                case EDataSource.Oracle:
                case EDataSource.MySql:
                {
                    //处理patternSqlParameter 此集合内仅一条
                    var pattenPara = patternSqlParameter.FirstOrDefault();
                    if (pattenPara != null)
                    {
                        //匹配值 四种可能 前% 后% 全% 中间
                        var pattenValue = pattenPara.Value.ToString();
                        if (pattenValue.StartsWith("%") && !pattenValue.EndsWith("%"))
                        {
                            pattenPara.Value = pattenValue.TrimStart('%');
                            result =
                                $"{Left.ToString(sourceType, out leftSqlParameter, creator)} LIKE concat('%',{patternStr})";
                        }
                        else if (pattenValue.StartsWith("%") && pattenValue.EndsWith("%"))
                        {
                            pattenPara.Value = pattenValue.TrimStart('%').TrimEnd('%');
                            result =
                                $"{Left.ToString(sourceType, out leftSqlParameter, creator)} LIKE concat('%',{patternStr},'%')";
                        }
                        else if (!pattenValue.StartsWith("%") && pattenValue.EndsWith("%"))
                        {
                            pattenPara.Value = pattenValue.TrimEnd('%');
                            result =
                                $"{Left.ToString(sourceType, out leftSqlParameter, creator)} LIKE concat({patternStr},'%')";
                        }
                        else
                        {
                            throw new ArgumentException("Like 表达式错误:未找到模式匹配部分中的头部或尾部%.");
                        }

                        break;
                    }

                    //匹配值 四种可能 前% 后% 全% 中间
                    var realValuePlaceHolder = Left.ToString(sourceType, out leftSqlParameter, creator);

                    if (_likeType == ELikeType.EndWith)
                        result = realValuePlaceHolder + " LIKE concat('%'," + patternStr + ")";
                    else if (_likeType == ELikeType.Contains)
                        result = realValuePlaceHolder + " LIKE concat('%'," + patternStr + ",'%')";
                    else if (_likeType == ELikeType.StartWith)
                        result = realValuePlaceHolder + " LIKE concat(" + patternStr + ", '%')";
                    else
                        throw new ArgumentException("Like 表达式错误:未找到模式匹配部分中的头部或尾部%.");

                    break;
                }
                case EDataSource.Sqlite:
                {
                    //处理patternSqlParameter 此集合内仅一条
                    var pattenPara = patternSqlParameter.FirstOrDefault();
                    if (pattenPara != null)
                    {
                        //匹配值 四种可能 前% 后% 全% 中间
                        result =
                            $"{Left.ToString(sourceType, out leftSqlParameter, creator)} LIKE {patternStr}";
                        break;
                    }

                    //匹配值 四种可能 前% 后% 全% 中间
                    var realValuePlaceHolder = Left.ToString(sourceType, out leftSqlParameter, creator);

                    result = realValuePlaceHolder +
                             " LIKE " + patternStr;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, $"未知的数据源{sourceType}");
            }


            //参数列表
            sqlParameters = new List<IDataParameter>();
            sqlParameters.AddRange(leftSqlParameter);
            sqlParameters.AddRange(patternSqlParameter);

            return result;
        }
    }
}