/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多方关联的元组标准化函数及其反函数.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:41:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     适用于多方关联的元组标准化函数及其反函数。
    ///     实施说明
    ///     假定关联引用的值已将被引对象排列成标准元组，物理形态为Tuple。因此标准化函数只需要顺次取出各元素并组成数组即可，其反函数则是生成一个Tuple，然后
    ///     从数组顺次取出元素填充各项。
    /// </summary>
    public class MultiAssociationTupleStandardizer : ITupleStandardizer
    {
        /// <summary>
        ///     关联引用的属性类型
        /// </summary>
        private readonly PropertyInfo _associationReferenceProperty;

        /// <summary>
        ///     构造适用于多方关联的元组标准化函数及其反函数
        /// </summary>
        /// <param name="associationReferenceProperty">关联引用的属性</param>
        public MultiAssociationTupleStandardizer(PropertyInfo associationReferenceProperty)
        {
            _associationReferenceProperty = associationReferenceProperty;
        }


        /// <summary>
        ///     元组标准化函数的反函数，将标准元组转换成被引对象元组。
        /// </summary>
        /// <returns>被引对象组成的元组（不限定元组的数据类型，只要逻辑上为元组即可）。</returns>
        /// <param name="tupleItems">标准化元组的项序列。</param>
        public object Revert(object[] tupleItems)
        {
            //多方关联 传入的对象即为关联型对象集合
            var result = new List<object>();

            //要把关联型对象中每个对象都转成元组
            foreach (var item in tupleItems)
            {
                Utils.GetIsMultiple(_associationReferenceProperty, out var type);
                //元组泛型参数
                var tupleTypeList = type.GetGenericArguments();

                //每个值都是一组对象
                if (item is object[] endObjects)
                {
                    var realObjects = new List<object>();
                    //按照元组泛型参数顺序加入
                    foreach (var tupleType in tupleTypeList)
                    {
                        //可能没有值 用null代替
                        var endObj = endObjects.FirstOrDefault(p => p != null && tupleType == p.GetType());
                        realObjects.Add(endObj);
                    }

                    //创建对象
                    var tuple = Activator.CreateInstance(type, BindingFlags.CreateInstance, null, realObjects.ToArray(),
                        null);
                    //加入元组
                    result.Add(tuple);
                }
            }

            //返回数组
            return result.ToArray();
        }

        /// <summary>
        ///     元组标准化函数，将被引对象元组转换成标准元组。
        /// </summary>
        /// <returns>表示标准化元组的对象数组。</returns>
        /// <param
        ///     name="referredTuple">
        ///     被引对象组成的元组（不限定元组的数据类型，只要逻辑上为元组即可）。被引对象是指关联引用指向的对象，如果关联引用是多重性
        ///     的，它是指其中的一个。
        /// </param>
        public object[] Standardize(object referredTuple)
        {
            //多方关联 传入的对象即为端对象
            var result = new List<object>();
            //如果是集合 就取出每一个
            if (referredTuple is IEnumerable iEnumerable)
            {
                var enumerator = iEnumerable.GetEnumerator();
                while (enumerator.MoveNext()) result.Add(enumerator.Current);

                if (enumerator is IDisposable disposable) disposable.Dispose();
            }
            else
            {
                result.Add(referredTuple);
            }

            //处理每个元组 变换成每个值的数组
            var tupleResult = new List<object>();
            foreach (var r in result)
                tupleResult.Add(GetTupleValues(r));

            return tupleResult.ToArray();
        }

        /// <summary>
        ///     获取元组的值集合
        /// </summary>
        /// <param name="tuple">元组</param>
        /// <returns></returns>
        private object[] GetTupleValues(object tuple)
        {
            //读取元组的值
            var fieldsWithTuples = tuple
                .GetType()
                .GetProperties()
                .Where(prop => prop.CanRead)
                .Where(prop => !prop.GetIndexParameters().Any())
                .Where(prop => Regex.IsMatch(prop.Name, "^Item[0-9]+$"))
                .Select(field => field.GetValue(tuple))
                .Where(item => item != null).ToArray();

            return fieldsWithTuples;
        }
    }
}