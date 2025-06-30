/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：更改通知模块,用于发送对象更改通知.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:45:40
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core.DependencyInjection;
using Obase.Core.Odm;
using Obase.Core.Saving;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     更改通知模块，用于发送对象更改通知。
    ///     订阅ISavingPipeline.EndSavingUnit、IDeletingPipeline.
    ///     EndDeletingGroup和IDirectlyChangingPipeline.EndDirectlyChanging事件。
    ///     更改通知的数据结构及生成通知的算法参见设计文档“执行映射/更改通知”章节。
    /// </summary>
    public class ChangeNoticeModule : IMappingModule
    {
        /// <summary>
        ///     数据模型
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     变更消息发送器
        /// </summary>
        private readonly IChangeNoticeSender _sender;

        /// <summary>
        ///     更改通知模块
        /// </summary>
        /// <param name="model">数据模型</param>
        /// <param name="sender">变更消息发送器</param>
        public ChangeNoticeModule(ObjectDataModel model, IChangeNoticeSender sender)
        {
            _model = model;
            _sender = sender;
        }

        /// <summary>
        ///     初始化更改通知模块
        /// </summary>
        /// <param name="context">上下文</param>
        public ChangeNoticeModule(ObjectContext context)
        {
            _model = context.Model;
            var contextType = context.GetType();
            var container = ServiceContainerInstance.Current.GetServiceContainer(contextType);
            if (container == null)
                throw new ArgumentNullException(nameof(contextType),
                    $"无法找到{contextType.FullName}的依赖注入容器,请使用ObaseDenpendencyInjection注册并建造服务容器.");

            var sender = container.GetService<IChangeNoticeSender>();

            _sender = sender ?? throw new ArgumentNullException(nameof(IChangeNoticeSender),
                $"无法找到{contextType.FullName}的IChangeNoticeSender服务,请使用ObaseDenpendencyInjection注册IChangeNoticeSender为单例的服务.");
        }

        /// <summary>
        ///     初始化映射模块。
        /// </summary>
        /// <param name="savingPipeline">"保存"管道。</param>
        /// <param name="deletingPipeline">"删除"管道。</param>
        /// <param name="queryPipeline">"查询"管道。</param>
        /// <param name="directlyChangingPipeline">"就地修改"管道。</param>
        /// <param name="objectContext">对象上下文</param>
        public void Init(ISavingPipeline savingPipeline, IDeletingPipeline deletingPipeline,
            IQueryPipeline queryPipeline, IDirectlyChangingPipeline directlyChangingPipeline,
            ObjectContext objectContext)
        {
            //订阅事件
            savingPipeline.EndSavingUnit += SavingPipeline_EndSavingUnit;
            deletingPipeline.EndDeletingGroup += DeletingPipeline_EndDeletingGroup;
            directlyChangingPipeline.EndDirectlyChanging += DirectlyChangingPipeline_EndDirectlyChanging;
        }

        /// <summary>
        ///     保存结束事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavingPipeline_EndSavingUnit(object sender, EndSavingUnitEventArgs e)
        {
            var writers = new List<IChangeNoticeWriter>();
            //判空
            if (e.MappingUnit == null)
                return;

            //取出信息
            var host = e.MappingUnit.HostObject;
            if (host == null)
                return;
            var objectType = _model.GetObjectType(host.GetType());
            var companions = e.MappingUnit.Companions;

            //不通知创建 且保存状态为创建 返回
            if (!objectType.NotifyCreation && e.HostObjectStatus == EObjectStatus.Added) return;
            //不通知更新 且保存状态为更新 返回
            if (!objectType.NotifyUpdate && e.HostObjectStatus == EObjectStatus.Modified) return;
            //加入编写器集合
            writers.Add(GenerateObjectChangeWriters(host, objectType, e.HostObjectStatus));

            //发送伴随通知
            if (objectType.NotifyUpdate)
                foreach (var companion in companions)
                {
                    var companionAssociationType = _model.GetAssociationType(companion.AssociationObject.GetType());
                    EObjectStatus value = 0;
                    if (companionAssociationType.NotifyCreation) value |= EObjectStatus.Added;
                    if (companionAssociationType.NotifyUpdate) value |= EObjectStatus.Modified;
                    if (companionAssociationType.NotifyDeletion) value |= EObjectStatus.Deleted;
                    if ((value & companion.Status) == companion.Status)
                        //加入编写器集合
                        writers.Add(GenerateObjectChangeWriters(companion.AssociationObject, companionAssociationType,
                            companion.Status));
                }

            SendNotices(writers);
        }

        /// <summary>
        ///     直接修改结束事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirectlyChangingPipeline_EndDirectlyChanging(object sender, EndDirectlyChangingEventArgs e)
        {
            var writers = new List<IChangeNoticeWriter>();

            //获取当前对象类型
            var objectType = _model.GetObjectType(e.Type);
            //检查通知状态
            if (!objectType.NotifyUpdate && (e.ChangeType == EDirectlyChangeType.Increment ||
                                             e.ChangeType == EDirectlyChangeType.Update)) return;
            if (!objectType.NotifyDeletion && e.ChangeType == EDirectlyChangeType.Delete) return;

            //加入编写器集合
            writers.Add(GenerateDirectlyChangingNoticeWriters(objectType, e.Expression, e.ChangeType, e.NewValues));

            SendNotices(writers);
        }

        /// <summary>
        ///     删除结束事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeletingPipeline_EndDeletingGroup(object sender, EndDeletingGroupEventArgs e)
        {
            var writers = new List<IChangeNoticeWriter>();

            //判空
            if (e.Objects == null)
                return;

            //取出信息
            var objects = e.Objects;
            //不通知删除 不发送消息
            if (!e.ObjectType.NotifyDeletion) return;

            foreach (var obj in objects)
                //处理每个对象
                writers.Add(GenerateObjectChangeWriters(obj, e.ObjectType, EObjectStatus.Deleted));

            SendNotices(writers);
        }

        /// <summary>
        ///     生成对象修改通知
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="objectStatus">修改状态</param>
        /// <returns></returns>
        private IChangeNoticeWriter GenerateObjectChangeWriters(object obj, ObjectType objectType,
            EObjectStatus objectStatus)
        {
            string changeAction;
            switch (objectStatus)
            {
                case EObjectStatus.Unchanged:
                    changeAction = string.Empty;
                    break;
                case EObjectStatus.Added:
                    changeAction = "Create";
                    break;
                case EObjectStatus.Deleted:
                    changeAction = "Delete";
                    break;
                case EObjectStatus.Modified:
                    changeAction = "Update";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectStatus), objectStatus, "未知的对象修改类型");
            }

            var objectKeyList = new List<ObjectAttribute>();
            var objectAttributeList = new List<ObjectAttribute>();

            //实体型和关联型通知
            switch (objectType)
            {
                case EntityType entityType:
                {
                    foreach (var attribute in entityType.Attributes)
                    {
                        if (entityType.KeyAttributes != null && entityType.KeyAttributes.Contains(attribute.Name))
                            objectKeyList.Add(new ObjectAttribute
                            {
                                Attribute = attribute.Name,
                                Value = ObjectSystemVisitor.GetValue(obj, objectType, attribute.Name)
                            });

                        GetNoticeAttribute(obj, objectType, attribute, objectAttributeList);
                    }

                    break;
                }
                case AssociationType associationType:
                {
                    foreach (var attribute in associationType.Attributes)
                    {
                        if (associationType.AssociationEnds != null)
                            foreach (var end in associationType.AssociationEnds)
                                objectKeyList.AddRange(end.Mappings.Select(mapp => new ObjectAttribute
                                {
                                    Attribute = mapp.KeyAttribute,
                                    Value = ObjectSystemVisitor.GetValue(obj, associationType, mapp.KeyAttribute)
                                }));

                        GetNoticeAttribute(obj, objectType, attribute, objectAttributeList);
                    }

                    break;
                }
            }

            return new ObjectChangeNoticeWriter(changeAction, objectType.Namespace, objectType.Name,
                objectAttributeList, objectKeyList);
        }

        /// <summary>
        ///     获取要包含在通知内的属性和值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="attribute">属性</param>
        /// <param name="objectAttributeList">通知属性列表</param>
        private void GetNoticeAttribute(object obj, ObjectType objectType, Attribute attribute,
            List<ObjectAttribute> objectAttributeList)
        {
            if (objectType.NoticeAttributes != null &&
                objectType.NoticeAttributes.Contains(attribute.Name))
                //处理通知属性
                objectAttributeList.Add(new ObjectAttribute
                {
                    Attribute = attribute.Name,
                    Value = ObjectSystemVisitor.GetValue(obj, objectType, attribute.Name)
                });
        }

        /// <summary>
        ///     生成就地修改通知
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <param name="expression">修改表达式</param>
        /// <param name="changeType">修改类型</param>
        /// <param name="newValues">字段值键值对</param>
        /// <returns></returns>
        private IChangeNoticeWriter GenerateDirectlyChangingNoticeWriters(ObjectType objectType, Expression expression,
            EDirectlyChangeType changeType, KeyValuePair<string, object>[] newValues)
        {
            //处理字段值键值对
            var realValues = new Dictionary<string, object>();
            if (newValues != null && newValues.Length > 0)
                foreach (var valuePair in newValues)
                    realValues.Add(valuePair.Key, valuePair.Value);

            string changeAction;
            switch (changeType)
            {
                case EDirectlyChangeType.Delete:
                    changeAction = "Delete";
                    break;
                case EDirectlyChangeType.Update:
                    changeAction = "Update";
                    break;
                case EDirectlyChangeType.Increment:
                    changeAction = "Increase";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(changeType), changeType, "未知的就地修改类型.");
            }

            return new DirectlyChangingNoticeWriter(changeAction, objectType.Namespace, objectType.Name,
                expression.ToString(), changeType, realValues);
        }

        /// <summary>
        ///     发送通知
        /// </summary>
        /// <param name="writers"></param>
        private void SendNotices(List<IChangeNoticeWriter> writers)
        {
            //发送
            foreach (var writer in writers)
                _sender.Send(writer.Write());
        }
    }
}