/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：模型结构映射执行器定义规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:35:46
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     为模型结构映射执行器定义规范，该执行器提供将模型结构映射为其它数据结构（例如关系结构）的方法。
    /// </summary>
    public interface IStructMappingExecutor
    {
        /// <summary>
        ///     执行结构映射。
        /// </summary>
        /// <param name="model"></param>
        void Execute(ObjectDataModel model);
    }
}
