/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：使用存储标记判断的异构存储断言提供器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:29:57
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;

namespace Obase.Core.Query
{
    /// <summary>
    ///     断言当前节点与根节点是否为存储异构的。如果节点代表的类型未定义异构存储扩展，使用模型默认的存储标记。
    ///     实施说明
    ///     调用私有方法GetStorageSymbol获取节点代表类型的存储标记。
    /// </summary>
    public class StorageHeterogeneityPredicationProvider : HeterogeneityPredicationProvider
    {
        /// <summary>
        ///     根节点代表类型的存储标记。
        /// </summary>
        private StorageSymbol _rootSymbol = StorageSymbols.Current.Default;

        /// <summary>
        ///     根节点
        /// </summary>
        public StorageSymbol RootSymbol => _rootSymbol;

        /// <summary>
        ///     判定当前实例与另一个实例是否相等，实现IEquatable的方法。
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(HeterogeneityPredicationProvider other)
        {
            if (other == null) return false;
            if (other is StorageHeterogeneityPredicationProvider) return GetType().FullName == other.GetType().FullName;
            return false;
        }

        /// <summary>
        ///     比较当前节点与根节点在关注特性上的异同。
        /// </summary>
        /// <returns>如果相同返回true，否则返回false。</returns>
        /// <param name="currentNode">当前节点。</param>
        public override bool Compare(AssociationTreeNode currentNode)
        {
            return _rootSymbol == GetStorageSymbol(currentNode.RepresentedType);
        }

        /// <summary>
        ///     判定当前实例与另一个实例是否相等，重写Object.Equals方法。
        /// </summary>
        /// <returns>
        ///     相等返回true，否则返回false。
        ///     给实施者的说明
        ///     对于异构断言提供程序而言，“相等”的含义是采用了相同的断言算法及参数（如果有），而不应关注是否为同一个提供程序实例。基于此含义，一个可行的实施方案是使用具体提供
        ///     程序类的完全限定名作为判定依据。
        /// </returns>
        /// <param name="other"></param>
        public override bool Equals(object other)
        {
            if (other is StorageHeterogeneityPredicationProvider storage) return Equals(storage);
            return false;
        }

        /// <summary>
        ///     返回异构断言提供程序实例的Hash码，重写Object.GetHashCode方法。
        ///     给实施者的说明
        ///     对于异构断言提供程序而言，“相等”的含义是采用了相同的断言算法及参数（如果有），而不应关注是否为同一个提供程序实例。
        ///     因此，应当确保采用同一断言方案及参数的提供程序具有相同的Hash码。一个可行的实施方案是基于具体提供程序类的完全限定名生成Hash码。
        /// </summary>
        public override int GetHashCode()
        {
            return GetType().FullName?.GetHashCode() ?? 0;
        }

        /// <summary>
        ///     获取类型的存储标记。
        ///     实施说明
        ///     将未定义存储扩展的类型视为使用模型默认的存储标记。具体实现请参见活动图“获取存储标记”。
        /// </summary>
        /// <param name="modelType">要获取其存储标记的类型。</param>
        private StorageSymbol GetStorageSymbol(StructuralType modelType)
        {
            //视图 从源类型获取存储标记
            if (modelType is TypeView typeView) return GetStorageSymbol(typeView.Source);
            //对象类型 从扩展获取存储标记
            if (modelType is ObjectType)
            {
                var extension = modelType.GetExtension(typeof(HeterogStorageExtension));
                if (extension != null)
                {
                    var h = (HeterogStorageExtension)extension;
                    return h.StorageSymbol;
                }

                return modelType.Model.StorageSymbol ?? StorageSymbols.Current.Default;
            }

            return null;
        }

        /// <summary>
        ///     寄存根节点的关注特性。
        /// </summary>
        /// <param name="rootNode">根节点。</param>
        public override void RegisterRoot(AssociationTreeNode rootNode)
        {
            var extension = rootNode.RepresentedType?.GetExtension(typeof(HeterogStorageExtension));
            if (extension != null)
            {
                var h = (HeterogStorageExtension)extension;
                _rootSymbol = h.StorageSymbol;
            }
        }
    }
}