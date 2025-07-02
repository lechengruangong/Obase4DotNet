/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序依据接口,提供排序字段访问器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:28:56
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     在隐式关联的显式化配置中，将关联引用的值包装成关联实例。
    /// </summary>
    public class AssociationReferenceValueWrapper : IValueGetter, IValueSetter
    {
        /// <summary>
        ///     被包装的关联引用所在关联端在关联型上的索引号（从1开始计数）。
        /// </summary>
        private readonly byte _associationEndIndex;

        /// <summary>
        ///     关联引用的关联类型。
        /// </summary>
        private readonly AssociationType _associationType;

        /// <summary>
        ///     基础取值器，用于取出当前关联引用的原始值。
        /// </summary>
        private readonly IValueGetter _foundationGetter;

        /// <summary>
        ///     基础设值器，用于为当前关联引用设置原始值。
        /// </summary>
        private readonly IValueSetter _foundationSetter;

        /// <summary>
        ///     是否是多方隐式关联
        /// </summary>
        private readonly bool _isMultiAssociation;

        /// <summary>
        ///     指示被包装的关联引用是否是多重的。
        /// </summary>
        private readonly bool _isMultiple;

        /// <summary>
        ///     元组标准化函数及其反函数。
        /// </summary>
        private readonly ITupleStandardizer _tupleStandardizer;

        /// <summary>
        ///     初始化AssociationReferenceValueWrapper类的新实例。
        /// </summary>
        /// <param name="foundationGetter">关联引用的基础取值器。</param>
        /// <param name="foundationSetter">关联引用的基础设值器。</param>
        /// <param name="tupleStandardizer">元组标准化函数及其反函数。</param>
        /// <param name="assoType">关联引用的关联类型。</param>
        /// <param name="isMultiple">指示关联引用是否是多重性的。</param>
        /// <param name="endIndex">关联引用所在关联端在关联型上的索引号（从1开始计数）。</param>
        /// <param name="isMultiAssociation">是否是多方隐式关联</param>
        public AssociationReferenceValueWrapper(IValueGetter foundationGetter, IValueSetter foundationSetter,
            ITupleStandardizer tupleStandardizer, AssociationType assoType, bool isMultiple, byte endIndex,
            bool isMultiAssociation)
        {
            _foundationGetter = foundationGetter;
            _foundationSetter = foundationSetter;
            _tupleStandardizer = tupleStandardizer;
            _associationEndIndex = endIndex;
            _isMultiAssociation = isMultiAssociation;
            _isMultiple = isMultiple;
            _associationType = assoType;
        }

        /// <summary>
        ///     从指定对象取值。
        ///     实施说明
        ///     参见顺序图“包装关联引用的值”。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public object GetValue(object obj)
        {
            //原始值
            var originalValue = _foundationGetter.GetValue(obj);

            //多方关联才加入处理
            if (!_isMultiAssociation)
                //两方关联 直接返回第1个
                return _tupleStandardizer.Standardize(originalValue)[0];

            //如果是空 直接返回
            if (originalValue == null)
                return null;

            //最终结果
            var resultList = new List<object>();

            //隐式关联型的构造函数
            var assConstructor = _associationType.Constructor;

            //原始值 即端对象
            var item = originalValue;
            //对原始值进行处理 每个圆度都是 将每个端都展开成关联端对象数组[End1,End2 ... End] 无一定的顺序
            var standardTruples = _tupleStandardizer.Standardize(item);

            //所有的关联端
            var ends = _associationType.AssociationEnds;

            //每个对象构造一个隐式关联型对象
            foreach (var standard in standardTruples)
            {
                //构造关联型
                var result = assConstructor.Construct();
                //每个值都是一组对象
                if (standard is object[] endObjects)
                {
                    //设值 每个端
                    foreach (var end in ends)
                        //自己这一端
                        if (end.Name == $"End{_associationEndIndex}")
                            end.SetValue(result, obj);
                        else
                            //其他端
                            //每个端对象
                            foreach (var endObject in endObjects)
                                if (endObject.GetType() == end.EntityType.ClrType)
                                    end.SetValue(result, endObject);
                    resultList.Add(result);
                }
            }

            //根据多重性 处理为单值和集合
            if (!_isMultiple)
                return resultList.First();

            return resultList;
        }

        /// <summary>
        ///     获取设值模式。
        /// </summary>
        public EValueSettingMode Mode => _foundationSetter.Mode;


        /// <summary>
        ///     为对象设值。
        ///     实施说明
        ///     参见顺序图“包装关联引用的值”。注意，须考虑“赋值”和“追加”两种设值模式。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        public void SetValue(object obj, object value)
        {
            //多方关联才加入处理
            if (!_isMultiAssociation)
            {
                //两方关联 直接设置值
                SetValueCore(obj, _tupleStandardizer.Revert(new[] { value }));
                return;
            }

            //关联型对象集合
            var assTypeObj = new List<object>();
            if (value is IEnumerable iEnumerable)
            {
                var enumerator = iEnumerable.GetEnumerator();
                while (enumerator.MoveNext()) assTypeObj.Add(enumerator.Current);

                if (enumerator is IDisposable disposable) disposable.Dispose();
            }
            else
            {
                assTypeObj.Add(value);
            }

            //所有的关联端
            var ends = _associationType.AssociationEnds;
            var endObjects = new List<object>();
            //处理每个关联对象
            foreach (var assObj in assTypeObj)
            {
                //每个处理成object[]
                var endObjs = new List<object>();
                foreach (var end in ends)
                {
                    if (end.Name == $"End{_associationEndIndex}")
                        continue;
                    var endObj = end.GetValue(assObj);
                    if (endObj != null)
                        endObjs.Add(endObj);
                }

                //如果不够数 加一些空值
                if (endObjs.Count < ends.Count - 1)
                {
                    var end = ends.Count - 1 - endObjs.Count;
                    var start = 0;
                    while (start < end)
                    {
                        endObjs.Add(null);
                        start++;
                    }
                }

                endObjects.Add(endObjs.ToArray());
            }

            //获取真正要设值的结果
            var revertObj = _tupleStandardizer.Revert(endObjects.ToArray());

            if (_isMultiple)
            {
                var vals = (IEnumerable<object>)revertObj;
                var enumerable = vals as object[] ?? vals.ToArray();
                //有值 直接设置
                if (enumerable.Any()) SetValueCore(obj, enumerable);
            }
            else
            {
                //应只有一个值
                foreach (var item in (IEnumerable<object>)revertObj)
                    SetValueCore(obj, item);
            }
        }

        /// <summary>
        ///     具体的设置值方法
        /// </summary>
        /// <param name="targetObj">目标</param>
        /// <param name="value">值</param>
        private void SetValueCore(object targetObj, IEnumerable value)
        {
            var settinMode = _foundationSetter.Mode;
            switch (settinMode)
            {
                case EValueSettingMode.Assignment:
                    _foundationSetter.SetValue(targetObj, value);
                    break;
                case EValueSettingMode.Appending:
                    if (value == null) return;
                    foreach (var valueItem in value) _foundationSetter.SetValue(targetObj, valueItem);
                    break;
            }
        }

        /// <summary>
        ///     具体的设置值方法
        /// </summary>
        /// <param name="targetObj">目标</param>
        /// <param name="value">值</param>
        private void SetValueCore(object targetObj, object value)
        {
            //前置过滤，如果value实现了IEnumerable或IEnumerable<>，调用另一重载。
            var valueType = value.GetType();
            if (valueType != typeof(string) && valueType.GetInterface("IEnumerable") != null)
            {
                var iEnumerableValue = (IEnumerable)value;
                SetValueCore(targetObj, iEnumerableValue);
            }
            else
            {
                _foundationSetter.SetValue(targetObj, value);
            }
        }
    }
}