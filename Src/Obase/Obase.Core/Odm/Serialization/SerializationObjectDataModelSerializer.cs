/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化对象数据模型对象序列化器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 16:02:19
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Common;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化对象数据模型对象序列化器
    /// </summary>
    public class SerializationObjectDataModelSerializer
    {
        /// <summary>
        ///     序列化对象数据模型对象
        /// </summary>
        private readonly SerializationObjectDataModel _model;

        /// <summary>
        ///     本次序列化过程中已经序列化的对象字典 key为对象的HashCode value为对象dto
        /// </summary>
        private readonly Dictionary<int, SerializationDataTransferObject> _serializedDto =
            new Dictionary<int, SerializationDataTransferObject>();

        /// <summary>
        ///     本次序列化过程中已经序列化的对象字典 key为对象的HashCode value为对象在本次序列化中分配的ID
        /// </summary>
        private readonly Dictionary<int, string> _serializedObjects = new Dictionary<int, string>();

        /// <summary>
        ///     ID计数器 用于在序列化过程中为每个对象分配一个唯一ID 从$0开始递增
        /// </summary>
        private int _id;

        /// <summary>
        ///     初始化序列化对象数据模型对象序列化器
        /// </summary>
        /// <param name="model">模型</param>
        public SerializationObjectDataModelSerializer(SerializationObjectDataModel model)
        {
            _model = model;
        }

        /// <summary>
        ///     序列化对象数据模型的序列化方法
        ///     最终返回Dto的包装对象
        /// </summary>
        /// <param name="list">要序列化的对象 无论是单值还是多值 都处理为List传入</param>
        /// <returns>Dto的包装对象</returns>
        public SerializationDataTransferObjectWrapper Serialize(List<object> list)
        {
            foreach (var obj in list)
                if (obj != null)
                {
                    //获取对象的模型类型 如果模型中没有定义这个类型 则不处理
                    var type = _model.GetTypeOrNull(obj.GetType());
                    if (type != null)
                        //处理对象的序列化
                        Serialize(obj, true);
                }

            //放入包装类型内
            var wrapper = new SerializationDataTransferObjectWrapper(_serializedDto.Values.ToList())
            {
                ModifiedTime = DateTime.Now
            };
            //返回包装对象
            return wrapper;
        }

        /// <summary>
        ///     某个具体对象的序列化方法
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isRoot">是否为根对象</param>
        /// <returns>dto</returns>
        private List<string> Serialize(object obj, bool isRoot)
        {
            //当前对象的类型
            var currentType = obj.GetType();

            //本层处理过的dto ID集合
            var result = new List<string>();

            //获取对象的模型类型 如果模型中没有定义这个类型 则不处理
            var type = _model.GetTypeOrNull(currentType);
            //如果需要处理
            if (type != null)
            {
                SerializationDataTransferObject dto;
                //如果已经处理过 直接取之前的dto 否则新建一个dto进行处理
                if (!_serializedObjects.ContainsKey(obj.GetHashCode()))
                {
                    //分配ID 
                    var id = $"${_id++}";
                    //构造dto
                    dto = new SerializationDataTransferObject
                    {
                        //dto的类型
                        TypeName = currentType.FullName,
                        //组件的名称
                        AssemblyName = currentType.Assembly.GetName().Name,
                        //为dto分配一个唯一ID
                        Id = id,
                        //是否为根对象
                        IsRoot = isRoot
                    };
                }
                else
                {
                    //从已有的集合中取出之前处理过的dto 并更新是否为根对象
                    dto = _serializedDto[obj.GetHashCode()];
                    dto.IsRoot = isRoot;
                }

                //根据模型处理
                //处理构造函数参数
                foreach (var parameter in type.ConstructorParameters)
                    //需要存储的构造函数参数 调用取值器获取值 进行存储
                    if (parameter.NeedStorage)
                    {
                        var value = parameter.GetValue(obj);
                        if (value != null && value.GetType() != parameter.ValueType)
                            throw new ArgumentException(
                                $"序列化{type.ClrType}的构造函数参数{parameter.Index}时出错,配置的值类型为{parameter.ValueType},实际取到的为{value.GetType()}.");
                        dto.ConstructorParameters[parameter.Index] = Utils.ConvertSerializationValue(value);
                    }

                //处理属性
                foreach (var attribute in type.Attributes)
                {
                    var value = attribute.GetValue(obj);
                    if (value != null && value.GetType() != attribute.ValueType)
                        throw new ArgumentException(
                            $"序列化{type.ClrType}的属性{attribute.Name}时出错,配置的值类型为{attribute.ValueType},实际取到的为{value.GetType()}.");
                    dto.Attributes[attribute.Name] = Utils.ConvertSerializationValue(value);
                }


                //加入已处理的集合
                _serializedObjects[obj.GetHashCode()] = dto.Id;
                _serializedDto[obj.GetHashCode()] = dto;

                //加入结果集合
                result.Add(dto.Id);

                //处理引用
                foreach (var reference in type.References)
                {
                    //取引用的值 无论单值还是集合 都以集合的形式进行处理
                    var value = reference.GetValue(obj);
                    var targets = Utils.GetObjectList(value);

                    //此引用的下层ID集合
                    var idList = new HashSet<string>();
                    foreach (var target in targets)
                        if (target != null && _model.GetTypeOrNull(target.GetType()) != null)
                        {
                            //如果没有处理过 进行处理
                            if (!_serializedObjects.ContainsKey(target.GetHashCode()))
                            {
                                //处理对象的序列化
                                var nextIds = Serialize(target, false);
                                //加入下一层的集合
                                if (nextIds?.Count > 0)
                                    foreach (var nextId in nextIds)
                                        idList.Add(nextId);
                            }
                            //否则 只需要保存之前的ID
                            else
                            {
                                idList.Add(_serializedObjects[target.GetHashCode()]);
                            }
                        }

                    //赋值引用的ID集合
                    if (idList.Count > 0)
                        dto.References[reference.Name] = idList.ToList();
                }
            }

            return result;
        }
    }
}