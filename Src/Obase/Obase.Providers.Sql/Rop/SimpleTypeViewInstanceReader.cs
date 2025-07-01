/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：简单视图实例读取器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:09:59
└──────────────────────────────────────────────────────────────┘
*/

using System.Data;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     简单视图实例读取器。
    ///     简单视图是指不包含引用元素的视图。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleTypeViewInstanceReader<T> : ResultReader<T>
    {
        /// <summary>
        ///     要读取其实例的视图。
        /// </summary>
        private readonly TypeView _typeView;

        /// <summary>
        ///     创建SimpleTypeViewInstanceReader实例。
        /// </summary>
        /// <param name="typeView">要读取其实例的类型视图。</param>
        /// <param name="dataReader">数据集阅读器。</param>
        /// <param name="sqlExecutor"></param>
        public SimpleTypeViewInstanceReader(TypeView typeView, IDataReader dataReader, ISqlExecutor sqlExecutor) : base(
            dataReader, sqlExecutor)
        {
            _typeView = typeView;
        }

        /// <summary>
        ///     获取要读取其实例的视图。
        /// </summary>
        public TypeView TypeView => _typeView;

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

            result = (T)_typeView.Instantiate(node => DataRowGetValue(dataRow, node));

            return true;
        }
    }
}