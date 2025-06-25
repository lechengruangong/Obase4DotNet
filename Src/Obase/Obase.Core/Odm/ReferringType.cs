/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：主引类型,可以引用其它对象的类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 17:32:46
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.Odm.Builder;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示可以引用其它对象的类型，简称为主引类型。
    /// </summary>
    public abstract class ReferringType : StructuralType
    {
        /// <summary>
        ///     根据指定的CLR类型创建引用实例。
        /// </summary>
        /// <param name="clrType"></param>
        /// <param name="derivingFrom"></param>
        protected ReferringType(Type clrType, StructuralType derivingFrom = null) : base(clrType, derivingFrom)
        {
        }

        /// <summary>
        ///     创建引用实例，该实例还没有关联的对象系统类型，有待后续指定。
        /// </summary>
        protected ReferringType()
        {
        }

        /// <summary>
        ///     获取类型包含的所有引用元素。
        /// </summary>
        public ReferenceElement[] ReferenceElements => Elements
            .OfType<ReferenceElement>().ToArray();

        /// <summary>
        ///     添加引用元素。
        /// </summary>
        /// <param name="element">引用元素。</param>
        public void AddReferenceElement(ReferenceElement element)
        {
            AddElement(element);
        }

        /// <summary>
        ///     根据名称查询引用类型。
        /// </summary>
        /// <param name="name">元素名称</param>
        public ReferenceElement GetReferenceElement(string name)
        {
            return GetElement(name) as ReferenceElement;
        }

        /// <summary>
        ///     实例化主引类型，并初始化实例属性。
        /// </summary>
        /// <param name="attrValueGetter">一个委托，用于为指定属性树节点所代表的简单属性取值。</param>
        /// <param name="refArgGetter">一个委托，用于为绑定到引用元素的构造参数取值。</param>
        public object Instantiate(Func<SimpleAttributeNode, object> attrValueGetter,
            Func<Parameter, object> refArgGetter)
        {
            //构造取参器
            var argValueGetter = new AttributeValueGetterBasedArgumentGetter(attrValueGetter);

            //包装为局部方法
            //等价于Func<Parameter, object> argGetter = p => p.ElementType == eElementType.Attribute ? argValueGetter.Get(p) : refArgGetter(p);
            object ArgGetter(Parameter p)
            {
                return p.ElementType == EElementType.Attribute ? argValueGetter.Get(p) : refArgGetter(p);
            }

            //返回基类方法值
            return Instantiate(ArgGetter);
        }

        /// <summary>
        ///     实例化主引类型，并初始化实例属性和引用元素。
        /// </summary>
        /// <param name="attrValueGetter">一个委托，用于为指定属性树节点所代表的简单属性取值。</param>
        /// <param name="refValueGetter">一个委托，用于为引用元素取值。</param>
        /// <param name="hasInclude">一个委托, 根据引用元素获取是否被Include</param>
        public object Instantiate(Func<SimpleAttributeNode, object> attrValueGetter,
            Func<ReferenceElement, object> refValueGetter, Func<ReferenceElement, bool> hasInclude)
        {
            //包装成局部方法
            object RefArgGetter(Parameter p)
            {
                var ele = p.GetElement();
                if (ele is Attribute attribute)
                    return attrValueGetter(new SimpleAttributeNode(attribute));
                return refValueGetter((ReferenceElement)ele);
            }

            //构造结构类型
            var resultObj = Instantiate(RefArgGetter, attrValueGetter);
            //处理引用元素
            if (ReferenceElements != null && ReferenceElements.Length > 0)
                foreach (var referenceElement in ReferenceElements)
                {
                    //从构造器获取参数
                    var para = _constructor.GetParameterByElement(referenceElement.Name);
                    //如果已经通过构造函数赋值，则不需要再赋值。
                    if (para != null) continue;
                    var values = refValueGetter.Invoke(referenceElement);
                    if (values == null) continue;
                    if (referenceElement.IsMultiple)
                    {
                        var vals = (IEnumerable<object>)values;
                        var enumerable = vals as object[] ?? vals.ToArray();
                        //有值 直接设置
                        if (enumerable.Any())
                        {
                            referenceElement.SetValue(resultObj, enumerable);
                        }
                        else
                        {
                            //没值 检查一次是否是被包含 被包含则初始化容器
                            if (hasInclude.Invoke(referenceElement))
                                referenceElement.SetValue(resultObj, new List<object>());
                        }
                    }
                    else
                    {
                        foreach (var item in (IEnumerable<object>)values)
                            referenceElement.SetValue(resultObj, item);
                    }
                }

            //返回s
            return resultObj;
        }


        /// <summary>
        ///     实例化主引类型，并初始化实例属性。
        /// </summary>
        /// <param name="attrValueGetter">属性取值委托，该属性须为类型的直接属性。</param>
        /// <param name="refArgGetter">一个委托，用于为绑定到引用元素的构造参数取值。</param>
        public object Instantiate(Func<Attribute, object> attrValueGetter, Func<Parameter, object> refArgGetter)
        {
            //等同于Func<Parameter, object> argGetter = parameter =>{ XXX }
            object ArgGetter(Parameter parameter)
            {
                if (parameter.ElementType == EElementType.Attribute)
                {
                    //类型为属性 强转
                    var attr = (Attribute)parameter.GetElement();
                    return attrValueGetter(attr);
                }

                return refArgGetter;
            }


            return Instantiate(ArgGetter, attrValueGetter);
        }

        /// <summary>
        ///     实例化主引类型，并初始化实例属性和引用元素。
        ///     实施说明
        ///     调用另一重载Instantiate(Func{Attribute, Object}, Func{Parameter,
        ///     Object})，参见顺序图“实例化主引类型（重载三）”。
        /// </summary>
        /// <param name="elementValueGetter">元素取值委托，该元素须为类型的直接元素。</param>
        public object Instantiate(Func<TypeElement, object> elementValueGetter)
        {
            Func<Attribute, object> attrValueGetter = elementValueGetter;

            //等同于Func<Parameter, object> refArgGetter = parameter => elementValueGetter(parameter.GetElement());
            object RefArgGetter(Parameter parameter)
            {
                return elementValueGetter(parameter.GetElement());
            }

            return Instantiate(attrValueGetter, RefArgGetter);
        }

        /// <summary>
        ///     获取类型的筛选键。
        ///     对于类型的某一个属性或属性序列，如果其值或值序列可以作为该类型实例的标识，该属性或属性序列即可作为该类型的筛选键。
        ///     对于实体型，可以用主键作为筛选键。对于关联型，可以用其在各关联端上的外键属性组合成的属性序列作为筛选键。
        /// </summary>
        /// <returns>构成筛选键的属性序列。</returns>
        public abstract Attribute[] GetFilterKey();

        /// <summary>
        ///     获取指定实例的标识。
        /// </summary>
        /// <returns>
        ///     作为标识的IdentityArray实例。
        ///     实施说明
        ///     首先获取当前类型的筛选键，然后顺序获取各筛选键属性的值，组合成标识。
        /// </returns>
        /// <param name="targetObj"></param>
        public IdentityArray GetIdentity(object targetObj)
        {
            //获取属性
            var attrs = GetFilterKey();
            //获取每一个属性值
            var listIdentity = attrs.Select(attribute => attribute.GetValue(targetObj)).ToList();
            //组合成标识
            var result = new IdentityArray();
            result.AddRange(listIdentity);

            return result;
        }

        /// <summary>
        ///     生成当前类型的筛选查询。
        ///     筛选查询用于从类型实例的集合中筛选出指定实例。
        /// </summary>
        /// <returns>
        ///     生成的筛选查询。
        ///     实施说明
        ///     参见顺序图“生成筛选查询”。
        /// </returns>
        /// <param name="objects">要从筛选源中筛选出来的实例。</param>
        /// <param name="nextOp">查询链中的下一节点。</param>
        public WhereOp GenerateFilterQuery(object[] objects, QueryOp nextOp = null)
        {
            //形参绑定
            var parameter = Expression.Parameter(RebuildingType);
            //过滤键属性
            var keyAttrs = GetFilterKey();
            //所有对象一起
            Expression bodyExpression = null;
            //每个对象
            foreach (var obj in objects)
            {
                //单个对象 所有属性一起
                Expression singeleExpression = null;
                //每个属性
                foreach (var keyAttr in keyAttrs)
                {
                    //左边 成员表达式
                    var memberExp = Expression.PropertyOrField(parameter, keyAttr.Name);
                    //右边 静态变量表达式
                    var attrValue = keyAttr.GetValue(obj);
                    var valueExp = Expression.Constant(attrValue);
                    //组合一下
                    var segement = Expression.Equal(memberExp, valueExp);
                    //组合至单个对象 所有属性一起
                    singeleExpression = singeleExpression == null
                        ? segement
                        : Expression.AndAlso(singeleExpression, segement);
                }

                //组合至所有对象一起
                bodyExpression = bodyExpression == null
                    ? singeleExpression
                    : Expression.OrElse(bodyExpression,
                        singeleExpression ?? throw new InvalidOperationException($"对象类型{ClrType}生成筛选查询错误:没有过滤键属性"));
            }

            if (bodyExpression == null)
                bodyExpression = Expression.Constant(true);

            var predicate = Expression.Lambda(bodyExpression, parameter);
            return (WhereOp)QueryOp.Where(predicate, Model, nextOp);
        }

        /// <summary>
        ///     为当前类型或建立在当前类型上的视图的指定实例集编写字典，该字典以实例的标识为键，以实例本身为值。
        /// </summary>
        /// <returns>
        ///     存储指定实例集的字典。
        ///     实施说明
        ///     参见活动图“为实例集编写字典”。
        /// </returns>
        /// <param name="objects">作为筛选源的实例集。</param>
        /// <param name="typeView">如果筛选源是视图实例，指定视图类型。</param>
        public IDictionary<IdentityArray, object> MakeDictionary(object[] objects, TypeView typeView = null)
        {
            //返回值
            var result = new Dictionary<IdentityArray, object>();
            //未指定视图类型
            if (typeView == null)
            {
                foreach (var obj in objects)
                    //直接获取标识
                    result.Add(GetIdentity(obj), obj);
            }
            else
            {
                //参与构造过滤的属性
                var filterAttrs = new List<Attribute>();
                //过滤键
                var filterKeys = GetFilterKey();
                //分解视图
                var stack = typeView.GetNestingStack();

                while (stack.Count > 0)
                {
                    var currentView = stack.Pop();
                    //每个再根据过滤键获取一次直观属性
                    filterAttrs.AddRange(filterKeys.Select(filterKey => currentView.GetIntuitiveAttribute(filterKey)));
                }

                //每个对象 按照过滤属性挨个取一边
                foreach (var obj in objects)
                {
                    var identity = new IdentityArray();
                    foreach (var filterAttr in filterAttrs)
                    {
                        var filterValue = filterAttr.GetValue(obj);
                        identity.Add(filterValue);
                    }

                    //添加标识
                    result.Add(identity, obj);
                }
            }

            return result;
        }

        /// <summary>
        ///     根据快照重建对象。
        /// </summary>
        /// <param name="snapshot"></param>
        /// <param name="attach"></param>
        /// <param name="asRoot"></param>
        /// <returns></returns>
        public override object Rebuild(ObjectSnapshot snapshot, AttachObject attach, bool asRoot)
        {
            //实施说明

            //首先调用ObjectSnapshot.GenerateTree方法生成关联树，附带生成用于重建对象系统的数据集。
            //然后实例化ObjectSystemBuilder，将其作为访问者访问上述关联树，生成对象。

            var tree = snapshot.GenerateTree(out var dataSet);
            var builder = new ObjectSystemBuilder(dataSet, attach);
            tree.Accept(builder);

            return builder.Result;
        }
    }
}