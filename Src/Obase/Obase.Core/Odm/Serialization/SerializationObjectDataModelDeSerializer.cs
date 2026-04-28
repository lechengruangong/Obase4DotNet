/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化对象数据模型对象反序列化器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 16:02:19
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obase.Core.Common;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化对象数据模型对象反序列化器
    /// </summary>
    public class SerializationObjectDataModelDeSerializer
    {
        /// <summary>
        ///     本次反序列化过程中已经反序列化的对象字典 key为对象在本次反序列化中分配的ID value为反序列化后的对象
        /// </summary>
        private readonly Dictionary<string, object> _deSerializedObject = new Dictionary<string, object>();

        /// <summary>
        ///     序列化对象数据模型对象
        /// </summary>
        private readonly SerializationObjectDataModel _model;

        /// <summary>
        ///     初始化序列化对象数据模型对象反序列化器
        /// </summary>
        /// <param name="model">序列化对象数据模型对象</param>
        public SerializationObjectDataModelDeSerializer(SerializationObjectDataModel model)
        {
            _model = model;
        }

        /// <summary>
        ///     序列化对象数据模型的反序列化方法
        ///     最终返回反序列化后的对象集合 其顺序与传入的Dto集合中根对象的顺序一致
        /// </summary>
        /// <param name="wrapper">Dto的包装对象</param>
        /// <returns>反序列化后的对象集合</returns>
        public List<object> DeSerialize(SerializationDataTransferObjectWrapper wrapper)
        {
            //对象集合
            foreach (var dto in wrapper.Dto)
            {
                //当前对象的类型
                Type currentType = null;
                if (!string.IsNullOrEmpty(dto.AssemblyName) && !string.IsNullOrEmpty(dto.TypeName))
                    currentType = Assembly.Load(dto.AssemblyName)?.GetType(dto.TypeName);

                //取类型
                if (currentType != null)
                {
                    //获取对象的模型类型 如果模型中没有定义这个类型 则不处理
                    var type = _model.GetTypeOrNull(currentType);
                    if (type != null)
                    {
                        var parameterValues = new List<object>();
                        //处理构造函数 按照Index排序
                        var parameters = type.ConstructorParameters.OrderBy(p => p,
                                Comparer<SerializationConstructorParameter>.Create(
                                    CreateConstructorParameterComparison))
                            .ToList();
                        foreach (var parameter in parameters)
                        {
                            object value = null;
                            //如果是需要存储 则从dto的构造函数参数字典中取出对应索引的值
                            if (parameter.NeedStorage)
                            {
                                if (dto.ConstructorParameters.TryGetValue(parameter.Index,
                                        out var constructorParameter))
                                    //进行一次通用的转换
                                    value = Utils.ConvertDbValue(constructorParameter,
                                        parameter.ValueType);
                            }
                            else
                            {
                                //否则使用取值器获取 注意此时固定传参为null
                                value = parameter.GetValue(null);
                            }

                            //统一进行一次类型检查 如果不为null且类型不匹配 则抛出异常
                            if (value != null &&
                                !Utils.IsNullableWrapperEqualOrEqual(value.GetType(), parameter.ValueType))
                                throw new ArgumentException(
                                    $"反序列化{type.ClrType}的构造函数参数{parameter.Index}时出错,配置的值类型为{parameter.ValueType},实际取到的为{value.GetType()}.");

                            parameterValues.Add(value);
                        }


                        var obj = type.Constructor.Construct(parameterValues.ToArray());

                        //处理属性
                        foreach (var attribute in type.Attributes)
                        {
                            var value = Utils.ConvertDbValue(dto.Attributes[attribute.Name], attribute.ValueType);
                            if (value != null)
                                attribute.SetValue(obj, value);
                        }

                        //加入已处理的集合
                        _deSerializedObject[dto.Id] = obj;
                    }
                }
            }

            //处理引用
            //取出其中的根对象 其余对象都是从根对象出发被引用的
            var rootIds = wrapper.Dto.Where(d => d.IsRoot).Select(p => p.Id).ToList();
            //递归的设置引用
            SetReferences(rootIds, wrapper.Dto, new HashSet<string>());

            //返回根对象集合
            return _deSerializedObject.Where(p => rootIds.Contains(p.Key)).Select(p => p.Value).ToList();
        }

        /// <summary>
        ///     设置引用
        /// </summary>
        /// <param name="ids">序列化ID结合</param>
        /// <param name="dtos">dto</param>
        /// <param name="hasSetIds">已经处理过的ID</param>
        private void SetReferences(List<string> ids, List<SerializationDataTransferObject> dtos,
            HashSet<string> hasSetIds)
        {
            foreach (var id in ids)
            {
                //取出对象
                if (!_deSerializedObject.TryGetValue(id, out var obj))
                    continue;
                //对象的类型
                var currentType = obj.GetType();
                //获取对象的模型类型 如果模型中没有定义这个类型 则不处理
                var type = _model.GetTypeOrNull(currentType);
                if (type != null)
                {
                    //取出对象的Dto
                    var dto = dtos.FirstOrDefault(d => d.Id == id);
                    if (dto != null)
                        if (!hasSetIds.Contains(dto.Id))
                            //根据dto的引用字典处理
                            foreach (var refrence in dto.References)
                            {
                                var refElement = type.References.FirstOrDefault(p => p.Name == refrence.Key);
                                if (refElement != null)
                                {
                                    //保存至已处理的集合中 避免下一层循环引用时重复处理
                                    hasSetIds.Add(dto.Id);
                                    //下层的结果
                                    var results = new List<object>();
                                    foreach (var referenceId in refrence.Value)
                                    {
                                        //为下一层设置引用
                                        SetReferences(refrence.Value, dtos, hasSetIds);
                                        //取出当前层的引用
                                        if (_deSerializedObject.TryGetValue(referenceId, out var value))
                                            results.Add(value);
                                    }

                                    //设置值
                                    if (results.Any())
                                        //如果是多值的属性 直接设置 否则 设置首个
                                        refElement.SetValue(obj,
                                            refElement.Multiple ? results : results.FirstOrDefault());
                                }
                            }
                }
            }
        }

        /// <summary>
        ///     创建构造函数参数比较器
        /// </summary>
        /// <param name="x">构造函数参数X</param>
        /// <param name="y">构造函数参数Y</param>
        /// <returns>大小</returns>
        private int CreateConstructorParameterComparison(SerializationConstructorParameter x,
            SerializationConstructorParameter y)
        {
            //根据具体的类型进行比较
            var xCode = Convert.ToInt32(x.Index.Replace("#", ""));
            var yCode = Convert.ToInt32(y.Index.Replace("#", ""));

            return xCode - yCode;
        }
    }
}