/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象运算执行器基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:22:20
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     为对象运算执行器提供基础实现。
    /// </summary>
    public abstract class OopExecutor : OpExecutor<OopContext>
    {
        /// <summary>
        ///     初始化OopExecutor类的新实例。
        /// </summary>
        /// <param name="queryOp">要执行的查询运算。</param>
        /// <param name="next">运算管道中的下一个执行器。</param>
        protected OopExecutor(QueryOp queryOp, OopExecutor next = null)
            : base(queryOp, next)
        {
        }

        /// <summary>
        ///     执行对象运算。
        /// </summary>
        /// <returns>执行查询运算的结果。</returns>
        /// <param name="sourceObjs">源对象集合</param>
        public object Execute(IEnumerable sourceObjs)
        {
            var context = new OopContext(sourceObjs);
            //按照查询源序列处理
            ((OpExecutor<OopContext>)this).Execute(context);
            return context.Result;
        }

        /// <summary>
        ///     执行对象运算。
        /// </summary>
        /// <returns>执行查询运算的结果。</returns>
        /// <param name="initValue">运算基点值。</param>
        public object Execute(object initValue)
        {
            // 实施说明
            // 
            // 如果运算基点值（initValue）为IEnumerable，将其视为查询源序列，否则视为已知的运算结果参与后续运算。
            // 后者通常发生在补充链上，且主链的最后一步是聚合运算。
            if (initValue is IEnumerable val)
                return Execute(val);
            var obj = initValue is OopContext c ? c.Result : initValue;
            //构造上下文
            var context = new OopContext(obj);
            ((OpExecutor<OopContext>)this).Execute(context);
            if (initValue is OopContext c1)
                c1.Result = context.Result;
            return context.Result;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="whereOp">要执行的Where运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(WhereOp whereOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(WhereExecutor<>), new[] { whereOp.SourceType }, whereOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="takeOp">要执行的Take运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(TakeOp takeOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(TakeExecutor<>), new[] { takeOp.SourceType }, takeOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="skipOp">要执行的Skip运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(SkipOp skipOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(SkipExecutor<>), new[] { skipOp.SourceType }, skipOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="takeWhileOp">要执行的TakeWhile运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(TakeWhileOp takeWhileOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(TakeWhileExecutor<>), new[] { takeWhileOp.SourceType },
                takeWhileOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="skipWhileOp">要执行的SkipWhile运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(SkipWhileOp skipWhileOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(SkipWhileExecutor<>), new[] { skipWhileOp.SourceType },
                skipWhileOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="firstOp">要执行的First运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(FirstOp firstOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(FirstExecutor<>), new[] { firstOp.SourceType }, firstOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="lastOp">要执行的Last运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(LastOp lastOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(LastExecutor<>), new[] { lastOp.SourceType }, lastOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="selectOp">要执行的Select运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(SelectOp selectOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(SelectExecutor<,>),
                new[] { selectOp.SourceType, selectOp.ResultType }, selectOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="collectionSelectOp">要执行的Select运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(CollectionSelectOp collectionSelectOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(SelectExecutor<,,>),
                new[] { collectionSelectOp.SourceType, collectionSelectOp.ElementType, collectionSelectOp.ResultType },
                collectionSelectOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="combiningOp">要执行的Select运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(CombiningSelectOp combiningOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(SelectExecutor<,>),
                new[] { combiningOp.SourceType, combiningOp.ResultType }, combiningOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="groupOp">要执行的Group运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(GroupOp groupOp, OopExecutor next)
        {
            OopExecutor oopExecutor;
            if (groupOp.ElementSelector != null)
                oopExecutor = CreateOopExecutorInstance(typeof(GroupExecutor<,,>),
                    new[] { groupOp.SourceType, groupOp.KeyType, groupOp.ElementType }, groupOp);
            else
                oopExecutor = CreateOopExecutorInstance(typeof(GroupExecutor<,>),
                    new[] { groupOp.SourceType, groupOp.KeyType }, groupOp);
            oopExecutor._next = next;
            return oopExecutor;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="groupAggregationOp">要执行的GroupAggregation运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(GroupAggregationOp groupAggregationOp, OopExecutor next)
        {
            OopExecutor oopExecutor;
            if (groupAggregationOp.ElementSelector == null)
                oopExecutor = CreateOopExecutorInstance(typeof(GroupAggregationExecutor<,,>),
                    new[] { groupAggregationOp.SourceType, groupAggregationOp.KeyType, groupAggregationOp.ResultType },
                    groupAggregationOp);
            else
                oopExecutor = CreateOopExecutorInstance(typeof(GroupAggregationExecutor<,,,>),
                    new[]
                    {
                        groupAggregationOp.SourceType, groupAggregationOp.KeyType, groupAggregationOp.ElementType,
                        groupAggregationOp.ResultType
                    },
                    groupAggregationOp);

            oopExecutor._next = next;
            return oopExecutor;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="orderOp">要执行的Order运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(OrderOp orderOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(OrderExecutor<,>), new[] { orderOp.SourceType, orderOp.KeyType },
                orderOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="distinctOp">要执行的Distinct运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(DistinctOp distinctOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(DistinctExecutor<>), new[] { distinctOp.SourceType },
                distinctOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="reverseOp">要执行的Reverse运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(ReverseOp reverseOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(ReverseExecutor<>), new[] { reverseOp.SourceType }, reverseOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="includeOp">要执行的Include运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(IncludeOp includeOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(IncludeExecutor<,>),
                new[] { includeOp.SourceType, includeOp.TargetType }, includeOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="setOp">要执行的Set运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(SetOp setOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(SetExecutor<>), new[] { setOp.SourceType }, setOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="containsOp">要执行的Contains运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(ContainsOp containsOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(ContainsExecutor<>), new[] { containsOp.SourceType },
                containsOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="allOp">要执行的All运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(AllOp allOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(AllExecutor<>), new[] { allOp.SourceType }, allOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="anyOp">要执行的Any运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(AnyOp anyOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(AnyExecutor<>), new[] { anyOp.SourceType }, anyOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="singleOp">要执行的Single运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(SingleOp singleOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(SingleExecutor<>), new[] { singleOp.SourceType }, singleOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="sequenceEqualOp">要执行的SequenceEqual运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(SequenceEqualOp sequenceEqualOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(SequenceEqualExecutor<>), new[] { sequenceEqualOp.SourceType },
                sequenceEqualOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="elementAtOp">要执行的ElementAt运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(ElementAtOp elementAtOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(ElementAtExecutor<>), new[] { elementAtOp.SourceType },
                elementAtOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="countOp">要执行的Count运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(CountOp countOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(CountExecutor<>), new[] { countOp.SourceType }, countOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="arithAggregateOp">要执行的ArithAggregate运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(ArithAggregateOp arithAggregateOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(ArithAggregateExecutor<,>),
                new[] { arithAggregateOp.SourceType, arithAggregateOp.ResultType }, arithAggregateOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="accumulateOp">要执行的Accumulate运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(AccumulateOp accumulateOp, OopExecutor next)
        {
            var types = new Type[3];
            types[0] = accumulateOp.SourceType;
            types[1] = accumulateOp.SeedType ?? accumulateOp.SourceType;
            types[2] = accumulateOp.ResultType ?? accumulateOp.SourceType;
            var oop = CreateOopExecutorInstance(typeof(AccumulateExecutor<,,>), types, accumulateOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="joinOp">要执行的Join运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(JoinOp joinOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(JoinExecutor<,,,>),
                new[] { joinOp.OuterType, joinOp.InnerType, joinOp.InnerKeyType, joinOp.ResultType }, joinOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="defaultIfEmptyOp">要执行的DefaultIfEmpty运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(DefaultIfEmptyOp defaultIfEmptyOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(DefaultIfEmptyExecutor<>), new[] { defaultIfEmptyOp.SourceType },
                defaultIfEmptyOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="castOp">要执行的Cast运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(CastOp castOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(CastExecutor<>), new[] { castOp.SourceType }, castOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="ofType">要执行的OfType运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(OfTypeOp ofType, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(OfTypeExecutor<>), new[] { ofType.SourceType }, ofType);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     为指定的查询运算创建执行器。
        /// </summary>
        /// <param name="zipOp">要执行的Zip运算。</param>
        /// <param name="next">对象运算管道中的下一个执行器。</param>
        internal static OopExecutor Create(ZipOp zipOp, OopExecutor next)
        {
            var oop = CreateOopExecutorInstance(typeof(ZipExecutor<,,>),
                new[] { zipOp.SourceType, zipOp.SecondType, zipOp.ResultType }, zipOp);
            oop._next = next;
            return oop;
        }

        /// <summary>
        ///     创建对象运算实例。
        /// </summary>
        /// <param name="type">对象运算类型。</param>
        /// <param name="genericTypes">泛型类类型集合。</param>
        /// <param name="args">参数集合。</param>
        /// <returns></returns>
        private static OopExecutor CreateOopExecutorInstance(Type type, Type[] genericTypes, params object[] args)
        {
            //创建一个泛型的对象运算执行器
            if (genericTypes != null && genericTypes != Type.EmptyTypes)
                type = type.MakeGenericType(genericTypes);
            return (OopExecutor)Activator.CreateInstance(type, args);
        }
    }
}