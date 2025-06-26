/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举查询运算名称.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:35:03
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Query
{
    /// <summary>
    ///     枚举查询运算名称。
    /// </summary>
    public enum EQueryOpName
    {
        /// <summary>
        ///     累加运算。
        /// </summary>
        Accumulate,

        /// <summary>
        ///     算术聚合运算。
        /// </summary>
        ArithAggregate,

        /// <summary>
        ///     All测定运算。
        /// </summary>
        All,

        /// <summary>
        ///     Any测定运算。
        /// </summary>
        Any,

        /// <summary>
        ///     类型转换运算。
        /// </summary>
        Cast,

        /// <summary>
        ///     Contains测定运算。
        /// </summary>
        Contains,

        /// <summary>
        ///     计数运算。
        /// </summary>
        Count,

        /// <summary>
        ///     取默认值运算。
        /// </summary>
        DefaultIfEmpty,

        /// <summary>
        ///     去重运算。
        /// </summary>
        Distinct,

        /// <summary>
        ///     索引运算。
        /// </summary>
        ElementAt,

        /// <summary>
        ///     First索引运算。
        /// </summary>
        First,

        /// <summary>
        ///     分组运算。
        /// </summary>
        Group,

        /// <summary>
        ///     包含运算。
        /// </summary>
        Include,

        /// <summary>
        ///     联接运算。
        /// </summary>
        Join,

        /// <summary>
        ///     Last索引运算。
        /// </summary>
        Last,

        /// <summary>
        ///     类型筛选运算。
        /// </summary>
        OfType,

        /// <summary>
        ///     排序运算。
        /// </summary>
        Order,

        /// <summary>
        ///     反序运算。
        /// </summary>
        Reverse,

        /// <summary>
        ///     投影运算。
        /// </summary>
        Select,

        /// <summary>
        ///     顺序相等比较运算。
        /// </summary>
        SequenceEqual,

        /// <summary>
        ///     集运算。
        /// </summary>
        Set,

        /// <summary>
        ///     单值索引运算。
        /// </summary>
        Single,

        /// <summary>
        ///     略过运算。
        /// </summary>
        Skip,

        /// <summary>
        ///     条件略过运算。
        /// </summary>
        SkipWhile,

        /// <summary>
        ///     提取运算。
        /// </summary>
        Take,

        /// <summary>
        ///     条件提取运算。
        /// </summary>
        TakeWhile,

        /// <summary>
        ///     筛选运算。
        /// </summary>
        Where,

        /// <summary>
        ///     合并运算。
        /// </summary>
        Zip,

        /// <summary>
        ///     无参数 全查询
        /// </summary>
        Non
    }
}