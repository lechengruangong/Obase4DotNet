/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：映射字段生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:12:33
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;

namespace Obase.Providers.Sql.Rop
{
    /// <summary>
    ///     映射字段生成器。
    ///     实施说明
    ///     如果根节点为简单属性，直接返回TargetField属性的值。如果为复杂属性，则沿属性路径，以MappingConnectionChar指定的字符依次将映射目标
    ///     串联起来即构成完整的映射字段。详见活动图“映射字段生成器”。
    /// </summary>
    public class TargetFieldGenerator : IAttributeTreeUpwardVisitor<string>
    {
        /// <summary>
        ///     用于缓存生成结果的字典，其键为属性树节点，值为该节点所代表的属性的映射目标。
        /// </summary>
        private readonly Dictionary<AttributeTreeNode, string>
            _fieldCache = new Dictionary<AttributeTreeNode, string>();

        /// <summary>
        ///     指示是否启用缓存。
        /// </summary>
        private bool _enableCache;

        /// <summary>
        ///     生成结果
        /// </summary>
        private string _result;

        /// <summary>
        ///     获取或设置一个值，该值指示是否启用缓存。
        /// </summary>
        public bool EnableCache
        {
            get => _enableCache;
            set => _enableCache = value;
        }

        /// <summary>
        ///     前置访问，即在访问父级前执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        /// <param name="outChildState">返回一个状态数据，在遍历到父级时该数据将被视为子级状态。</param>
        /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
        public bool Previsit(AttributeTree subTree, object childState, out object outChildState,
            out object outPrevisitState)
        {
            //直接返回
            if (!_enableCache)
            {
                outChildState = null;
                outPrevisitState = null;
                return true;
            }

            //如果是根节点
            if (subTree.Parent == null)
            {
                outChildState = null;
                outPrevisitState = null;
                _result = null;
                return false;
            }

            //查找换成
            string cacheValue = null;
            if (_fieldCache.TryGetValue(subTree.Node, out var value)) cacheValue = value;

            if (cacheValue == null)
            {
                outChildState = null;
                outPrevisitState = null;
                return true;
            }

            _result = cacheValue;
            outPrevisitState = true;
            outChildState = null;
            return false;
        }

        /// <summary>
        ///     后置访问，即在访问父级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="childState">访问子级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AttributeTree subTree, object childState, object previsitState)
        {
            //前置结果为true
            if (previsitState is bool boolState && boolState)
                return;
            //取属性的字段
            var attributeField = subTree.Attribute.TargetField;
            _result = $"{_result}{attributeField}";
            //如果不是复杂属性 则到此结束
            if (!subTree.IsComplex)
                return;
            //如果到底 且 为复杂属性
            if (subTree.SubTrees == null || subTree.SubTrees.Length == 0)
                throw new ExpressionIllegalException(null, "该属性路径未指向一个简单属性,不能生成字段");
            //此时肯定为复杂属性
            var connectChar = ((ComplexAttribute)subTree.Attribute).MappingConnectionChar;
            //写入结果 存入缓存
            _result = connectChar == char.MinValue ? "" : $"{_result}{connectChar}";
            _fieldCache[subTree.Node] = _result;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _result = null;
        }

        /// <summary>
        ///     获取遍历属性树的结果。
        /// </summary>
        public string Result => _result;
    }
}