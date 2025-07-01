/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：复杂类型实例读取器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:09:25
└──────────────────────────────────────────────────────────────┘
*/

using System.Data;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     复杂类型实例读取器
    /// </summary>
    /// <typeparam name="T">复杂类型</typeparam>
    public class ComplexTypeInstanceReader<T> : ResultReader<T>
    {
        /// <summary>
        ///     要读取其实例的复杂类型。
        /// </summary>
        private readonly ComplexType _complexType;


        /// <summary>
        ///     创建ComplexTypeInstanceReader实例。
        /// </summary>
        /// <param name="complexType">要读取其实例的复杂类型。</param>
        /// <param name="dataReader">数据集阅读器。</param>
        /// <param name="sqlExecutor"></param>
        public ComplexTypeInstanceReader(ComplexType complexType, IDataReader dataReader, ISqlExecutor sqlExecutor) :
            base(dataReader, sqlExecutor)
        {
            _complexType = complexType;
        }

        /// <summary>
        ///     获取要读取其实例的复杂类型。
        /// </summary>
        public ComplexType ComplexType => _complexType;

        /// <summary>
        ///     从结果集读取下一个元素（值或对象）。
        /// </summary>
        /// <param name="result">返回读取结果。</param>
        /// <returns>读取成功返回true，否则返回false。</returns>
        protected override bool Read(out T result)
        {
            //读取数据行
            var dataRow = NextRow();
            //没有 返回空
            if (dataRow == null)
            {
                result = default;
                return false;
            }

            //包装成两个委托
            //Func<SimpleAttributeNode, object> 和 Func<DataRow, SimpleAttributeNode, object>
            object DataRowGetValue(DataRow row, SimpleAttributeNode node)
            {
                return row.GetValue(node);
            }

            result = (T)_complexType.Instantiate(node => DataRowGetValue(dataRow, node));

            return true;
        }
    }
}