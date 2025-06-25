/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象导航行为.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:34:40
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     描述对象导航行为。
    ///     基于特定的关联，可以从一个对象转移到另一个对象，这个过程称为导航。
    ///     有两种类型的导航。一种是间接导航，即借助于关联对象，先从源对象转移到关联对象，然后再转移到目标对象。另一种是直接导航，即从源对象直接转移到目标对象。
    ///     不管哪种导航都必须基于特定的关联，而导航总是发生在两个关联端之间。基于这一理解，源对象可称为源端，目标对象则可称为目标端。
    ///     在物理层面上看，导航需要借助引用元素（即对象内部的指针）来实现。直接导航需要在源对象定义一个指向目标对象的关联引用，称为直接引用。间接导航则需要两个引用元素，分
    ///     别为定义在源对象的关联引用和定义在关联对象的关联端。前者指向关联对象，称为发出引用；后者指向目标对象，称为到达引用。
    /// </summary>
    public class ObjectNavigation
    {
        /// <summary>
        ///     作为导航依据的关联型。
        /// </summary>
        private readonly AssociationType _associationType;

        /// <summary>
        ///     导航类型。
        /// </summary>
        private readonly ENavigationType _navigationType;

        /// <summary>
        ///     源端名，即作为源端的关联端的名称。值为null表示源端不明确。
        /// </summary>
        private readonly string _sourceEndName;

        /// <summary>
        ///     目标端名，即作为目标端的关联端的名称。值为null表示目标端不明确。
        /// </summary>
        private readonly string _targetEndName;

        /// <summary>
        ///     目标对象类型。目标端不明确时返回null。
        /// </summary>
        private readonly ObjectType _targetType;

        /// <summary>
        ///     创建表示导航的ObjectNavigation实例，指定源端名称和目标端名称。
        /// </summary>
        /// <param name="assoType">关联型。</param>
        /// <param name="source">源端名称。值为null表示源端未明确的间接导航。source与target不能同时为null。</param>
        /// <param name="target">目标端名称。值为null表示目标端未明确的间接导航。source与target不能同时为null。</param>
        public ObjectNavigation(AssociationType assoType, string source, string target)
        {
            if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(target))
                throw new ArgumentNullException(nameof(source), "source与target不能同时为Null");

            _associationType = assoType;
            _sourceEndName = source;
            _targetEndName = target;

            _navigationType = assoType.Visible ? ENavigationType.Indirectly : ENavigationType.Directly;

            //源端
            if (!string.IsNullOrWhiteSpace(source))
                SourceEnd = assoType.AssociationEnds.FirstOrDefault(p => p.Name == source);

            //目标端
            if (!string.IsNullOrWhiteSpace(target))
            {
                TargetEnd = assoType.AssociationEnds.FirstOrDefault(p => p.Name == target);
                _targetType = TargetEnd?.ReferenceType;
            }
        }

        /// <summary>
        ///     获取作为导航依据的关联型。
        /// </summary>
        public AssociationType AssociationType => _associationType;

        /// <summary>
        ///     获取导航类型。
        /// </summary>
        public ENavigationType NavigationType => _navigationType;

        /// <summary>
        ///     获取源端，值为null表示源端不明确。
        /// </summary>
        public AssociationEnd SourceEnd { get; }

        /// <summary>
        ///     获取源端名，即作为源端的关联端的名称。值为null表示源端不明确。
        /// </summary>
        public string SourceEndName => _sourceEndName;

        /// <summary>
        ///     获取目标端，值为null表示目标端不明确。
        /// </summary>
        public AssociationEnd TargetEnd { get; }

        /// <summary>
        ///     获取目标端名，即作为目标端的关联端的名称。值为null表示目标端不明确。
        /// </summary>
        public string TargetEndName => _targetEndName;

        /// <summary>
        ///     获取目标对象类型。目标端不明确时返回null。
        /// </summary>
        public ObjectType TargetType => _targetType;
    }
}