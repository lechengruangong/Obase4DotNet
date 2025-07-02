/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结果读取器工厂.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:50:36
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Data;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     结果读取器工厂。
    /// </summary>
    public class ResultReaderFactory
    {
        /// <summary>
        ///     创建具体的结果读取器。
        /// </summary>
        /// <param name="dataReader">数据集阅读器。</param>
        /// <param name="resultType">结果的类型。</param>
        /// <param name="includingTree">包含树。</param>
        /// <param name="sqlExecutor">Sql执行器</param>
        /// <param name="attachObj">对象附加委托。不指定则不附加</param>
        /// <param name="attachRoot">指示是否附加根对象</param>
        public IEnumerable Create(IDataReader dataReader, TypeBase resultType, AssociationTree includingTree,
            ISqlExecutor sqlExecutor,
            AttachObject attachObj = null, bool attachRoot = true)
        {
            IEnumerable reader = null;
            //创建各自类型的读取器
            if (resultType is PrimitiveType)
            {
                var type = typeof(ValueReader<>).MakeGenericType(resultType.ClrType);
                reader = (IEnumerable)Activator.CreateInstance(type, dataReader, sqlExecutor);
            }
            else if (resultType is ObjectType objectType)
            {
                var type = typeof(ObjectReader<>).MakeGenericType(objectType.ClrType);
                reader = (IEnumerable)Activator.CreateInstance(type, objectType, includingTree,
                    dataReader, sqlExecutor, attachObj, attachRoot);
            }
            else if (resultType is ComplexType complexType)
            {
                var type = typeof(ComplexTypeInstanceReader<>).MakeGenericType(resultType.ClrType);
                reader = (IEnumerable)Activator.CreateInstance(type, complexType, dataReader, sqlExecutor);
            }
            else if (resultType is TypeView typeView)
            {
                if (typeView.ViewReferences.Length > 0)
                {
                    var type = typeof(ObjectReader<>).MakeGenericType(typeView.ClrType);
                    reader = (IEnumerable)Activator.CreateInstance(type, typeView, includingTree,
                        dataReader, sqlExecutor, attachObj, attachRoot);
                }
                else
                {
                    var type = typeof(SimpleTypeViewInstanceReader<>).MakeGenericType(typeView.ClrType);
                    reader = (IEnumerable)Activator.CreateInstance(type, typeView, dataReader, sqlExecutor);
                }
            }

            if (reader == null) throw new ArgumentException("没有合适的结果读取器");
            return reader;
        }
    }
}