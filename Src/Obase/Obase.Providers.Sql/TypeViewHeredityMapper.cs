/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为类型视图的映射源提供默认的遗传映射机制.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:19:57
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.ObjectSys;
using Obase.Core.Odm.TypeViews;
using Obase.Core.Saving;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     为类型视图的映射源提供默认的遗传映射机制。
    /// </summary>
    public class TypeViewHeredityMapper : IHeredityMapper
    {
        /// <summary>
        ///     别名生成器。
        /// </summary>
        private readonly AliasGenerator _aliasGenerator = new AliasGenerator();


        /// <summary>
        ///     作为联接依据的视图引用。
        /// </summary>
        public ViewReference JoinReference { get; set; }

        /// <summary>
        ///     根据字段在母源中名称推断其在衍生源中的名称。
        /// </summary>
        /// <param name="fieldName">字段在母源中的名称。</param>
        public string Map(string fieldName)
        {
            return Map(JoinReference, fieldName);
        }

        /// <summary>
        ///     推断其在衍生源中的名称
        /// </summary>
        /// <param name="joinRef">要Join的引用</param>
        /// <param name="fieldName">字段名称</param>
        /// <returns></returns>
        private string Map(ViewReference joinRef, string fieldName)
        {
            //获取锚点
            var anchor = joinRef.Anchor;
            if (anchor is TypeViewNode)
            {
                //获取视图绑定
                var binding = joinRef.Binding;
                var reference = binding as ViewReference;
                return Map(reference, fieldName);
            }

            //转换为关联树
            var tree = anchor.AsTree();
            return tree.Accept(_aliasGenerator, fieldName) ?? fieldName;
        }
    }
}