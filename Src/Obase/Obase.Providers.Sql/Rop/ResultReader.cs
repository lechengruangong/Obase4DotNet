/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结果读取器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:48:22
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     结果读取器。用于从结果集中读取一个元素（值或对象）。
    ///     类型参数：
    ///     T	元素的类型
    /// </summary>
    public abstract class ResultReader<T> : IEnumerable<T>
    {
        /// <summary>
        ///     Sql执行器
        /// </summary>
        private readonly ISqlExecutor _sqlExecutor;

        /// <summary>
        ///     别名生成器
        /// </summary>
        protected readonly AliasGenerator AliasGenerator = new AliasGenerator { EnableCache = true };

        /// <summary>
        ///     数据读取器，负责从数据库读取数据。
        /// </summary>
        protected readonly IDataReader DataReader;

        /// <summary>
        ///     映射字段生成器
        /// </summary>
        protected readonly TargetFieldGenerator TargetFieldGenerator = new TargetFieldGenerator { EnableCache = true };


        /// <summary>
        ///     构造ResultReader的新实例。
        /// </summary>
        /// <param name="dataReader">数据读取器，负责从数据库读取数据。</param>
        /// <param name="sqlExecutor">SQL执行器</param>
        protected ResultReader(IDataReader dataReader, ISqlExecutor sqlExecutor)
        {
            DataReader = dataReader;
            _sqlExecutor = sqlExecutor;
        }

        /// <summary>
        ///     获取迭代器
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator GetEnumerator()
        {
            try
            {
                while (Read(out var temp))
                    yield return temp;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        ///     实现接口方法 获取迭代器
        /// </summary>
        /// <returns></returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumeratorT();
        }

        /// <summary>
        ///     获取迭代器(泛型)
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator<T> GetEnumeratorT()
        {
            try
            {
                while (Read(out var temp))
                    yield return temp;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        ///     从结果集读取下一个元素（值或对象）。
        /// </summary>
        /// <param name="result">返回读取结果。</param>
        /// <returns>读取成功返回true，否则返回false。</returns>
        protected abstract bool Read(out T result);

        /// <summary>
        ///     关闭读取器。
        /// </summary>
        protected void Close()
        {
            DataReader?.Close();
            DataReader?.Dispose();
            //如果是执行模式 那就是由执行器开启的连接 此处需要关闭
            if (_sqlExecutor.ConnectionMode == EConnectionMode.Execution)
                _sqlExecutor?.CloseConnection();
        }

        /// <summary>
        ///     将数据读取器移动到下一行。
        /// </summary>
        /// <returns>如果数据读取器已关闭或已到末尾返回null；否则返回一个数据项字典，其中键为列名，值为当前行在该列上的值。</returns>
        protected DataRow NextRow()
        {
            var closted = DataReader.IsClosed;
            if (closted)
                return null;
            //没有数据的时候
            if (!DataReader.Read())
            {
                Close();
                return null;
            }

            //数据行
            var dataRow = new DataRow(AliasGenerator, TargetFieldGenerator);
            //获取列数据
            for (var i = 0; i < DataReader.FieldCount; i++)
            {
                var name = DataReader.GetName(i);
                var obj = DataReader.GetValue(i);

                dataRow.Add(name, obj, i);
            }

            return dataRow;
        }
    }
}