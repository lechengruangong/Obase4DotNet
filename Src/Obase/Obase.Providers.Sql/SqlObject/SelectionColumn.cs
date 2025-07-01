/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示投影集中的一个列.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:32:41
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     表示投影集中的一个列。
    /// </summary>
    public abstract class SelectionColumn
    {
        /// <summary>
        ///     获取哈希码
        /// </summary>
        /// <returns></returns>
        public abstract override int GetHashCode();

        /// <summary>
        ///     确定指定的投影列是否与当前投影列相等。注：两个投影列相等的充要条件是表达式和别名均相等。
        /// </summary>
        /// <param name="other">要与当前投影列进行比较的投影列。</param>
        public abstract bool Equals(SelectionColumn other);

        /// <summary>
        ///     确定指定的对象与当前投影列是否相等。（重写Object.Equals）
        /// </summary>
        /// <param name="otherObj">要与当前投影列进行比较的对象。</param>
        public override bool Equals(object otherObj)
        {
            return Equals(otherObj as SelectionColumn);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public abstract string ToString(EDataSource sourceType);

        /// <summary>
        ///     相等比较运算符。
        /// </summary>
        /// <param name="column1">第一个操作数。</param>
        /// <param name="column2">第二个操作数。</param>
        public static bool operator ==(SelectionColumn column1, SelectionColumn column2)
        {
            if (ReferenceEquals(column2, column1))
                return true;
            if (Equals(column1, null))
                return false;
            return column1.Equals(column2);
        }

        /// <summary>
        ///     不相等比较运算符。
        /// </summary>
        /// <param name="column1">第一个操作数。</param>
        /// <param name="column2">第二个操作数。</param>
        public static bool operator !=(SelectionColumn column1, SelectionColumn column2)
        {
            return !(column1 == column2);
        }

        /// <summary>
        ///     为投影列涉及到的源的别名设置前缀。
        ///     注：只有简单源有别名，忽略非简单源。
        /// </summary>
        /// <param name="prefix">别名前缀。</param>
        public abstract void SetSourceAliasPrefix(string prefix);
    }
}