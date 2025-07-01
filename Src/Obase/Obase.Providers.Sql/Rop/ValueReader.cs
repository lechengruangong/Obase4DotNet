/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：值读取器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:14:28
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Data;
using Obase.Core.Common;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     值读取器，负责从结果集读取值。
    ///     类型参数：
    ///     T	结果集中的值的类型
    /// </summary>
    public class ValueReader<T> : ResultReader<T>
    {
        /// <summary>
        ///     构造ValueReader的新实例。
        /// </summary>
        /// <param name="dataReader">数据读取器，负责从数据库读取数据。</param>
        /// <param name="sqlExecutor"></param>
        public ValueReader(IDataReader dataReader, ISqlExecutor sqlExecutor) : base(dataReader, sqlExecutor)
        {
        }

        /// <summary>
        ///     从结果集读取下一个元素（值或对象）。
        /// </summary>
        /// <param name="result">返回读取结果。</param>
        /// <returns>读取成功返回true，否则返回false。</returns>
        protected override bool Read(out T result)
        {
            //读取数据行
            var dataRow = NextRow();
            //为空 则返回默认值
            if (dataRow == null)
            {
                result = default;
                return false;
            }

            //读取第一行
            var obj = dataRow.GetValue(0);
            if (obj == null || obj is DBNull)
            {
                result = default;
                return true;
            }

            obj = Utils.ConvertDbValue(obj, typeof(T));
            result = (T)obj;
            return true;
        }
    }
}