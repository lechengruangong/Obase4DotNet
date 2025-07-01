/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Count 字段生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:11:15
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Obase.Core.Odm.ObjectSys;
using Obase.Providers.Sql.SqlObject;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     Count 字段生成器
    /// </summary>
    public class CountingFieldGenerator : IAttributeTreeDownwardVisitor<FieldExpression[]>
    {
        /// <summary>
        ///     作为生成结果的字段表达式。
        /// </summary>
        private readonly List<FieldExpression> _fieldExpressions = new List<FieldExpression>();

        /// <summary>
        ///     字段所属的源。
        /// </summary>
        private readonly MonomerSource _source;

        /// <summary>
        ///     映射字段生成器。
        /// </summary>
        private readonly TargetFieldGenerator _targetFieldGenerator = new TargetFieldGenerator();

        /// <summary>
        ///     构造Count 字段生成器
        /// </summary>
        /// <param name="source">源</param>
        public CountingFieldGenerator(MonomerSource source)
        {
            _source = source;
        }

        /// <summary>
        ///     前置访问，即在访问子级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public void Previsit(AttributeTree subTree, object parentState, out object outParentState,
            out object outPrevisitState)
        {
            outParentState = null;
            outPrevisitState = null;

            if (subTree.IsComplex)
                return;
            //加入字段
            var targetFiled = subTree.Accept(_targetFieldGenerator);
            _fieldExpressions.Add(new FieldExpression(new Field(_source, targetFiled)));
        }

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AttributeTree subTree, object parentState, object previsitState)
        {
            //Nothing to do
        }

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset()
        {
            //Nothing to Do
        }

        /// <summary>
        ///     获取遍历属性树的结果。
        /// </summary>
        public FieldExpression[] Result => _fieldExpressions.ToArray();
    }
}