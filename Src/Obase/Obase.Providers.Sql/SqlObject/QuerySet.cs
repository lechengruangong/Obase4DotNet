/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示对两个查询结果执行集运算的Sql语句.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:29:26
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using Obase.Core.Query;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示对两个查询结果执行集运算的Sql语句。
    /// </summary>
    public class QuerySet : ISetOperand
    {
        /// <summary>
        ///     左操作数
        /// </summary>
        private readonly ISetOperand _left;

        /// <summary>
        ///     集运算操作符。
        /// </summary>
        private readonly ESetOperator _operator;

        /// <summary>
        ///     右操作数
        /// </summary>
        private readonly ISetOperand _right;

        /// <summary>
        ///     创建QuerySet实例，同时指定左操作数、右操作数和运算符。
        /// </summary>
        /// <param name="left">作为左操作数的查询Sql语句。</param>
        /// <param name="right">作为右操作数的查询Sql语句。</param>
        /// <param name="eOperator">集运算符。</param>
        public QuerySet(ISetOperand left, ISetOperand right, ESetOperator eOperator)
        {
            _left = left;
            _right = right;
            _operator = eOperator;
        }


        /// <summary>
        ///     获取集运算操作符。
        /// </summary>
        public ESetOperator Operator => _operator;

        /// <summary>
        ///     获取作为集运算左操作数的查询Sql语句。
        /// </summary>
        public ISetOperand Left => _left;

        /// <summary>
        ///     获取作为集运算右操作数的查询Sql语句。
        /// </summary>
        public ISetOperand Right => _right;

        /// <summary>
        ///     根据查询Sql语句的对象表示法生成参数化Sql语句。
        /// </summary>
        /// <param name="parameters">返回字符串中的参数及其值的集合。</param>
        /// <param name="creator"></param>
        public string ToSql(out List<IDataParameter> parameters, IParameterCreator creator)
        {
            return ToSql(EDataSource.SqlServer, out parameters, creator);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public string ToSql(EDataSource sourceType)
        {
            string result;
            //对每个部分处理
            switch (_operator)
            {
                case ESetOperator.Concat:
                    result = $"{_left.ToSql(sourceType)} union all {_right.ToSql(sourceType)}";
                    break;
                case ESetOperator.Except:
                    if (sourceType == EDataSource.MySql) throw new ArgumentException("MySql不支持Except运算.");
                    result = $"{_left.ToSql(sourceType)} except {_right.ToSql(sourceType)}";
                    break;
                case ESetOperator.Interact:
                    if (sourceType == EDataSource.MySql) throw new ArgumentException("MySql不支持Interact运算.");
                    result = $"{_left.ToSql(sourceType)} intersect {_right.ToSql(sourceType)}";
                    break;
                case ESetOperator.Union:
                    result = $"{_left?.ToSql(sourceType)} union {_right?.ToSql(sourceType)}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_operator), $"未知的集运算类型{_operator}.");
            }

            return result;
        }

        /// <summary>
        ///     使用参数化的方式 和 指定的数据源 将Sql对象表示为Sql字符串
        /// </summary>
        /// <param name="sourceType">指定的数据源</param>
        /// <param name="parameters">参数</param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public string ToSql(EDataSource sourceType, out List<IDataParameter> parameters, IParameterCreator creator)
        {
            //参数集合
            parameters = new List<IDataParameter>();
            var parameterLeft = new List<IDataParameter>();
            var parameterRight = new List<IDataParameter>();
            string result;
            //对每个部分处理
            switch (_operator)
            {
                case ESetOperator.Concat:
                    result =
                        $"{_left.ToSql(sourceType, out parameterLeft, creator)} union all {_right.ToSql(sourceType, out parameterRight, creator)}";
                    break;
                case ESetOperator.Except:
                    if (sourceType == EDataSource.MySql) throw new ArgumentException("MySql不支持Except运算.");
                    result =
                        $"{_left.ToSql(sourceType, out parameterLeft, creator)} except {_right.ToSql(sourceType, out parameterRight, creator)}";
                    break;
                case ESetOperator.Interact:
                    if (sourceType == EDataSource.MySql) throw new ArgumentException("MySql不支持Interact运算.");
                    result =
                        $"{_left.ToSql(sourceType, out parameterLeft, creator)} intersect {_right.ToSql(sourceType, out parameterRight, creator)}";
                    break;
                case ESetOperator.Union:
                    result =
                        $"{_left?.ToSql(sourceType, out parameterLeft, creator)} union {_right?.ToSql(sourceType, out parameterRight, creator)}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_operator), $"未知的集运算类型{_operator}.");
            }

            parameters.AddRange(parameterLeft);
            parameters.AddRange(parameterRight);
            return result;
        }
    }
}