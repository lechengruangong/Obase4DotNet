/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：介入者接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:05:54
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     介入者接口
    /// </summary>
    public interface IIntervener
    {
        /// <summary>
        ///     通知介入者属性已更改。
        /// </summary>
        /// <param name="obj">发生属性更改的对象</param>
        /// <param name="attrName">发生更改的属性</param>
        void AttributeChanged(object obj, string attrName);

        /// <summary>
        ///     请求介入者加载关联。
        ///     对于实体对象，本方法将加载关联引用；对于关联对象则加载关联端。
        /// </summary>
        /// <param name="obj">
        ///     要加载关联的对象
        /// </param>
        /// <param name="referenceName">
        ///     要加载的关联引用或关联端的名称
        /// </param>
        void LoadAssociation(object obj, string referenceName);
    }
}
