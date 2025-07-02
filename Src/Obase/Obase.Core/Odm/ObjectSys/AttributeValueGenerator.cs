/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性值生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:24:56
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     属性值生成器。
    /// </summary>
    public class AttributeValueGenerator : IAttributeTreeDownwardVisitor
    {
        /// <summary>
        ///     一个委托，用于获取属性树节点代表的简单属性的值。
        /// </summary>
        private readonly Func<SimpleAttributeNode, object> _attributeValueGetter;

        /// <summary>
        ///     临时值存储字典
        /// </summary>
        private readonly Dictionary<string, object> _tempDict = new Dictionary<string, object>();

        /// <summary>
        ///     遍历属性树的结果
        /// </summary>
        private object _result;

        /// <summary>
        ///     创建AttributeValueGenerator实例。
        /// </summary>
        /// <param name="attrValueGetter">一个委托，用于获取属性树节点代表的简单属性的值。</param>
        public AttributeValueGenerator(Func<SimpleAttributeNode, object> attrValueGetter)
        {
            _attributeValueGetter = attrValueGetter;
        }

        /// <summary>
        ///     获取遍历属性树的结果。
        /// </summary>
        public object Result => _result;

        /// <summary>
        ///     后置访问，即在访问子级后执行操作。
        /// </summary>
        /// <param name="subTree">被访问的子树。</param>
        /// <param name="parentState">访问父级时产生的状态数据。</param>
        /// <param name="previsitState">前置访问产生的状态数据。</param>
        public void Postvisit(AttributeTree subTree, object parentState, object previsitState)
        {
            if (!subTree.IsComplex) //当前节点是否为复杂属性
            {
                //是根节点 则node必为simple 取值或为null
                _result = subTree.Parent == null && _attributeValueGetter != null
                    ? _attributeValueGetter((SimpleAttributeNode)subTree.Node)
                    : null;
            }
            //处理复杂节点
            else
            {
                var type = subTree.AttributeType;
                if (type is StructuralType structuralType)
                {
                    //构造实例化结构类型的委托
                    //实际上这里用了个local方法
                    object ArgGetter(Parameter p)
                    {
                        var elemet = p.GetElement();
                        if (elemet is Attribute attr)
                        {
                            var value = attr.IsComplex
                                ? _tempDict[attr.Name]
                                : _attributeValueGetter((SimpleAttributeNode)subTree.Node);

                            if (p.ValueConverter != null) return p.ValueConverter(value);
                        }

                        return p.GetElement();
                    }

                    //构造结果
                    //var targetObject = structuralType.Instantiate(_attributeValueGetter);
                    var targetObject = structuralType.Instantiate(ArgGetter);

                    //结果为值类型
                    if (targetObject.GetType().IsValueType)
                        //进行包装
                        targetObject = new StructWrapper(targetObject);

                    //为属性设值
                    foreach (var item in subTree.SubTrees ?? Array.Empty<AttributeTree>())
                        //有些值在构造时就以被赋值 这些值不用处理
                        if (structuralType.Constructor.GetParameterByElement(item.AttributeName) == null)
                        {
                            if (item.IsComplex)
                            {
                                item.Attribute.SetValue(targetObject, _tempDict[subTree.AttributeName]);
                            }
                            else
                            {
                                var val = _attributeValueGetter((SimpleAttributeNode)item.Node);
                                if (val != null)
                                    item.Attribute.SetValue(targetObject, val);
                            }
                        }

                    object valueObject = null;
                    //设置完属性后 结果为值类型 拆包
                    if (targetObject is StructWrapper structWrapper)
                        valueObject = structWrapper.Struct;
                    else
                        valueObject = targetObject;

                    //是根节点
                    if (subTree.Parent == null)
                    {
                        //最终结果
                        _result = valueObject;
                        _tempDict.Clear();
                    }
                    else
                    {
                        //暂存
                        _tempDict[subTree.AttributeName] = valueObject;
                    }
                }
            }
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
        }

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset()
        {
            //Nothing to Do
        }
    }
}