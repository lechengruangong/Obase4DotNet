/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：发生并发冲突时引发异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:56:56
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     发生并发冲突时引发异常。
    /// </summary>
    public class ThrowExceptionConflictHandler : ConcurrentConflictHandler, IRepeatCreationHandler,
        IVersionConflictHandler, IUpdatingPhantomHandler
    {
        /// <summary>
        ///     用于获取属性原值的委托。
        /// </summary>
        private readonly GetAttributeValue _attributeOriginalValueGetter;

        /// <summary>
        ///     创建ThrowException-ConflictHandler实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attributeOriginalValueGetter">用于获取属性原值的委托。</param>
        public ThrowExceptionConflictHandler(ObjectDataModel model,
            GetAttributeValue attributeOriginalValueGetter = null) : base(model)
        {
            _attributeOriginalValueGetter = attributeOriginalValueGetter;
        }

        /// <summary>
        ///     处理重复创建冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        void IRepeatCreationHandler.ProcessConflict(MappingUnit mappingUnit)
        {
            ProcessConflict(mappingUnit, EConcurrentConflictType.RepeatCreation);
        }

        /// <summary>
        ///     处理更新幻影冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        void IUpdatingPhantomHandler.ProcessConflict(MappingUnit mappingUnit)
        {
            ProcessConflict(mappingUnit, EConcurrentConflictType.UpdatingPhantom);
        }

        /// <summary>
        ///     处理版本冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        void IVersionConflictHandler.ProcessConflict(MappingUnit mappingUnit)
        {
            ProcessConflict(mappingUnit, EConcurrentConflictType.VersionConflict);
        }

        /// <summary>
        ///     处理版本冲突
        /// </summary>
        /// <param name="mappingUnit">映射执行器</param>
        /// <param name="conflictType">并发冲突类型</param>
        public override void ProcessConflict(MappingUnit mappingUnit, EConcurrentConflictType conflictType)
        {
            if (mappingUnit == null) return;
            var obj = mappingUnit.HostObject;
            if (obj == null) return;
            var objType = Model.GetObjectType(obj.GetType());
            Exception ex;
            switch (conflictType)
            {
                case EConcurrentConflictType.RepeatCreation:
                    ex = new RepeatCreationException(obj, objType);
                    break;
                case EConcurrentConflictType.UpdatingPhantom:
                    ex = new UpdatingPhantomException(obj, objType);
                    break;
                case EConcurrentConflictType.VersionConflict:
                    //没有版本键 发生版本冲突异常不能表示实际的情况 暂且忽略
                    if (objType.VersionAttributes == null || objType.VersionAttributes.Count == 0)
                        return;
                    //处理所有的映射对象
                    var objItems = mappingUnit.MappingObjects;
                    var keys = new List<ObjectKey>();
                    foreach (var objItem in objItems ?? new List<object>())
                    {
                        if (objItem == null) continue;
                        var members = new List<ObjectKeyMember>();
                        var itemType = Model.GetObjectType(objItem.GetType());
                        if (itemType == null) continue;
                        //加入键属性
                        foreach (var keyMenberName in itemType.KeyFields ?? new List<string>())
                        {
                            var keyMemberValue = itemType.GetElement(keyMenberName);
                            members.Add(
                                new ObjectKeyMember($"{itemType.ClrType.FullName}-{keyMenberName}", keyMemberValue));
                        }

                        //加入版本键
                        foreach (var attrName in itemType.VersionAttributes ?? new List<string>())
                        {
                            var attr = itemType.GetAttribute(attrName);
                            var valObj = _attributeOriginalValueGetter.Invoke(objItem, attr);
                            var member = new ObjectKeyMember(itemType.ClrType.FullName + "-" + attrName, valObj);
                            members.Add(member);
                        }

                        keys.Add(new ObjectKey(itemType, members));
                    }

                    ex = new VersionConflictException(obj, objType, keys);
                    break;
                default:
                    ex = new ArgumentOutOfRangeException(nameof(conflictType), conflictType,
                        $"未知的并发冲突类型{conflictType}");
                    break;
            }

            throw ex;
        }
    }
}