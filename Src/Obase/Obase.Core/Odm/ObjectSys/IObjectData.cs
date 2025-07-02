/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象数据规范,符合该规范的数据可用于创建一个对象.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:32:37
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.ObjectSys
{
    /// <summary>
    ///     对象数据规范，符合该规范的数据可用于创建一个对象。
    /// </summary>
    public interface IObjectData
    {
        /// <summary>
        ///     获取指定属性树节点代表的简单属性的值。
        /// </summary>
        /// <param name="attrNode">属性树节点。</param>
        object GetValue(SimpleAttributeNode attrNode);

        /// <summary>
        ///     获取对象标识。
        /// </summary>
        /// 给实现者的说明
        /// 实现该接口时应当对每一个标识成员的值进行验证，如果从存储设备中未能取出任一成员的值（如SQL Server中返回DBNull），应当返回null。
        ObjectKey GetObjectKey();
    }
}