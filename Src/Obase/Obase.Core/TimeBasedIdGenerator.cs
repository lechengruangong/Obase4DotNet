/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：实现基于时间的标识生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 16:20:24
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core
{
    /// <summary>
    ///     实现基于时间的标识生成器。
    ///     生成规则：当时时间的Unix时间戳 * 100 + 两位随机数。
    /// </summary>
    public class TimeBasedIdGenerator : IDGenerator<long>
    {
        /// <summary>
        ///     Unix起始时间 用于计算UNIX时间戳
        /// </summary>
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     生成标识使用的随机数产生器。
        /// </summary>
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);


        /// <summary>
        ///     生成下一个标识。
        /// </summary>
        public long Next()
        {
            //当前时间的Unix时间戳
            var unixStamp = (long)DateTime.Now.ToUniversalTime().Subtract(Epoch).TotalSeconds;

            return unixStamp * 100 + _random.Next(0, 99);
        }
    }
}