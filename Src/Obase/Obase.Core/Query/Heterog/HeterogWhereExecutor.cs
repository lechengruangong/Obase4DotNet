/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构筛选运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:26:14
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     异构筛选运算执行器。
    /// </summary>
    public class HeterogWhereExecutor : HeterogOpExecutor
    {
        /// <summary>
        ///     用于执行同构运算的执行器。在执行异构筛选运算过程中，需要执行两次同构运算：一是执行同构子筛选；二是执行异构子筛选中的基础查询。
        /// </summary>
        private readonly HomogOpExecutor _homogOpExecutor;

        /// <summary>
        ///     初始化HeterogOpExecutor类的新实例。
        /// </summary>
        /// <param name="storageProviderCreator">创建存储提供程序的委托。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        /// <param name="baseQueryProvider"></param>
        public HeterogWhereExecutor(Func<StorageSymbol, IStorageProvider> storageProviderCreator, ObjectDataModel model,
            Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback, IBaseQueryProvider baseQueryProvider = null) : base(
            storageProviderCreator, model,
            preexecutionCallback, postexecutionCallback)
        {
            _homogOpExecutor =
                new HomogOpExecutor(storageProviderCreator, model, preexecutionCallback,
                    postexecutionCallback, baseQueryProvider);
        }

        /// <summary>
        ///     执行异构子筛选。
        /// </summary>
        /// <param name="heterogFactor">作为筛选条件的异构或因子。</param>
        /// <param name="including">包含树。</param>
        /// <param name="mainQuery">主查询。</param>
        /// <param name="resultDict">存储结果集的字典。</param>
        /// <param name="attachObject"></param>
        /// <param name="attachRoot"></param>
        private void ExecuteHeterogSub(OrFactor heterogFactor, AssociationTree including, QueryOp mainQuery,
            IDictionary<IdentityArray, object> resultDict, AttachObject attachObject, bool attachRoot = true)
        {
            //执行基础查询
            WhereOp whereOp = null;
            var baseFactor = heterogFactor.BaseFactor;
            if (baseFactor != null)
            {
                var predicate = baseFactor.ToLambda();
                whereOp = new WhereOp(predicate, Model);
            }

            var baseQuery = mainQuery.ReplaceTail(whereOp);
            //一定为object[]
            var baseInstances =
                ProcessInstances(_homogOpExecutor.Execute(whereOp, baseQuery, including, attachObject, attachRoot));

            var sourceType = heterogFactor.SourceType;
            //剔除已存在的实例
            var objects = baseInstances.Where(o => !resultDict.ContainsKey(sourceType.GetIdentity(o)))
                .Select(p => p).ToArray();
            //生成校验视图
            var checkView = heterogFactor.GenerateCheckView(out var checkAttrs);
            Model.AddType(checkView);
            //生成校验投影运算
            var selectOp = QueryOp.Select(checkView, Model);
            //生成校验查询
            var checkQuery = sourceType.GenerateFilterQuery(objects, selectOp);
            //执行查询
            HeterogQueryProvider.AttachRoot = false;
            var checkViewInstances = HeterogQueryProvider.Execute(checkQuery, including);
            var sourceObjs = ProcessInstances(checkViewInstances);
            //装入字典
            var checkDict = sourceType.MakeDictionary(sourceObjs, checkView);
            //逐个校验
            foreach (var baseResult in baseInstances)
            {
                var identity = sourceType.GetIdentity(baseResult);
                if (checkDict.TryGetValue(identity, out var checkIntanse))
                {
                    var check = true;
                    foreach (var attribute in checkAttrs)
                    {
                        //校验属性的值是否相等
                        var baseInstanceNeedCheckAttr = attribute.Sources?.Length > 0
                            ? attribute.Sources[0].AttributeNode.AttributeName
                            : null;
                        if (baseInstanceNeedCheckAttr == null) continue;
                        var checkValue = attribute.GetValue(checkIntanse).ToString() == sourceType
                            .GetAttribute(baseInstanceNeedCheckAttr).GetValue(baseResult).ToString();
                        if (!checkValue)
                        {
                            check = false;
                            break;
                        }
                    }

                    //放入字典
                    if (check) resultDict.Add(identity, baseResult);
                }
            }
        }

        /// <summary>
        ///     执行异构运算。
        /// </summary>
        /// <param name="heterogOp">要执行的异构运算。</param>
        /// <param name="heterogQuery">要执行的异构运算所在的查询链，它是该查询链的末节点。</param>
        /// <param name="including">包含树。</param>
        /// <param name="attachObject"></param>
        /// <param name="attachRoot"></param>
        public override object Execute(QueryOp heterogOp, QueryOp heterogQuery, AssociationTree including,
            AttachObject attachObject, bool attachRoot = true)
        {
            if (heterogOp is WhereOp whereOp)
            {
                _homogOpExecutor.SetHeterogQueryProvider(HeterogQueryProvider);
                //或因子分解
                var orFactors = whereOp.Decompose(Model);
                //同构和异构拆开
                var homoFactors = orFactors.Where(p => !p.Heterogeneous).ToArray();
                var heterogFactors = orFactors.Where(p => p.Heterogeneous).ToArray();

                var sourceType = heterogFactors[0].SourceType;

                var result = new List<object>();

                var resultDictionary = new Dictionary<IdentityArray, object>();
                foreach (var homoFactor in homoFactors)
                {
                    var predicate = homoFactor.ToLambda();
                    var homoSubWhere = new WhereOp(predicate, Model);
                    var homoSubQuery = heterogOp.ReplaceTail(homoSubWhere);
                    var homogSubInstances = _homogOpExecutor.Execute(homoSubWhere, homoSubQuery, including,
                        attachObject, attachRoot);
                    var homogObjs = ProcessInstances(homogSubInstances);
                    var tempDictionary = sourceType.MakeDictionary(homogObjs);
                    foreach (var temp in tempDictionary)
                        resultDictionary.Add(temp.Key, temp.Value);
                }

                //异构查询
                foreach (var heterogFactor in heterogFactors)
                {
                    ExecuteHeterogSub(heterogFactor, including, heterogQuery, resultDictionary, attachObject,
                        attachRoot);
                    result.AddRange(resultDictionary.Values);
                }

                return ProcessResult(result, sourceType.ClrType);
            }

            throw new ArgumentException("异构筛选运算执行器只能处理WhereOp", nameof(heterogOp));
        }

        /// <summary>
        ///     处理某个子查询结果
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        private object[] ProcessInstances(object instances)
        {
            //将对象查出
            if (instances is IEnumerable iEnumerable)
            {
                var tempResult = new List<object>();
                var enumerator = iEnumerable.GetEnumerator();
                while (enumerator.MoveNext()) tempResult.Add(enumerator.Current);
                if (enumerator is IDisposable disposable) disposable.Dispose();

                instances = tempResult.Count == 0 ? null : tempResult.ToArray();
            }

            if (instances == null)
                return Array.Empty<object>();

            return instances as object[] ?? new[] { instances };
        }

        /// <summary>
        ///     处理最终结果
        /// </summary>
        /// <param name="result"></param>
        /// <param name="clrType"></param>
        /// <returns></returns>
        private object ProcessResult(List<object> result, Type clrType)
        {
            if (result == null)
                return null;

            var realResult = Activator.CreateInstance(typeof(List<>).MakeGenericType(clrType));
            foreach (var r in result)
            {
                var addMethod = realResult.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                addMethod?.Invoke(realResult, new[] { r });
            }

            return realResult;
        }
    }
}