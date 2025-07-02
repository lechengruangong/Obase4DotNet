/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示版本冲突的异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:58:04
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     表示版本冲突的异常。
    /// </summary>
    public class VersionConflictException : ConcurrentConflictException
    {
        /// <summary>
        ///     发生冲突的各个对象（主对象和伴随映射对象）的初始版本标识。
        /// </summary>
        private readonly List<ObjectKey> _initialVersionKeys;

        /// <summary>
        ///     创建VersionConflictException实例。
        /// </summary>
        /// <param name="obj">发生冲突的对象。</param>
        /// <param name="objType">发生冲突的对象的类型。</param>
        /// <param name="initVersionKeys">发生冲突的各个对象（主对象和伴随映射对象）的初始版本标识。</param>
        public VersionConflictException(object obj, ObjectType objType, List<ObjectKey> initVersionKeys) : base(obj,
            objType)
        {
            _initialVersionKeys = initVersionKeys;
        }

        /// <summary>
        ///     获取发生冲突的各个对象（主对象和伴随映射对象）的初始版本标识。
        /// </summary>
        public List<ObjectKey> InitialVersionKeys => _initialVersionKeys;

        /// <summary>
        ///     返回异常详细信息。
        ///     信息格式：“发生了并发冲突，更新对象时发现本地版本已过时，对象标识为[ObjectKey]。”
        /// </summary>
        public override string Message
        {
            get
            {
                return
                    $"发生了并发冲突，更新对象时发现本地版本已过时，对象标识为[{string.Join(",", _initialVersionKeys?.Select(p => p.TypeName) ?? new List<string>())}]";
            }
        }
    }
}