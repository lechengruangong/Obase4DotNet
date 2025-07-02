/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为存储结构映射提供程序定义规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:58:26
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core
{
    /// <summary>
    ///     为存储结构映射提供程序定义规范，该提供程序实现在存储服务（如数据库）中建立存储结构的系列方法。
    /// </summary>
    public interface IStorageStructMappingProvider
    {
        /// <summary>
        ///     向指定的表追加字段。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">要追加的字段。</param>
        void AppendField(string tableName, Field[] fields);

        /// <summary>
        ///     索引一致性检查，确认表的既有索引与指定索引一致。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="keyFields">标识属性。</param>
        bool CheckKey(string tableName, string[] keyFields);

        /// <summary>
        ///     创建索引。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">索引字段的名称序列。</param>
        void CreateIndex(string tableName, string[] fields);

        /// <summary>
        ///     创建表。
        /// </summary>
        /// <param name="name">表名。</param>
        /// <param name="fields">表的字段。</param>
        /// <param name="keyFields">标识字段的名称序列。</param>
        void CreateTable(string name, Field[] fields, string[] keyFields);

        /// <summary>
        ///     扩大指定字段的长度。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">要增加宽度的字段。</param>
        void ExpandField(string tableName, Field[] fields);

        /// <summary>
        ///     探测指定的字段是否已存在。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">待检测的字段。</param>
        /// <param name="lackOnes">返回缺少的字段。</param>
        /// <param name="shorterOnes">返回长度不足的字段。</param>
        void FieldExist(string tableName, Field[] fields, out Field[] lackOnes, out Field[] shorterOnes);

        /// <summary>
        ///     探测指定的索引是否已存在。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <param name="fields">索引字段的名称序列。</param>
        bool[] IndexExist(string tableName, string[] fields);

        /// <summary>
        ///     探测指定的表是否已存在。
        /// </summary>
        /// <param name="name">表名。</param>
        bool TableExist(string name);
    }
}