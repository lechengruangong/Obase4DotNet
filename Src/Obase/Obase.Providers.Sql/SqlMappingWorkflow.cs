/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：特定于SQL源的映射工作流.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:56:53
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Obase.Core;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Saving;
using Obase.Providers.Sql.SqlObject;
using Field = Obase.Providers.Sql.SqlObject.Field;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     特定于SQL源的映射工作流。
    /// </summary>
    public class SqlMappingWorkflow : IMappingWorkflow
    {
        /// <summary>
        ///     寄存器（寄存代表映射筛选器片段）。
        /// </summary>
        private readonly List<ICriteria> _segments = new List<ICriteria>();

        /// <summary>
        ///     SQL语句执行器。
        /// </summary>
        private readonly ISqlExecutor _sqlExecutor;

        /// <summary>
        ///     用于级联删除的SQL语句。
        ///     级联规则
        ///     如果基点类型为实体型，探测其参与的所有关联型，对每一关联，如果其它端都是聚合的，删除这些端然后删除其自身；否则不删除该关联也不删除其它端。
        ///     如果基点类型为关联型，如果所有端都是聚合的，删除所有端然后删除关联。
        /// </summary>
        private Stack<ChangeSql> _cascadedSqls = new Stack<ChangeSql>();

        /// <summary>
        ///     用于持久化工作流的SQL语句。
        /// </summary>
        private ChangeSql _changeSql;

        /// <summary>
        ///     是否已设置修改类型
        /// </summary>
        private bool _hasSetChangeType;

        /// <summary>
        ///     创建SqlMappingWorkflow实例。
        /// </summary>
        /// <param name="sqlExecutor">SQL执行器</param>
        internal SqlMappingWorkflow(ISqlExecutor sqlExecutor)
        {
            _sqlExecutor = sqlExecutor;
            _cascadedSqls.Clear();
            _changeSql = new ChangeSql();
        }

        /// <summary>
        ///     开始跟踪修改。
        ///     实施说明
        ///     须清空之前跟踪到的所有修改。
        /// </summary>
        public void Begin()
        {
            _cascadedSqls.Clear();
            _changeSql = new ChangeSql();
        }

        /// <summary>
        ///     接受本次工作流的存储源名称（如数据库表名）。
        /// </summary>
        /// <param name="targetSource">目标源</param>
        public IMappingWorkflow SetSource(string targetSource)
        {
            _changeSql.Source = new SimpleSource(targetSource);
            return this;
        }

        /// <summary>
        ///     指示本次工作流将向存储源插入新对象。
        /// </summary>
        public IMappingWorkflow ForInserting()
        {
            _changeSql.ChangeType = EChangeType.Insert;
            _changeSql.SqlType = ESqlType.Insert;
            _hasSetChangeType = true;
            return this;
        }

        /// <summary>
        ///     指示本次工作流将修改存储源中已有的对象。
        /// </summary>
        public IMappingWorkflow ForUpdating()
        {
            _changeSql.ChangeType = EChangeType.Update;
            _changeSql.SqlType = ESqlType.Update;
            _hasSetChangeType = true;
            return this;
        }

        /// <summary>
        ///     指示本次工作流将删除存储源中的对象。
        /// </summary>
        public IMappingWorkflow ForDeleting()
        {
            _changeSql.ChangeType = EChangeType.Delete;
            _changeSql.SqlType = ESqlType.Delete;
            _hasSetChangeType = true;
            return this;
        }

        /// <summary>
        ///     设置指定域（如数据库表的字段）的值。
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        public IMappingWorkflow SetField(string field, object value)
        {
            _changeSql?.OverwriteField(field, value);
            return this;
        }

        /// <summary>
        ///     对指定域（如数据库表的字段）的值施加一个增量。
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="increment">增量</param>
        public IMappingWorkflow IncreaseField(string field, object increment)
        {
            //构造语句
            var fieldExp = new Field(field);
            var exp = Expression.Fields(fieldExp);
            var incrementExp = Expression.Constant(increment, typeof(long));
            var arithmetic = Expression.Add(exp, incrementExp);
            //覆盖当前值
            _changeSql?.OverwriteField(field, arithmetic);
            return this;
        }

        /// <summary>
        ///     指示本次工作流应当忽略指定域（如数据库表的字段），如果已跟踪到了该域的修改，应当将其排除。
        /// </summary>
        /// <param name="field">字段</param>
        public IMappingWorkflow IgnoreField(string field)
        {
            _changeSql?.RemoveFieldSetter(field);
            return this;
        }

        /// <summary>
        ///     为当前工作流新增一个映射筛选器，该筛选器与已存在的筛选器进行逻辑“与”运算。
        /// </summary>
        /// <returns>新增的映射筛选器。</returns>
        public MappingFilter And()
        {
            return new MappingFilter(this, ELogicalOperator.And, FilterReady, SegmentReady);
        }

        /// <summary>
        ///     为当前工作流新增一个映射筛选器，该筛选器与已存在的筛选器进行逻辑“或”运算。
        /// </summary>
        /// <returns>新增的映射筛选器。</returns>
        public MappingFilter Or()
        {
            return new MappingFilter(this, ELogicalOperator.Or, FilterReady, SegmentReady);
        }

        /// <summary>
        ///     级联删除，即从基点类型开始沿关联关系递归删除。实施者制定具体的级联规则。
        /// </summary>
        /// <param name="initType">起始基点类型</param>
        public void DeleteCascade(ObjectType initType)
        {
            var result = new Stack<ChangeSql>();
            var source = new SimpleSource(initType.TargetTable);

            var sqls = new List<ChangeSql>();
            if (initType is EntityType entityType)
                JoinAssociation(out sqls, source, source, entityType);
            else if (initType is AssociationType associationType)
                JoinAssociationEnd(out sqls, source, source, associationType);

            foreach (var sql in sqls)
            {
                if (_sqlExecutor.SourceType == EDataSource.Sqlite || _sqlExecutor.SourceType == EDataSource.PostgreSql)
                {
                    if (sql.Criteria is InSelectCriteria selectCriteria)
                        selectCriteria.ValueSetSql.Criteria = _changeSql.Criteria;
                    else
                        sql.Criteria = _changeSql.Criteria;
                }
                else
                {
                    sql.Criteria = _changeSql.Criteria;
                }

                result.Push(sql);
            }

            _cascadedSqls = result;
        }

        /// <summary>
        ///     提交工作流。
        /// </summary>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        public void Commit(Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback)
        {
            //有删除 处理删除
            if (_cascadedSqls.Count > 0)
                foreach (var changeSql in _cascadedSqls)
                {
                    //执行Sql前事件
                    preexecutionCallback?.Invoke(
                        new PreExecuteCommandEventArgs(changeSql));
                    var sqlStr = changeSql.ToSql(_sqlExecutor.SourceType, out var sqlParameters,
                        _sqlExecutor.CreateParameterCreator());
                    int affectCount;
                    var watch = new Stopwatch();

                    watch.Start();
                    try
                    {
                        affectCount = _sqlExecutor.Execute(sqlStr, sqlParameters.ToArray());
                        watch.Stop();
                        postexecutionCallback?.Invoke(
                            new PostExecuteCommandEventArgs(changeSql, (int)watch.ElapsedTicks, affectCount));
                    }
                    catch (NothingUpdatedException)
                    {
                        watch.Stop();
                        affectCount = 0;
                        postexecutionCallback?.Invoke(
                            new PostExecuteCommandEventArgs(changeSql, (int)watch.ElapsedTicks, affectCount));
                    }
                    catch (Exception ex)
                    {
                        watch.Stop();
                        postexecutionCallback?.Invoke(
                            new PostExecuteCommandEventArgs(changeSql, (int)watch.ElapsedTicks, ex));
                        throw;
                    }
                }

            //处理一般提交
            if (_changeSql != null && _hasSetChangeType)
            {
                //触发事件
                preexecutionCallback?.Invoke(new PreExecuteCommandEventArgs(_changeSql));
                //转为Sql字符串
                var sqlStr = _changeSql.ToSql(_sqlExecutor.SourceType, out var sqlParameters,
                    _sqlExecutor.CreateParameterCreator());

                //执行Sql
                var watch = new Stopwatch();
                try
                {
                    watch.Start();
                    //执行sql返回自增值
                    _sqlExecutor.Execute(sqlStr, sqlParameters.ToArray());
                    watch.Stop();
                    postexecutionCallback?.Invoke(
                        new PostExecuteCommandEventArgs(_changeSql, (int)watch.ElapsedMilliseconds, 1));
                }
                catch (Exception ex)
                {
                    watch.Stop();
                    postexecutionCallback?.Invoke(
                        new PostExecuteCommandEventArgs(_changeSql, (int)watch.ElapsedMilliseconds, ex));

                    if (_cascadedSqls?.Count > 0 && _sqlExecutor.SourceType == EDataSource.Sqlite &&
                        ex is NothingUpdatedException)
                    {
                        //Sqlite会将级联删除处理为子查询 导致常规删除语句未能删除 此种情况的NothingUpdatedException应忽略
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        ///     提交工作流。
        /// </summary>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        /// <param name="identity">返回存储服务为新对象生成的标识。</param>
        public void Commit(Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback, out object identity)
        {
            //处理一般提交
            if (_changeSql != null)
            {
                //触发事件
                preexecutionCallback?.Invoke(new PreExecuteCommandEventArgs(_changeSql));
                //转为Sql字符串
                var sqlStr = _changeSql.ToSql(_sqlExecutor.SourceType, out var sqlParameters,
                    _sqlExecutor.CreateParameterCreator());

                //自增获取
                var getNewIdentityStr = " ;select @@identity;";
                if (_sqlExecutor.SourceType == EDataSource.Sqlite) getNewIdentityStr = " ;select last_insert_rowid();";

                if (_sqlExecutor.SourceType == EDataSource.PostgreSql) getNewIdentityStr = " ;select lastval();";


                //执行Sql
                var watch = new Stopwatch();
                try
                {
                    watch.Start();
                    //执行sql返回自增值
                    identity = _sqlExecutor.ExecuteScalar($"{sqlStr}{getNewIdentityStr}", sqlParameters.ToArray());
                    watch.Stop();
                    postexecutionCallback?.Invoke(
                        new PostExecuteCommandEventArgs(_changeSql, (int)watch.ElapsedMilliseconds, 1));
                }
                catch (Exception ex)
                {
                    watch.Stop();
                    postexecutionCallback?.Invoke(
                        new PostExecuteCommandEventArgs(_changeSql, (int)watch.ElapsedMilliseconds, ex));
                    throw;
                }
            }
            else
            {
                identity = null;
            }
        }

        /// <summary>
        ///     映射筛选器制作完成时回调
        /// </summary>
        /// <param name="operator">操作符</param>
        private void FilterReady(ELogicalOperator @operator)
        {
            ICriteria criteria;
            if (_segments.Count <= 0) return;
            if (_segments.Count == 1)
            {
                criteria = _segments[0];
            }
            else
            {
                criteria = new ComplexCriteria(_segments[0], _segments[1]);
                for (var i = 2; i < _segments.Count; i++) criteria = criteria.And(_segments[i]);
            }

            _segments.Clear();

            if (_changeSql.Criteria == null)
            {
                _changeSql.Criteria = criteria;
                return;
            }

            switch (@operator)
            {
                case ELogicalOperator.And:
                    _changeSql.Criteria = _changeSql.Criteria.And(criteria);
                    break;
                case ELogicalOperator.Or:
                    _changeSql.Criteria = _changeSql.Criteria.Or(criteria);
                    break;
            }
        }

        /// <summary>
        ///     映射筛选器片段制作完成时回调
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="val">值</param>
        private void SegmentReady(string field, object val)
        {
            var sourceName = ((SimpleSource)_changeSql.Source).Name;
            if (val == null)
            {
                _segments.Add(new StringCriteria(sourceName, field, ERelationOperator.Equal, null));
            }
            else if (val is string str)
            {
                _segments.Add(new StringCriteria(sourceName, field, ERelationOperator.Equal, str));
            }
            else if (val is char cha)
            {
                _segments.Add(new CharCriteria(sourceName, field, ERelationOperator.Equal, cha));
            }
            else if (val is DateTime dateTime)
            {
                _segments.Add(new DateTimeCriteria(sourceName, field, ERelationOperator.Equal, dateTime));
            }
            else
            {
                var criteria = typeof(NumericCriteria<>).MakeGenericType(val.GetType());
                _segments.Add((ICriteria)Activator.CreateInstance(criteria, sourceName, field, ERelationOperator.Equal,
                    val));
            }
        }

        /// <summary>
        ///     遍历指定实体型的关联引用，连接其映射表，同时返回Delete-SQL集合。
        /// </summary>
        /// <param name="deletionSqls">Delete-SQL集合。</param>
        /// <param name="leftSource">Join运算的左操作数。</param>
        /// <param
        ///     name="entitySource">
        ///     当左操作数为连接源时，指定具体参与连接运算的简单源；当左操作数为简单源时，为左操作数自身。
        /// </param>
        /// <param name="entityType">要与其关联引用的映射源进行连接的实体型。</param>
        /// <param name="aliasRoot">别名根，用于与关联引用名称串联生成右操作数（即关联引用的映射表）的别名。默认值为空字符串。</param>
        /// <param name="currentLevel">连接层级。默认值为1。</param>
        private void JoinAssociation(out List<ChangeSql> deletionSqls, ISource leftSource, ISource entitySource,
            EntityType entityType, string aliasRoot = "", int currentLevel = 1)
        {
            deletionSqls = new List<ChangeSql>();

            if (currentLevel <= 3)
                foreach (var re in entityType.AssociationReferences)
                {
                    var newSource = leftSource;
                    var alias = aliasRoot + "_" + re.Name;
                    var assoSource = entitySource;

                    //如果执行器不为Sqlite 此处尝试处理为InnerJoin
                    if (_sqlExecutor.SourceType != EDataSource.Sqlite &&
                        _sqlExecutor.SourceType != EDataSource.PostgreSql)
                        if (!re.LeftAsAssociationTable)
                        {
                            assoSource = new SimpleSource(re.AssociationType.TargetTable, alias);
                            var end = re.AssociationType.GetAssociationEnd(re.LeftEnd);
                            var c = GenerateJoinCriteria(assoSource, entitySource, end);
                            newSource = newSource.InnerJoin(assoSource, c);
                            if (re.AssociationType.Independent)
                            {
                                var sql = new ChangeSql(newSource, EChangeType.Delete)
                                    { TargetSource = (SimpleSource)assoSource };
                                deletionSqls.Add(sql);
                            }
                        }

                    JoinAssociationEnd(out var sqls, newSource, assoSource, re.AssociationType, re.LeftEnd, alias,
                        currentLevel + 1);
                    deletionSqls.AddRange(sqls);
                }
        }

        /// <summary>
        ///     遍历指定关联型的端，连接其映射表，同时返回Delete-SQL集合。
        /// </summary>
        /// <param name="deletionSqls">Delete-SQL集合。</param>
        /// <param name="leftSource">Join运算的左操作数。</param>
        /// <param name="assoSource">当左操作数为连接源时，指定具体参与连接运算的简单源；当左操作数为简单源时，为左操作数自身。</param>
        /// <param name="assoType">要与其关联端的映射源进行连接的关联型。</param>
        /// <param name="excludedEnd">要排除的关联端。默认值为空字符串，表示不排除任何关联端。</param>
        /// <param name="aliasRoot">别名根，用于与关联引用名称串联生成右操作数（即关联引用的映射表）的别名。默认值为空字符串。</param>
        /// <param name="currentLevel">连接层级。默认值为1。</param>
        private void JoinAssociationEnd(out List<ChangeSql> deletionSqls, ISource leftSource, ISource assoSource,
            AssociationType assoType, string excludedEnd = "", string aliasRoot = "", int currentLevel = 1)
        {
            deletionSqls = new List<ChangeSql>();

            var ends = assoType.AssociationEnds.OrderBy(assoType.IsCompanionEnd).ToList();
            foreach (var end in ends)
                if (end.Name != excludedEnd)
                    //为聚合的端处理级联删除
                    if (end.IsAggregated)
                    {
                        var newSource = leftSource;
                        var alias = aliasRoot + "_" + end.Name;
                        var endSource = assoSource;
                        //非伴随端 直接连接
                        if (!assoType.IsCompanionEnd(end))
                        {
                            endSource = new SimpleSource(end.EntityType.TargetTable, alias);
                            var c = GenerateJoinCriteria(assoSource, endSource, end);
                            newSource = newSource.InnerJoin(endSource, c);
                        }

                        //非Sqlite 处理为InnerJoin
                        if (_sqlExecutor.SourceType != EDataSource.Sqlite &&
                            _sqlExecutor.SourceType != EDataSource.PostgreSql)
                        {
                            //创建删除Sql语句
                            var sql = new ChangeSql(newSource, EChangeType.Delete)
                                { TargetSource = (SimpleSource)endSource };
                            //加入级联删除
                            deletionSqls.Add(sql);
                        }
                        else
                        {
                            assoSource = new SimpleSource(end.EntityType.TargetTable);
                            //处理为子查询
                            var fieldExps = new List<Expression>();
                            //处理键属性映射
                            foreach (var mapping in end.Mappings)
                            {
                                var field = new Field((MonomerSource)assoSource, mapping.TargetField);
                                var fieldExp = new FieldExpression(field);
                                fieldExps.Add(fieldExp);
                            }

                            //连接所有键属性
                            var funcExp = new FunctionExpression("concat", fieldExps.ToArray());
                            var column = new ExpressionColumn
                            {
                                Expression = funcExp
                            };
                            var selectionSet = new SelectionSet(column);
                            //连接其他的端
                            var otherEnds = ends.Where(p => p != end).ToList();
                            foreach (var other in otherEnds)
                            {
                                //构造joinedSource
                                var otherEndSource = new SimpleSource(other.EntityType.TargetTable);
                                var c = GenerateJoinCriteria(assoSource, otherEndSource, other);
                                //左连接
                                newSource = otherEndSource.LeftJoin(assoSource, c);
                            }

                            //创建子查询
                            var querySql = new QuerySql(newSource)
                            {
                                SelectionSet = selectionSet
                            };

                            //创建删除Sql语句
                            var criteria = new InSelectCriteria(funcExp, querySql);
                            var changeSql = new ChangeSql(assoSource, EChangeType.Delete, criteria)
                                { TargetSource = (SimpleSource)endSource };
                            //加入级联删除
                            deletionSqls.Add(changeSql);
                        }

                        JoinAssociation(out var sqls, newSource, endSource, end.EntityType, alias,
                            currentLevel + 1);
                        deletionSqls.AddRange(sqls);
                    }
        }

        /// <summary>
        ///     生成连接条件。
        /// </summary>
        /// <param name="assoSource">关联型的映射表。</param>
        /// <param name="endSource">关联端的映射表。</param>
        /// <param name="end">要连接的关联端。</param>
        private ICriteria GenerateJoinCriteria(ISource assoSource, ISource endSource, AssociationEnd end)
        {
            ICriteria c = null;
            foreach (var mapping in end.Mappings)
            {
                var endAttr = end.EntityType.GetAttribute(mapping.KeyAttribute);
                var segment = new FieldCriteria(assoSource, mapping.TargetField, ERelationOperator.Equal, endSource,
                    endAttr.TargetField);
                c = c == null ? segment : c.And(segment);
            }

            return c;
        }
    }
}