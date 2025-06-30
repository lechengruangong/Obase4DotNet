/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：存储结构映射执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 16:19:08
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;
using Obase.Core.Odm.ObjectSys;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core
{
    /// <summary>
    ///     存储结构映射执行器，将模型结构映射为存储数据结构。
    /// </summary>
    public class StorageStructMappingExecutor : IStructMappingExecutor
    {
        /// <summary>
        ///     根据存储符号创建映射提供程序的委托
        /// </summary>
        private readonly Func<StorageSymbol, IStorageStructMappingProvider> _mappingProviderCreator;

        /// <summary>
        ///     构造存储结构映射执行器
        /// </summary>
        /// <param name="mappingProviderCreator">根据存储符号创建映射提供程序的委托</param>
        public StorageStructMappingExecutor(Func<StorageSymbol, IStorageStructMappingProvider> mappingProviderCreator)
        {
            _mappingProviderCreator = mappingProviderCreator;
        }

        /// <summary>
        ///     执行结构映射。
        /// </summary>
        /// <param name="model"></param>
        public void Execute(ObjectDataModel model)
        {
            var storageSymbol = model.StorageSymbol;
            var provider = _mappingProviderCreator(storageSymbol);

            if (provider == null)
                return;

            var types = new List<StructuralType>();
            //分开处理
            types.AddRange(model.Types.OfType<EntityType>());
            types.AddRange(model.Types.OfType<AssociationType>());

            //具体的映射逻辑
            foreach (var type in types)
                if (type is EntityType entityType)
                {
                    var keyAttrs = entityType.KeyAttributes;
                    var attrFields = MapAttribute(entityType);
                    EnsureTable(entityType.TargetTable, keyAttrs.ToArray(), attrFields.ToArray(), provider);
                }
                else if (type is AssociationType associationType)
                {
                    var attrFields = MapAttribute(associationType);
                    //独立映射
                    if (associationType.Independent)
                    {
                        var endFields = new List<string>();
                        foreach (var end in associationType.AssociationEnds)
                        {
                            //映射关联端
                            var endfield = MapAssocicationEnd(end);
                            endFields.AddRange(endfield.Select(p => p.Name));
                            attrFields.AddRange(endfield);
                        }

                        //合并去重
                        attrFields = attrFields.GroupBy(p => p.Name).Select(p => p.ToList().First()).ToList();
                        EnsureTable(associationType.TargetTable, endFields.ToArray(), attrFields.ToArray(), provider);
                    }
                    else
                    {
                        foreach (var end in associationType.AssociationEnds)
                        {
                            //映射关联端
                            var endfield = MapAssocicationEnd(end);
                            attrFields.AddRange(endfield);
                        }

                        //合并去重
                        attrFields = attrFields.GroupBy(p => p.Name).Select(p => p.ToList().First()).ToList();

                        provider.FieldExist(associationType.TargetTable, attrFields.ToArray(), out var lackOnes,
                            out var shorter);
                        provider.AppendField(associationType.TargetTable, lackOnes);
                        provider.ExpandField(associationType.TargetTable, shorter);

                        foreach (var end in associationType.AssociationEnds)
                        {
                            if (end.IsCompanionEnd())
                                continue;
                            //映射关联端
                            var endfields = MapAssocicationEnd(end).Select(p => p.Name).ToArray();
                            //循环关联端的键属性
                            foreach (var fieldName in endfields)
                                //如果某个关联端的键属性不存在
                                if (!provider.CheckKey(associationType.TargetTable, new[] { fieldName }))
                                    provider.CreateIndex(associationType.TargetTable, new[] { fieldName });
                        }
                    }
                }
        }

        /// <summary>
        ///     处理表
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keyFields"></param>
        /// <param name="fields"></param>
        /// <param name="provider"></param>
        private void EnsureTable(string name, string[] keyFields, Field[] fields,
            IStorageStructMappingProvider provider)
        {
            //创建表
            if (!provider.TableExist(name)) provider.CreateTable(name, fields, keyFields);
            //检测主键索引
            foreach (var keyField in keyFields)
                try
                {
                    //挨个检查
                    if (!provider.CheckKey(name, new[] { keyField }))
                        //没有索引 创建索引
                        provider.CreateIndex(name, new[] { keyField });
                }
                catch (Exception ex)
                {
                    //检查或创建过程中出错 抛出异常由用户检查
                    throw new InvalidOperationException(
                        $"表{name}的索引与主键不完全匹配且暂时无法自动创建,请检查以下字段[{string.Join(",", keyFields)}]中{keyField}字段,自行创建相应字段或者删除此表由自动映射创建.",
                        ex);
                }

            //扩展字段
            provider.FieldExist(name, fields, out var lackOnes, out var shorterOnes);
            provider.AppendField(name, lackOnes);
            provider.ExpandField(name, shorterOnes);
        }

        /// <summary>
        ///     映射属性
        /// </summary>
        /// <param name="objectType"></param>
        private List<Field> MapAttribute(ObjectType objectType)
        {
            var trees = objectType.EnumerateAttributeTree();
            var collector = new FieldCollector();
            foreach (var tree in trees) tree.Accept(collector);
            return collector.Result;
        }

        /// <summary>
        ///     映射关联端
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        private List<Field> MapAssocicationEnd(AssociationEnd end)
        {
            var result = new List<Field>();
            foreach (var mapping in end.Mappings)
            {
                //查找对应端的键属性
                var attribute = end.EntityType.GetAttribute(mapping.KeyAttribute);
                result.Add(new Field(mapping.TargetField, PrimitiveType.FromType(attribute.DataType),
                    attribute.ValueLength, false, attribute.Precision, attribute.Nullable));
            }

            return result;
        }

        /// <summary>
        ///     字段收集器，作为属性树向下访问者收集叶子节点的映射字段。
        /// </summary>
        private class FieldCollector : IAttributeTreeDownwardVisitor<List<Field>>
        {
            /// <summary>
            ///     结果
            /// </summary>
            private List<Field> _result = new List<Field>();

            /// <summary>
            ///     前置访问，即在访问子级前执行操作。
            /// </summary>
            /// <param name="subTree">被访问的子树。</param>
            /// <param name="parentState">访问父级时产生的状态数据。</param>
            /// <param name="outParentState">返回一个状态数据，在遍历到子级时该数据将被视为父级状态。</param>
            /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
            public void Previsit(AttributeTree subTree, object parentState, out object outParentState,
                out object outPrevisitState)
            {
                outParentState = null;
                outPrevisitState = null;

                var treeeNode = subTree.Node;
                //收集简单属性
                if (treeeNode.Attribute is Attribute attribute && !(treeeNode.Attribute is ComplexAttribute))
                {
                    var isSelfIncrese = false;
                    if (attribute.HostType is EntityType entityType)
                        isSelfIncrese = entityType.KeyFields.Contains(attribute.Name) && entityType.KeyIsSelfIncreased;

                    _result.Add(new Field($"{parentState}{attribute.TargetField}",
                        PrimitiveType.FromType(attribute.DataType), attribute.ValueLength, isSelfIncrese,
                        attribute.Precision, attribute.Nullable));
                }
                //复杂属性 向下访问
                else if (treeeNode.Attribute is ComplexAttribute complexAttribute)
                {
                    var connectChar = complexAttribute.MappingConnectionChar;
                    outParentState = parentState + (connectChar == char.MinValue
                        ? ""
                        : $"{complexAttribute.TargetField}{connectChar}");
                }
            }

            /// <summary>
            ///     后置访问，即在访问子级后执行操作。
            /// </summary>
            /// <param name="subTree">被访问的子树。</param>
            /// <param name="parentState">访问父级时产生的状态数据。</param>
            /// <param name="previsitState">前置访问产生的状态数据。</param>
            public void Postvisit(AttributeTree subTree, object parentState, object previsitState)
            {
                //nothing to do 
            }

            /// <summary>
            ///     重置访问者。
            /// </summary>
            public void Reset()
            {
                _result = new List<Field>();
            }

            /// <summary>
            ///     获取遍历属性树的结果。
            /// </summary>
            public List<Field> Result => _result;
        }
    }
}