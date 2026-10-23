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
        ///     由下层抛出的异常
        /// </summary>
        private readonly Exception _innerException;

        /// <summary>
        ///     创建ThrowException-ConflictHandler实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="innerException">内部异常</param>
        /// <param name="attributeOriginalValueGetter">用于获取属性原值的委托。</param>
        public ThrowExceptionConflictHandler(ObjectDataModel model, Exception innerException,
            GetAttributeValue attributeOriginalValueGetter = null) : base(model)
        {
            _innerException = innerException;
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
            if (obj == null)
            {
                //如果是HostObject为null的情况 说明此时是处理伴随关联时发生了虚假的并发冲突 实际上是因为伴随关联要保存的表内没有正确的插入新对象数据导致的
                //此时需要抛一个特定的异常来表示此时的情况
                var companions = mappingUnit.Companions;
                //没有伴随关联对象时无法判断是哪个类型的问题 与之前的行为保持一致 直接忽略
                if (companions.Count == 0) return;

                var companionTypeNames = new List<string>();
                foreach (var companionMapping in companions)
                {
                    var associationType = Model.GetAssociationType(companionMapping.AssociationObject.GetType());
                    //无法获取关联型或伴随端时无法判断是哪个类型的问题 跳过
                    if (associationType?.CompanionEnd == null) continue;
                    companionTypeNames.Add(associationType.CompanionEnd.EntityType.ClrType.Name);
                }

                throw new ArgumentException(
                    $"发生了伴随关联保存错误:已建立了伴随关联,但参与该关联的关联端对象中存在没有附加到上下文的对象,请检查以下类型的新对象有没有正确的附加:{string.Join(", ", companionTypeNames)}.");
            }
            var objType = Model.GetObjectType(obj.GetType());
            Exception ex;
            switch (conflictType)
            {
                case EConcurrentConflictType.RepeatCreation:
                    ex = new RepeatCreationException(obj, objType, _innerException);
                    break;
                case EConcurrentConflictType.UpdatingPhantom:
                    ex = new UpdatingPhantomException(obj, objType, _innerException);
                    break;
                case EConcurrentConflictType.VersionConflict:
                    //没有版本键 发生版本冲突异常不能表示实际的情况 暂且忽略
                    if (objType.VersionAttributes == null || objType.VersionAttributes.Count == 0)
                    {
                        ex = new ArgumentException("没有版本键，无法处理版本冲突, 请参照内部异常.", _innerException);
                        break;
                    }
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

                    ex = new VersionConflictException(obj, objType, keys, _innerException);
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