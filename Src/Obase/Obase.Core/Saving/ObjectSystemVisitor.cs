/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象系统访问器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:28:31
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Odm;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     对象系统访问器。
    /// </summary>
    public static class ObjectSystemVisitor
    {
        #region 取出对象关联引用的关联对象集合（如果为隐式关联就构造隐式关联对象）

        /// <summary>
        ///     关联导航。
        ///     取出源对象中指定关联引用的值，如果为隐式关联则实施显式化操作（自动创建隐式关联对象）。
        /// </summary>
        /// <param name="obj">源对象</param>
        /// <param name="reference">要导航的关联引用</param>
        public static IEnumerable<object> AssociationNavigate(object obj, AssociationReference reference)
        {
            //获取关联引用对象（取出的是实体对象集合:List<文章>）
            var target = GetValue(obj, reference);
            if (!reference.AssociationType.Visible) //隐式关联
            {
                var type = reference.AssociationType;
                List<object> assocObjs = null;
                //遍历关联对象引用
                foreach (var item in target)
                {
                    if (assocObjs == null)
                        assocObjs = new List<object>();

                    //左端对象 和 右端对象
                    var dic = new Dictionary<string, object>
                        { { reference.LeftEnd, obj }, { reference.RightEnd, item } };
                    //创建隐式关联对象（构造的是隐式关联对象（ImplicitAssociation<分类,文章>））
                    var assObj = BuildObject(type, dic);
                    //如果有符合关联端映射的键属性 则为其赋值
                    //找出左端
                    var leftEnd = type.AssociationEnds.FirstOrDefault(p => p.Name == reference.LeftEnd);
                    if (leftEnd != null) SetEndMappingFiled(type, leftEnd, obj, assObj);
                    //找出右端
                    var rightEnd = type.AssociationEnds.FirstOrDefault(p => p.Name == reference.RightEnd);
                    if (rightEnd != null) SetEndMappingFiled(type, rightEnd, item, assObj);
                    //添加到关联对象集合
                    assocObjs.Add(assObj);
                }

                //隐式关联对象集合
                target = assocObjs;
            }

            //返回 显示关联直接就是引用对象 隐式则需要创建
            return target;
        }

        /// <summary>
        ///     为隐式关联型的关联映射属性设值
        /// </summary>
        /// <param name="type">关联型</param>
        /// <param name="end">关联端</param>
        /// <param name="endObj">端对象</param>
        /// <param name="assObj">关联对象</param>
        private static void SetEndMappingFiled(AssociationType type, AssociationEnd end, object endObj, object assObj)
        {
            //端的实体型
            var endType = end.EntityType;
            //端的键属性
            var endKeyAttrs = endType.Attributes.Where(p => endType.KeyAttributes.Contains(p.Name)).ToList();
            //在端内寻找符合条件的映射和属性
            foreach (var endKeyAttr in endKeyAttrs)
            foreach (var mapping in end.Mappings)
                //如果映射键属性和端对象键属性相同
                if (endKeyAttr.Name == mapping.KeyAttribute)
                {
                    //从端对象内取出键属性
                    var keyValue = endKeyAttr.ValueGetter.GetValue(endObj);
                    //为关联型内映射属性(映射属性在表内字段肯定和映射的目标属性相同)赋值
                    type.Attributes?.FirstOrDefault(p => p.TargetField == mapping.TargetField)?.ValueSetter
                        ?.SetValue(assObj, keyValue);
                }
        }

        #endregion

        #region 通过取值器获取属性值

        /// <summary>
        ///     从对象中获取元素（属性、关联引用、关联端）的值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="elementName">目标元素的名称</param>
        /// <param name="model">对象数据模型</param>
        public static object GetValue(object obj, string elementName, ObjectDataModel model)
        {
            //获取模型
            var type = model.GetStructuralType(obj.GetType());
            if (type == null) throw new ArgumentException($"无法获取到{obj.GetType().Name}的模型.", nameof(obj));
            //获取对象元素的值
            return GetValue(obj, type, elementName);
        }

        /// <summary>
        ///     从对象中获取元素（属性、关联引用、关联端）的值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="type">对象的类型（实体型、关联型、复杂类型）</param>
        /// <param name="elementName">目标元素的名称</param>
        public static object GetValue(object obj, StructuralType type, string elementName)
        {
            //获取元素
            var typeElement = type.GetElement(elementName);
            if (typeElement == null) throw new ArgumentException($"无法获取到{elementName}的类型元素.", nameof(elementName));
            //获取对象元素的值
            return GetValue(obj, typeElement);
        }

        /// <summary>
        ///     从对象中获取元素（属性、关联引用、关联端）的值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="element">目标元素</param>
        public static object GetValue(object obj, TypeElement element)
        {
            object result;
            if (element is ReferenceElement && obj is IIntervene inter)
            {
                //禁用延迟加载（防止延迟加载期间内部访问属性又开始加载，造成死循环）
                inter.ForbidLazyLoading();
                //获取值
                result = element.ValueGetter.GetValue(obj);
                //启用延迟加载
                inter.EnableLazyLoading();
            }
            else
            {
                //获取值
                result = element.ValueGetter.GetValue(obj);
            }

            return result;
        }

        /// <summary>
        ///     从对象获取关联引用的所有取值，如果关联重数大于1则多次调用取值器取出所有值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="reference">要读取其值的关联引用</param>
        public static IEnumerable<object> GetValue(object obj, AssociationReference reference)
        {
            var inter = obj as IIntervene;
            inter?.ForbidLazyLoading();

            var value = reference.ValueGetter.GetValue(obj);

            //是否是多重的
            if (reference.IsMultiple)
            {
                var valueienumerable = (IEnumerable<object>)value ?? new List<object>();
                inter?.EnableLazyLoading();
                return valueienumerable;
            }

            inter?.EnableLazyLoading();

            return value == null ? new List<object>() : new List<object> { value };
        }

        /// <summary>
        ///     获取关联对象指定关联端的标识属性的值。
        /// </summary>
        /// <param name="associationObj">指定的关联对象</param>
        /// <param name="associationType">指定关联对象的类型</param>
        /// <param name="associationEnd">要获取其标识属性值的关联端</param>
        /// <param name="keyAttribute">要获取其值的标识属性</param>
        public static object GetValue(object associationObj, AssociationType associationType,
            AssociationEnd associationEnd,
            string keyAttribute)
        {
            object value = null;
            //获取关联端对象
            var endObj = GetValue(associationObj, associationEnd);
            if (endObj == null)
            {
                //获取关联端标识属性字段名
                var fieId = associationEnd.GetTargetField(keyAttribute);
                //根据字段在关联型查询属性
                var attr = associationType.FindAttributeByTargetField(fieId);
                if (attr != null)
                    //获取属性值
                    value = GetValue(associationObj, attr);
            }
            else
            {
                //获取关联端的属性值
                value = GetValue(endObj, associationEnd.EntityType, keyAttribute);
            }

            //返回标识属性值
            return value;
        }

        /// <summary>
        ///     从对象中获取指定子属性的值。子属性是某一复杂属性的类型的属性，该复杂属性称为该子属性的父属性。
        /// </summary>
        /// <param name="obj">目标对象。</param>
        /// <param name="attribute">子属性。</param>
        /// <param name="parent">指向父属性的属性路径。</param>
        public static object GetValue(object obj, Attribute attribute, AttributePath parent)
        {
            var targetObj = obj;
            var enumerator = parent.GetEnumerator();
            while (enumerator.MoveNext()) targetObj = parent.Current.ValueGetter.GetValue(targetObj);
            enumerator.Dispose();
            var attributeValueGetter = attribute.ValueGetter;
            var attributeValue = attributeValueGetter.GetValue(targetObj);
            return attributeValue;
        }

        #endregion

        #region 通过设值器设置值

        /// <summary>
        ///     为对象的关联引用设置值，如果关联重数大于1则多次调用设值器。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="reference">目标关联引用</param>
        /// <param name="values">要设置的值的集合</param>
        public static void SetValue(object obj, AssociationReference reference, object[] values)
        {
            if (values != null && values.Length > 0)
                for (var i = 0; i < values.Length; i++)
                    //追加设值
                    SetValue(obj, reference, values[i]);
        }

        /// <summary>
        ///     为对象的指定元素（属性、关联引用、关联端）设置值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="elementName">目标元素的名称</param>
        /// <param name="value">值对象</param>
        /// <param name="model">对象数据模型</param>
        public static void SetValue(object obj, string elementName, object value, ObjectDataModel model)
        {
            //获取对象模型
            var type = model.GetStructuralType(obj.GetType());
            if (type == null) throw new ArgumentException($"无法获取到{obj.GetType().Name}的模型.", nameof(obj));
            //模型元素设值
            SetValue(obj, type, elementName, value);
        }

        /// <summary>
        ///     为对象的指定元素（属性、关联引用、关联端）设置值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="type">对象的类型（实体型、关联型、复杂类型）</param>
        /// <param name="elementName">目标元素的名称</param>
        /// <param name="value">值对象</param>
        public static void SetValue(object obj, StructuralType type, string elementName, object value)
        {
            //获取元素
            var element = type.GetElement(elementName);
            if (element == null) throw new ArgumentException($"无法获取到{elementName}的类型元素.", nameof(elementName));
            //元素设值
            SetValue(obj, element, value);
        }

        /// <summary>
        ///     为对象的指定元素（属性、关联引用、关联端）设置值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="element">目标元素</param>
        /// <param name="value">值对象</param>
        public static void SetValue(object obj, TypeElement element, object value)
        {
            var inter = obj as IIntervene;

            if (element is ReferenceElement)
            {
                //禁用延迟加载（设值时禁用延迟加载避免造成循环）
                inter?.ForbidLazyLoading();
                //设置值
                element.ValueSetter?.SetValue(obj, value);
                //启用延迟加载
                inter?.EnableLazyLoading();
            }
            else
            {
                element.ValueSetter?.SetValue(obj, value);
            }
        }

        #endregion

        #region 通过构造器构造对象

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <param name="type">目标对象的类型（实体型、关联型、复杂类型）</param>
        /// <param name="referredObjects">一个字典，存储目标对象的关联引用或关联端，键为元素名称，值为关联引用或关联端对象</param>
        public static object BuildObject(StructuralType type, Dictionary<string, object> referredObjects)
        {
            //使用构造器构造对象
            var target = type.Constructor.Construct();
            //遍历引用属性（关联端、关联引用）
            foreach (var re in type.Elements)
                if (referredObjects?.ContainsKey(re.Name) ?? false)
                    SetValue(target, re, referredObjects[re.Name]);
            return target;
        }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <param name="type">目标对象的类型（实体型、关联型、复杂类型）</param>
        /// <param name="attrGetter">一个委托，用于获取目标对象的属性值，委托参数为要获取其值的属性，委托返回值为属性值</param>
        /// <param name="nullCount">此对象查出的数据中DBNull的个数</param>
        public static object BuildObject(StructuralType type, Func<Attribute, object> attrGetter, ref int nullCount)
        {
            //使用构造器构造对象
            var target = type.Constructor.Construct();

            if (target.GetType().IsValueType && !target.GetType().IsPrimitive && !(target is Enum))
                target = new StructWrapper(target);

            //遍历属性
            foreach (var attr in type.Attributes)
            {
                //是否为复杂属性 复杂属性进行剥离
                var value = !attr.IsComplex
                    ? attrGetter(attr)
                    : BuildObject(((ComplexAttribute)attr).ComplexType, attrGetter, ref nullCount);
                //向对象设置属性值
                SetValue(target, type, attr.Name, value is DBNull ? null : value);
                //记录DBNull的个数
                if (value is DBNull) nullCount++;
            }

            if (target is StructWrapper structWrapper)
                target = structWrapper.Struct;
            return target;
        }

        /// <summary>
        ///     构造对象
        /// </summary>
        /// <param name="type">结构化类型</param>
        /// <param name="attrGetter">属性值获取委托</param>
        /// <param name="referredObjects">参照对象</param>
        /// <param name="nullCount">空值个数</param>
        /// <returns></returns>
        public static object BuildObject(StructuralType type, Func<Attribute, object> attrGetter,
            Dictionary<string, object> referredObjects, ref int nullCount)
        {
            //使用构造器构造对象
            var target = BuildObject(type, attrGetter, ref nullCount);

            //遍历引用属性（关联端、关联引用）
            foreach (var re in type.Elements)
                //给引用属性设值
                SetValue(target, re, referredObjects[re.Name]);

            if (target is StructWrapper structWrapper)
                target = structWrapper.Struct;
            return target;
        }

        #endregion

        #region 通过标识属性构造标识对象

        /// <summary>
        ///     获取实体对象的标识
        /// </summary>
        /// <param name="entityObj">目标对象</param>
        /// <param name="entityType">目标对象对应的实体型</param>
        public static ObjectKey GetObjectKey(object entityObj, EntityType entityType)
        {
            //对象标识成员由标识属性名与属性值组合
            var objectKeyMemberList = new List<ObjectKeyMember>();
            if (entityType.KeyAttributes != null)
                foreach (var key in entityType.KeyAttributes)
                {
                    //获取属性值
                    var value = GetValue(entityObj, entityType, key);
                    //创建标识成员
                    var member = new ObjectKeyMember($"{entityType.ClrType.FullName}-{key}", value);
                    objectKeyMemberList.Add(member);
                }

            //创建对象标识
            return new ObjectKey(entityType, objectKeyMemberList);
        }

        /// <summary>
        ///     获取关联对象的标识
        /// </summary>
        /// <param name="associationObj">目标对象</param>
        /// <param name="associationType">目标对象对应的关联型</param>
        public static ObjectKey GetObjectKey(object associationObj, AssociationType associationType)
        {
            var objectKeyMemberList = new List<ObjectKeyMember>();
            //遍历关联端
            foreach (var associationEnd in associationType.AssociationEnds)
            {
                //获取端对象
                var endObj = GetValue(associationObj, associationEnd);
                //遍历关联端的映射
                foreach (var mapping in associationEnd.Mappings)
                {
                    object value;
                    if (endObj == null)
                    {
                        //根据字段名查找属性
                        var attr = associationType.FindAttributeByTargetField(mapping.TargetField);
                        if (attr == null)
                            throw new ArgumentException(
                                $"关联型{associationType.Name}的关联端{associationEnd.Name}映射属性{mapping.TargetField}不存在.");
                        value = GetValue(associationObj, attr);
                    }
                    else
                    {
                        //取出关联端的标识属性值
                        value = GetValue(endObj, associationEnd.EntityType, mapping.KeyAttribute);
                    }

                    //创建标识成员
                    var member =
                        new ObjectKeyMember(
                            associationEnd.EntityType.ClrType.FullName + "-" + associationEnd.Name + "." +
                            mapping.KeyAttribute, value);
                    objectKeyMemberList.Add(member);
                }
            }

            //创建对象标识
            return new ObjectKey(associationType, objectKeyMemberList);
        }

        /// <summary>
        ///     获取对象的标识。
        ///     注：如果对象既不是实体对象也不是关联对象则引发异常。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="modelType">目标对象对应的模型类型</param>
        public static ObjectKey GetObjectKey(object obj, StructuralType modelType)
        {
            if (modelType is EntityType entityType)
                return GetObjectKey(obj, entityType);
            if (modelType is AssociationType associationType)
                return GetObjectKey(obj, associationType);
            throw new ArgumentOutOfRangeException(nameof(obj), $"无法获取对象的标识,类型{obj.GetType()}未注册.");
        }

        /// <summary>
        ///     获取关联端标识。
        /// </summary>
        /// <param name="associationObj">要获取其标识的关联端所属的关联对象。</param>
        /// <param name="associationType">关联对象的类型。</param>
        /// <param name="associationEnd">要获取其标识的关联端。</param>
        public static ObjectKey GetObjectKey(object associationObj, AssociationType associationType,
            AssociationEnd associationEnd)
        {
            //获取关联端对象
            var endObj = GetValue(associationObj, associationEnd);
            var members = new List<ObjectKeyMember>();
            //遍历关联端映射
            foreach (var mapp in associationEnd.Mappings)
            {
                object value;
                if (endObj == null)
                {
                    //根据字段名查找属性
                    var attr = associationType.FindAttributeByTargetField(mapp.TargetField);
                    //获取关联对象属性值
                    value = GetValue(associationObj, attr);
                }
                else
                {
                    //获取关联端的标识属性值
                    value = GetValue(endObj, associationEnd.EntityType, mapp.KeyAttribute);
                }

                //创建标识成员
                var member = new ObjectKeyMember(associationEnd.EntityType.ClrType.FullName + "-" + mapp.KeyAttribute,
                    value);
                members.Add(member);
            }

            //创建对象标识
            var key = new ObjectKey(associationEnd.EntityType, members);
            return key;
        }

        #endregion
    }
}