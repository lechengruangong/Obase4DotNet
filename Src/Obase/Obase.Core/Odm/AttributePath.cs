/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性路径,属性树中某一节点相对于根节点的寻址结构.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 09:53:35
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示属性路径。
    ///     将对象型视为根节点，该对象型直接包含的所有属性视为一级节点。这些属性中有一部分为复杂属性，复杂属性的类型所具有的属性视为该属性的子属性。依此类推，可以将对象的属
    ///     性体系视为一个树型结构，称为该对象或对象类型的属性树。
    ///     属性路径是指属性树中某一节点相对于根节点的寻址结构，形式上可以表示成：/一级属性/二级属性/.../目标属性。
    /// </summary>
    public class AttributePath : IEnumerable<Attribute>
    {
        /// <summary>
        ///     表示属性路径的各个节点。0个节点表示当前路径指向属性树的根。
        /// </summary>
        private readonly List<Attribute> _attributes = new List<Attribute>();

        /// <summary>
        ///     属性树的类型。
        /// </summary>
        private readonly StructuralType _modelType;

        /// <summary>
        ///     枚举器
        /// </summary>
        private IEnumerator<Attribute> _enumerator;

        /// <summary>
        /// </summary>
        private AttributePath _parent;

        /// <summary>
        ///     创建指定类型的AttributePath实例。
        /// </summary>
        /// <param name="modelType">类型。</param>
        public AttributePath(StructuralType modelType)
        {
            _modelType = modelType;
        }


        /// <summary>
        ///     获取属性树的类型。
        /// </summary>
        public StructuralType ModelType => _modelType;

        /// <summary>
        ///     获取属性路径指向的属性。
        /// </summary>
        public Attribute Current => _enumerator.Current;

        /// <summary>
        ///     获取属性路径指向的属性的父属性，即路径的上一级。
        /// </summary>
        public AttributePath Parent => _parent;

        /// <summary>
        ///     获取枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Attribute> GetEnumerator()
        {
            return (_enumerator ?? (_enumerator = _attributes?.GetEnumerator())) ??
                   throw new InvalidOperationException("无法获取GetEnumerator");
        }

        /// <summary>
        ///     获取枚举器
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_enumerator ?? (_enumerator = _attributes?.GetEnumerator())) ??
                   throw new InvalidOperationException("无法获取GetEnumerator");
        }

        /// <summary>
        ///     将属性路径表示成字符串形式。格式为：/一级属性/二级属性/.../目标属性。
        /// </summary>
        public override string ToString()
        {
            var pathBuilder = new StringBuilder("/");
            //组合各个属性的名称
            for (var i = 0; i < _attributes.Count; i++)
                pathBuilder.Append(i == 0 ? _attributes[i].Name : $"/{_attributes[i].Name}");

            return pathBuilder.ToString();
        }

        /// <summary>
        ///     沿属性树向下，将属性路径向下延伸到指定节点（属性）。
        /// </summary>
        /// <param name="attribute">目标属性。</param>
        public AttributePath GoDown(Attribute attribute)
        {
            //加入根节点
            var parentPath = new AttributePath(_modelType);
            parentPath._attributes.AddRange(_attributes);
            _parent = parentPath;
            //加入目标节点
            _attributes.Add(attribute);
            return this;
        }
    }
}