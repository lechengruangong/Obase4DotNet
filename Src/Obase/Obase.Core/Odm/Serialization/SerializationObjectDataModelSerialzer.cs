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
    public class SerializationObjectDataModelSerialzer
    {
        /// <summary>
        ///     ID计数器 用于在序列化过程中为每个对象分配一个唯一ID 从$0开始递增
        /// </summary>
        private static int _id;

        /// <summary>
        ///     序列化对象数据模型对象
        /// </summary>
        private readonly SerializationObjectDataModel _model;

        /// <summary>
        ///     本次序列化过程中已经序列化的对象字典 key为对象的HashCode value为对象在本次序列化中分配的ID
        /// </summary>
        private readonly Dictionary<int, string> _serializedObjects = new Dictionary<int, string>();

        /// <summary>
        ///     初始化序列化对象数据模型对象序列化器
        /// </summary>
        /// <param name="model"></param>
        public SerializationObjectDataModelSerialzer(SerializationObjectDataModel model)
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
            var dtos = new List<SerializationDataTransferObject>();
            foreach (var obj in list)
                if (obj != null)
                {
                    //获取对象的模型类型 如果模型中没有定义这个类型 则不处理
                    var type = _model.GetTypeOrNull(obj.GetType());
                    if (type != null)
                    {
                        //处理对象的序列化
                        var dto = Serialize(obj, true);
                        //将dto添加到结果集合中
                        if (dto?.Count > 0)
                            dtos.AddRange(dto);
                    }
                }

            //放入包装类型内
            var wrapper = new SerializationDataTransferObjectWrapper(dtos)
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
        private List<SerializationDataTransferObject> Serialize(object obj, bool isRoot)
        {
            //当前对象的类型
            var currentType = obj.GetType();

            //本层和下一层的结果集合
            var result = new List<SerializationDataTransferObject>();

            //获取对象的模型类型 如果模型中没有定义这个类型 则不处理
            var type = _model.GetTypeOrNull(currentType);
            //如果需要处理
            if (type != null)
            {
                //分配ID 
                var id = $"${_id++}";
                //构造dto
                var dto = new SerializationDataTransferObject
                {
                    //dto的类型
                    TypeName = currentType.FullName,
                    //为dto分配一个唯一ID
                    Id = id,
                    //是否为根对象
                    IsRoot = isRoot
                };

                //根据模型处理
                //处理构造函数参数
                foreach (var parameter in type.ConstructorParameters)
                    //需要存储的构造函数参数 调用取值器获取值 进行存储
                    if (parameter.NeedStorage)
                        dto.ConstructorParameters[parameter.Index] = parameter.GetValue(obj);

                //处理属性
                foreach (var attribute in type.Attributes)
                    dto.Attributes[attribute.Name] = attribute.GetValue(obj);

                //加入已处理的集合
                _serializedObjects[obj.GetHashCode()] = id;

                //加入结果集合
                result.Add(dto);

                //处理引用
                foreach (var reference in type.References)
                {
                    //取引用的值 无论单值还是集合 都以集合的形式进行处理
                    var value = reference.GetValue(obj);
                    var targets = Utils.GetObjectList(value);

                    //此引用的下层ID集合
                    var idList = new List<string>();
                    foreach (var target in targets)
                        if (target != null && _model.GetTypeOrNull(target.GetType()) != null)
                        {
                            //如果没有处理过 进行处理
                            if (!_serializedObjects.ContainsKey(target.GetHashCode()))
                            {
                                //处理对象的序列化
                                var nextDtos = Serialize(target, false);
                                //加入下一层的集合
                                if (nextDtos?.Count > 0)
                                {
                                    idList.AddRange(nextDtos.Select(d => d.Id).ToList());
                                    result.AddRange(nextDtos);
                                }
                            }
                            //否则 只需要保存之前的ID
                            else
                            {
                                idList.Add(_serializedObjects[target.GetHashCode()]);
                            }
                        }

                    //赋值引用的ID集合
                    if (idList.Count > 0)
                        dto.References[reference.Name] = idList;
                }
            }

            return result;
        }
    }
}