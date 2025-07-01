/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示调用函数的表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:10:15
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示调用函数的表达式。
    /// </summary>
    public class FunctionExpression : Expression
    {
        /// <summary>
        ///     实参集合。
        /// </summary>
        private readonly Expression[] _arguments;

        /// <summary>
        ///     函数的名称。
        /// </summary>
        private readonly string _functionName;

        /// <summary>
        ///     作用于函数表达式的over子句
        /// </summary>
        private OverClause _over;

        /// <summary>
        ///     创建FunctionExpression的实例，并指定Arguments属性的值。
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="argments"></param>
        internal FunctionExpression(string functionName, Expression[] argments)
        {
            _functionName = functionName;
            _arguments = argments;
        }

        /// <summary>
        ///     获取函数名称。
        /// </summary>
        public string FunctionName => _functionName;

        /// <summary>
        ///     获取实参集合。
        /// </summary>
        public Expression[] Arguments => _arguments;

        /// <summary>
        ///     获取或设置作用于函数表达式的Over子句。
        /// </summary>
        public OverClause Over
        {
            get => _over;
            set => _over = value;
        }

        /// <summary>
        ///     判定具体类型的表达式对象是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected override bool ConcreteEquals(Expression other)
        {
            var funcOther = other as FunctionExpression;
            if (funcOther != null && FunctionName == funcOther.FunctionName && Over == funcOther.Over &&
                Arguments.SequenceEqual(funcOther.Arguments))
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
            string result;

            //识别是否为空
            string isNullStr;
            switch (sourceType)
            {
                case EDataSource.SqlServer:
                {
                    isNullStr = "isnull";
                    break;
                }
                case EDataSource.PostgreSql:
                {
                    isNullStr = "COALESCE";
                    break;
                }
                case EDataSource.Oledb:
                case EDataSource.MySql:
                case EDataSource.Oracle:
                case EDataSource.Sqlite:
                {
                    isNullStr = "ifnull";
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                }
            }

            switch (FunctionName.ToUpper())
            {
                case "CONVERT":
                {
                    switch (sourceType)
                    {
                        case EDataSource.SqlServer:
                        {
                            result =
                                $"{FunctionName}({string.Join(",", Arguments.Select(s => s.ToString(sourceType)))})";
                            break;
                        }
                        case EDataSource.PostgreSql:
                        {
                            result = FunctionName == "CONVERT"
                                ? $"{Arguments[1].ToString(sourceType)}::{Arguments[0].ToString(sourceType)}"
                                : $"{FunctionName}({string.Join(",", Arguments.Select(s => s.ToString(sourceType)))})";
                            break;
                        }
                        case EDataSource.MySql:
                        case EDataSource.Oracle:
                        {
                            //MySql的Convert函数类型仅支持以下几类BINARY,CHAR,DATE,TIME,DATETIME,DECIMAL,SIGNED,UNSIGNED
                            if (_arguments.Length > 0 && _arguments[0] is ConstantExpression constant)
                                switch (constant.Value.ToString().ToLower())
                                {
                                    case "smallint":
                                    case "int":
                                    case "bigint":
                                    case "bit":
                                    {
                                        constant = new ConstantExpression("SIGNED");
                                        _arguments[0] = constant;
                                        break;
                                    }
                                    case "varchar":
                                    case "char":
                                    {
                                        constant = new ConstantExpression("CHAR");
                                        _arguments[0] = constant;
                                        break;
                                    }
                                    case "real":
                                    case "float":
                                    case "numeric":
                                    {
                                        constant = new ConstantExpression("DECIMAL");
                                        _arguments[0] = constant;
                                        break;
                                    }
                                }

                            result =
                                $"{FunctionName}({string.Join(",", Arguments.Reverse().Select(s => s.ToString(sourceType)))})";
                            break;
                        }
                        case EDataSource.Sqlite:
                        {
                            //Sqlite翻译为Cast(xx as xx)
                            //Sqlite的Cast函数类型仅支持以下几类TEXT,REAL,INTEGER
                            if (_arguments.Length > 0 && _arguments[0] is ConstantExpression constant)
                                switch (constant.Value.ToString().ToLower())
                                {
                                    case "smallint":
                                    case "int":
                                    case "bigint":
                                    case "bit":
                                    {
                                        constant = new ConstantExpression("INTEGER");
                                        _arguments[0] = constant;
                                        break;
                                    }
                                    case "varchar":
                                    case "char":
                                    {
                                        constant = new ConstantExpression("TEXT");
                                        _arguments[0] = constant;
                                        break;
                                    }
                                    case "real":
                                    case "float":
                                    case "numeric":
                                    {
                                        constant = new ConstantExpression("REAL");
                                        _arguments[0] = constant;
                                        break;
                                    }
                                }

                            result =
                                $"CAST({string.Join(" as ", Arguments.Reverse().Select(s => s.ToString(sourceType)))})";
                            break;
                        }
                        default:
                        {
                            throw new ArgumentOutOfRangeException(nameof(sourceType), $"不支持的数据源{sourceType}");
                        }
                    }

                    break;
                }
                case "Average":
                {
                    result =
                        $"{isNullStr}(Avg(cast({string.Join(",", Arguments.Select(s => s.ToString(sourceType)))} as decimal(10,2))),0)";
                    break;
                }
                case "MAX":
                {
                    result = $"{isNullStr}(max({string.Join(",", Arguments.Select(s => s.ToString(sourceType)))}),0)";
                    break;
                }
                case "MIN":
                {
                    result = $"{isNullStr}(min({string.Join(",", Arguments.Select(s => s.ToString(sourceType)))}),0)";
                    break;
                }
                case "SUM":
                {
                    result = $"{isNullStr}(sum({string.Join(",", Arguments.Select(s => s.ToString(sourceType)))}),0)";
                    break;
                }
                case "CONCAT":
                {
                    result = sourceType == EDataSource.Sqlite || sourceType == EDataSource.PostgreSql
                        ? $"{string.Join("||", Arguments.Select(s => s.ToString(sourceType)))}"
                        : $"{FunctionName}({string.Join(",", Arguments.Select(s => s.ToString(sourceType)))})";
                    break;
                }
                default:
                {
                    result = $"{FunctionName}({string.Join(",", Arguments.Select(s => s.ToString(sourceType)))})";
                    break;
                }
            }

            if (Over != null) result += Over.ToString(sourceType);

            return result;
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
            //函数表达式没有参数化
            sqlParameters = new List<IDataParameter>();

            return ToString(sourceType);
        }
    }
}