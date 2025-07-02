/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：特定于SQL源的存储提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:59:09
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query;
using Obase.Core.Query.Oop;
using Obase.Core.Saving;
using Obase.Providers.Sql.Common;
using Obase.Providers.Sql.Rop;
using Obase.Providers.Sql.SqlObject;
using IsolationLevel = System.Data.IsolationLevel;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     特定于SQL源的存储提供程序。
    ///     实施说明
    ///     对ExecutePipeline方法的实现：
    ///     [删除]（1）穿越运算管道取出最后一个节点（RopTerminator），设置包含树；
    ///     （1）实例化RopContext，将参数including作为初始包含树；
    ///     （2）执行运算管道，然后从关系运算上下文取出QuerySql，执行Sql语句；
    ///     （3）使用结果读取器（Query.Rop.ResultReader）工厂生产相应读取器，从Sql执行结果中读取对象实例，参见顺序图“Rop/查询执行器/执行查询”。
    /// </summary>
    public class SqlStorageProvider : IStorageProvider, ITransactionable, IAmbientTransactionable
    {
        /// <summary>
        ///     SQL语句执行器。
        /// </summary>
        private readonly ISqlExecutor _sqlExecutor;

        /// <summary>
        ///     本地事务是否开始
        /// </summary>
        private bool _transactionBegun;

        /// <summary>
        ///     创建SqlStorageProvider实例。
        /// </summary>
        /// <param name="sqlExecutor">SQL语句执行器。</param>
        public SqlStorageProvider(ISqlExecutor sqlExecutor)
        {
            _sqlExecutor = sqlExecutor;
        }

        /// <summary>
        ///     向受.NET事务基础结构支持的事务登记。
        /// </summary>
        public void EnlistTransaction()
        {
            _sqlExecutor.EnlistTransaction();
            _transactionBegun = true;
        }

        /// <summary>
        ///     准备存储资源，如打开数据库连接。
        /// </summary>
        public void PrepareResource()
        {
            _sqlExecutor.OpenConnection();
        }

        /// <summary>
        ///     释放存储资源，如关闭数据库连接。
        /// </summary>
        public void ReleaseResource()
        {
            _sqlExecutor.CloseConnection();
        }

        /// <summary>
        ///     获取一个值，该值指示是否已开启本地事务。
        /// </summary>
        public bool TransactionBegun
        {
            get
            {
                _transactionBegun = _sqlExecutor.TransactionBegun;
                return _transactionBegun;
            }
        }

        /// <summary>
        ///     启动一个新的映射工作流。
        /// </summary>
        /// <returns>一个用于跟踪工作流的对象，它实现了IMappingWorkflow接口。</returns>
        public IMappingWorkflow CreateMappingWorkflow()
        {
            return new SqlMappingWorkflow(_sqlExecutor);
        }

        /// <summary>
        ///     删除符合指定条件的对象。
        /// </summary>
        /// <param name="objType">要删除的对象的类型。</param>
        /// <param name="filterExpression">用于测试对象是否符合条件的断言函数。</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        public int Delete(ObjectType objType, LambdaExpression filterExpression,
            Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback)
        {
            //构造Sql
            GetSourceAndCriterial(filterExpression, objType.Model, out var source, out var criteria);
            var sql = new ChangeSql(source, criteria) { TargetSource = new SimpleSource(objType.TargetTable) };
            if (source is MonomerSource monomerSource) sql.TargetSource = monomerSource;
            //触发事件
            preexecutionCallback?.Invoke(new PreExecuteCommandEventArgs(sql));

            var watch = new Stopwatch();
            int affectCount;
            //执行Sql
            try
            {
                var sqlStr = sql.ToSql(_sqlExecutor.SourceType, out var sqlParameters,
                    _sqlExecutor.CreateParameterCreator());
                watch.Start();
                affectCount = _sqlExecutor.Execute(sqlStr, sqlParameters.ToArray());
                watch.Stop();
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds,
                    affectCount));
            }
            catch (NothingUpdatedException)
            {
                watch.Stop();
                affectCount = 0;
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds,
                    affectCount));
            }
            catch (Exception ex)
            {
                watch.Stop();
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds, ex));
                throw;
            }

            return affectCount;
        }

        /// <summary>
        ///     搜索符合指定条件的对象，为其属性（部分或全部）设置新值。
        /// </summary>
        /// <param name="objType">要修改其属性的对象的类型。</param>
        /// <param name="filterExpression">用于测试对象是否符合条件的断言函数。</param>
        /// <param name="newValues">存储属性新值的字典。</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        public int SetAttributes(ObjectType objType, LambdaExpression filterExpression,
            KeyValuePair<string, object>[] newValues,
            Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback)
        {
            //检查传入的集合
            foreach (var valuePair in newValues)
            {
                //无法找到对应的属性名称
                if (objType.Attributes.All(p => p.TargetField != valuePair.Key))
                    throw new ArgumentException($"映射集合中的{valuePair.Key}无法与{objType.Name}中任意属性相对应.", nameof(newValues));
                //要改动的属性名称为自增主键
                if (objType is EntityType entityType &&
                    entityType.KeyFields.FirstOrDefault(p => p == valuePair.Key) != null &&
                    entityType.KeyIsSelfIncreased)
                    throw new ArgumentException($"不能更改自增主键{valuePair.Key}的值", nameof(newValues));
            }

            var setters = new List<IFieldSetter>();
            foreach (var kvp in newValues)
            {
                var attribute = objType.GetAttribute(kvp.Key) ??
                                objType.Attributes.FirstOrDefault(p => p.TargetField == kvp.Key);
                if (attribute == null)
                    throw new ArgumentException($"映射集合中的{kvp.Key}无法与{objType.Name}中任意属性相对应.", nameof(newValues));
                var set = SqlUtils.GetFieIdSetter(attribute.DataType, attribute.TargetField, kvp.Value);
                if (set != null)
                    setters.Add(set);
            }


            //构造Sql
            GetSourceAndCriterial(filterExpression, objType.Model, out var source, out var criteria);
            var sql = new ChangeSql(source, criteria, setters) { TargetSource = new SimpleSource(objType.TargetTable) };
            if (source is MonomerSource monomerSource) sql.TargetSource = monomerSource;

            //触发事件
            preexecutionCallback?.Invoke(new PreExecuteCommandEventArgs(sql));

            var sqlStr = sql.ToSql(_sqlExecutor.SourceType, out var sqlParameters,
                _sqlExecutor.CreateParameterCreator());
            var watch = new Stopwatch();
            int affectCount;
            //执行Sql
            try
            {
                watch.Start();
                affectCount = _sqlExecutor.Execute(sqlStr, sqlParameters.ToArray());
                watch.Stop();
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds,
                    affectCount));
            }
            catch (NothingUpdatedException)
            {
                watch.Stop();
                affectCount = 0;
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds,
                    affectCount));
            }
            catch (Exception ex)
            {
                watch.Stop();
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds, ex));
                throw;
            }

            return affectCount;
        }

        /// <summary>
        ///     搜索符合指定条件的对象，为其属性（部分或全部）施加一个增量。
        /// </summary>
        /// <param name="objType">要修改其属性的对象的类型。</param>
        /// <param name="filterExpression">用于测试对象是否符合条件的断言函数。</param>
        /// <param name="increaseValues">存储增量值的字典。</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        public int IncreaseAttributes(ObjectType objType, LambdaExpression filterExpression,
            KeyValuePair<string, object>[] increaseValues,
            Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback)
        {
            //检查传入的集合
            foreach (var valuePair in increaseValues)
            {
                //无法找到对应的属性名称
                if (objType.Attributes.All(p => p.TargetField != valuePair.Key))
                    throw new ArgumentException($"映射集合中的{valuePair.Key}无法与{objType.Name}中任意属性相对应.",
                        nameof(increaseValues));
                //要改动的属性名称为自增主键
                if (objType is EntityType entityType &&
                    entityType.KeyFields.FirstOrDefault(p => p == valuePair.Key) != null &&
                    entityType.KeyIsSelfIncreased)
                    throw new ArgumentException($"不能更改自增主键{valuePair.Key}的值", nameof(increaseValues));
            }

            var setters = new List<IFieldSetter>();
            foreach (var kvp in increaseValues)
            {
                var attribute = objType.GetAttribute(kvp.Key);
                if (!attribute.DataType.IsValueType)
                    throw new ArgumentException($"无法为非值类型{kvp.Key}创建增量字段设值器.", nameof(kvp.Key));
                var set = SqlUtils.GetFieIdSetter(attribute.DataType, attribute.TargetField, kvp.Value, true);
                if (set != null)
                    setters.Add(set);
            }


            //构造Sql
            GetSourceAndCriterial(filterExpression, objType.Model, out var source, out var criteria);
            var sql = new ChangeSql(source, criteria, setters) { TargetSource = new SimpleSource(objType.TargetTable) };
            if (source is MonomerSource monomerSource) sql.TargetSource = monomerSource;

            //触发事件
            preexecutionCallback?.Invoke(new PreExecuteCommandEventArgs(sql));
            var sqlStr = sql.ToSql(_sqlExecutor.SourceType, out var sqlParameters,
                _sqlExecutor.CreateParameterCreator());
            var watch = new Stopwatch();
            int affectCount;
            try
            {
                watch.Start();
                affectCount = _sqlExecutor.Execute(sqlStr, sqlParameters.ToArray());
                watch.Stop();
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds,
                    affectCount));
            }
            catch (NothingUpdatedException)
            {
                watch.Stop();
                affectCount = 0;
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds,
                    affectCount));
            }
            catch (Exception ex)
            {
                watch.Stop();
                postexecutionCallback?.Invoke(new PostExecuteCommandEventArgs(sql, (int)watch.ElapsedMilliseconds, ex));
                throw;
            }

            return affectCount;
        }

        /// <summary>
        ///     为指定的查询生成运算管道。
        /// </summary>
        /// <param name="query">要执行的查询。</param>
        /// <param
        ///     name="complement">
        ///     返回补充查询，即生成运算管道后剩余的一段查询链，该部分查询存储服务无法执行须以对象运算补充执行，简称补充查询或补充链。
        /// </param>
        /// <param name="complementBuilder">补充运算管道建造器</param>
        public OpExecutor GeneratePipeline(QueryOp query, out QueryOp complement,
            out OopPipelineBuilder complementBuilder)
        {
            //创建查询运算符管道
            var builder = new RopPipelineBuilder(query?.Model, _sqlExecutor.SourceType);
            query?.Accept(builder);
            complement = builder.OutArgument;
            complementBuilder = new ComplementaryPipelineBuilder();
            return builder.Pipeline;
        }

        /// <summary>
        ///     执行运算管道。
        /// </summary>
        /// <param name="pipeline">要执行的运算管道。</param>
        /// <param name="resultIncluding">指定由运算管道加载的对象须包含的引用（相对于结果类型），必须是同构的。</param>
        /// <param name="attachObject">用于在对象上下文中附加对象的委托 不指定将不执行附加操作</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        /// <param name="attachRoot">指示是否附加根对象</param>
        public object ExecutePipeline(OpExecutor pipeline, AssociationTree resultIncluding,
            Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback, AttachObject attachObject = null, bool attachRoot = true)
        {
            //（1）实例化RopContext，将参数including作为初始包含树；
            //（2）执行运算管道，然后从关系运算上下文取出QuerySql，执行Sql语句；
            //（3）使用结果读取器（Query.Rop.ResultReader）工厂生产相应读取器，从Sql执行结果中读取对象实例，参见顺序图“Rop/查询执行器/执行查询”。
            if (pipeline is RopExecutor ropExecutor)
            {
                var ropContext = new RopContext(ropExecutor.QueryOp.SourceType, ropExecutor.QueryOp.Model,
                    _sqlExecutor.SourceType, resultIncluding);
                ropExecutor.Execute(ropContext);
                //查询结果的sql对象
                var resultSql = ropContext.ResultSql;
                //触发执行Sql之前事件
                var context = new QueryContext(ropExecutor.QueryOp)
                {
                    Command = resultSql
                };
                preexecutionCallback?.Invoke(new QueryEventArgs(context));
                //生成sql语句
                var sql = resultSql.ToSql(_sqlExecutor.SourceType, out var sqlParameters,
                    _sqlExecutor.CreateParameterCreator());
                //查询结果是否为枚举
                var isEnum = ropContext.ResultIsEnum;
                IEnumerable objs = null;
                object result = null;
                var watch = new Stopwatch();
                if (isEnum)
                {
                    var resultReaderFactory = new ResultReaderFactory();

                    //获取表达式树
                    var indudingTree = ropContext.Includings;
                    //获取查询结果类型
                    var resultType = ropContext.ResultModelType;
                    watch.Start();
                    try
                    {
                        var dr = _sqlExecutor.ExecuteReader(sql, sqlParameters.ToArray());
                        watch.Stop();
                        objs = resultReaderFactory.Create(dr, resultType, indudingTree, _sqlExecutor, attachObject);
                        postexecutionCallback?.Invoke(new QueryEventArgs(context));
                    }
                    catch (DbException dbException)
                    {
                        ReleaseResource();
                        watch.Stop();
                        context = new QueryContext(ropExecutor.QueryOp)
                        {
                            Command = resultSql,
                            TimeConsumed = (int)watch.ElapsedMilliseconds,
                            Exception = dbException,
                            HasCanceled = true
                        };
                        postexecutionCallback?.Invoke(new QueryEventArgs(context));
                        throw;
                    }
                    catch (Exception ex)
                    {
                        watch.Stop();
                        context = new QueryContext(ropExecutor.QueryOp)
                        {
                            Command = resultSql,
                            TimeConsumed = (int)watch.ElapsedMilliseconds,
                            Exception = ex,
                            HasCanceled = true
                        };
                        postexecutionCallback?.Invoke(new QueryEventArgs(context));
                        throw;
                    }
                }
                else
                {
                    watch.Start();
                    try
                    {
                        result = _sqlExecutor.ExecuteScalar(sql, sqlParameters.ToArray());
                        watch.Stop();
                        context = new QueryContext(ropExecutor.QueryOp)
                        {
                            Command = resultSql,
                            TimeConsumed = (int)watch.ElapsedMilliseconds
                        };
                        postexecutionCallback?.Invoke(new QueryEventArgs(context));
                    }
                    catch (Exception ex)
                    {
                        watch.Stop();
                        context = new QueryContext(ropExecutor.QueryOp)
                        {
                            Command = resultSql,
                            TimeConsumed = (int)watch.ElapsedMilliseconds,
                            Exception = ex
                        };
                        postexecutionCallback?.Invoke(new QueryEventArgs(context));
                        throw;
                    }
                    finally
                    {
                        ReleaseResource();
                    }
                }

                return objs ?? result;
            }

            return null;
        }

        /// <summary>
        ///     开始本地事务。
        /// </summary>
        /// <param name="isolationLevel">事务隔离级别。</param>
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _sqlExecutor.BeginTransaction(isolationLevel);
            _transactionBegun = true;
        }

        /// <summary>
        ///     提交本地事务。
        /// </summary>
        public void CommitTransaction()
        {
            _sqlExecutor.CommitTransaction();
            _transactionBegun = false;
        }

        /// <summary>
        ///     回滚本地事务。
        /// </summary>
        public void RollbackTransaction()
        {
            _sqlExecutor.RollbackTransaction();
            _transactionBegun = false;
        }

        /// <summary>
        ///     <font color="#0f0f0f">根据过滤表达式生成源和条件。</font>
        /// </summary>
        /// <param name="filterExpression">过滤表达式。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="source">解析出的对象源。</param>
        /// <param name="finalCriteria">解析出的条件。</param>
        private void GetSourceAndCriterial(LambdaExpression filterExpression, ObjectDataModel model,
            out ISource source, out ICriteria finalCriteria)
        {
            //构造当前类型的Rop查询
            var context = new RopContext(filterExpression.Parameters[0].Type, model, _sqlExecutor.SourceType);
            //处理条件
            var tree = new SubTreeEvaluator(filterExpression);
            var criteriaParser = new CriteriaExpressionParser(model, tree, _sqlExecutor.SourceType);
            var criteria = criteriaParser.Parse(filterExpression);
            //用Where执行器解析
            var whereExecutor = new WhereExecutor(filterExpression, criteria);
            whereExecutor.Execute(context);

            //获取最终的源和条件
            source = context.ResultSql.Source;
            finalCriteria = context.ResultSql.Criteria;
        }
    }
}