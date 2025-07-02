/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：扩展成员表达式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:46:21
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using System.Collections.Generic;
using System.Data;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     对象读取器，负责从结果集读取对象。
    ///     类型参数：
    ///     T	结果集中的对象的类型
    /// </summary>
    public class ObjectReader<T> : ResultReader<T>
    {
        /// <summary>
        ///     数据分配器
        /// </summary>
        private readonly DataRowAssigner _dataRowAssigner;

        /// <summary>
        ///     DataRowAssignment存储
        /// </summary>
        private readonly DataRowAssignmentSet _dataRowAssignmentSet;

        /// <summary>
        ///     表示挂起的所有包含运算的关联树。
        /// </summary>
        private readonly AssociationTree _includingTree;

        /// <summary>
        ///     对象建造器
        /// </summary>
        private readonly ObjectSystemBuilder _objectBuilder;

        /// <summary>
        ///     对象的模型类型。
        /// </summary>
        private readonly ReferringType _objectType;

        /// <summary>
        ///     构造ObjectReader的新实例。
        /// </summary>
        /// <param name="objectType">要读取的对象的模型类型。</param>
        /// <param name="includingTree">包含所有挂起的包含运算的关联树。</param>
        /// <param name="dataReader">数据读取器，负责从数据库读取数据。</param>
        /// <param name="sqlExecutor">Sql执行器</param>
        /// <param name="attachObject">对象附加委托。不指定则不附加</param>
        /// <param name="attachRoot">指示是否附加根对象</param>
        public ObjectReader(ReferringType objectType, AssociationTree includingTree, IDataReader dataReader,
            ISqlExecutor sqlExecutor,
            AttachObject attachObject = null, bool attachRoot = true)
            : base(dataReader, sqlExecutor)
        {
            _objectType = objectType;
            _includingTree = includingTree;
            _dataRowAssignmentSet = new DataRowAssignmentSet();
            _objectBuilder = new ObjectSystemBuilder(_dataRowAssignmentSet, attachObject, attachRoot);
            _dataRowAssigner = new DataRowAssigner(_dataRowAssignmentSet);
        }

        /// <summary>
        ///     获取要读取的对象的模型类型。
        /// </summary>
        public ReferringType ObjectType => _objectType;

        /// <summary>
        ///     获取迭代器
        /// </summary>
        /// <returns></returns>
        public override IEnumerator GetEnumerator()
        {
            try
            {
                while (Read(out var temp)) yield return temp;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        ///     获取迭代器(泛型)
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator<T> GetEnumeratorT()
        {
            try
            {
                while (Read(out var temp)) yield return temp;
            }
            finally
            {
                Close();
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     从结果集读取下一个元素（值或对象）。
        /// </summary>
        /// <param name="result">返回读取结果。</param>
        /// <returns>读取成功返回true，否则返回false。</returns>
        protected override bool Read(out T result)
        {
            var buildResult = default(T);
            var tagetReturn = false; //输出

            while (true)
            {
                //读取下一行
                var dataRow = NextRow();

                if (dataRow == null && _dataRowAssignmentSet.IsEmpty)
                {
                    //什么也没有
                    result = default;
                    return false;
                }

                if (dataRow == null && !_dataRowAssignmentSet.IsEmpty)
                {
                    //建造对象
                    result = (T)_includingTree.Accept(_objectBuilder);
                    _dataRowAssignmentSet.Clear();
                    return true;
                }

                if (dataRow != null)
                {
                    if (!_dataRowAssignmentSet.ContainEquivalent(_includingTree.Node, dataRow) &&
                        !_dataRowAssignmentSet.IsEmpty)
                    {
                        //建造对象
                        buildResult = (T)_includingTree.Accept(_objectBuilder);
                        _dataRowAssignmentSet.Clear();
                        tagetReturn = true;
                    }

                    _dataRowAssigner.SetDataRow(dataRow);
                    _includingTree.Accept(_dataRowAssigner);
                }

                if (dataRow == null || tagetReturn)
                {
                    result = buildResult;
                    return true;
                }
            }
        }
    }
}