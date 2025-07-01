/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：别名生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:14:31
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.ObjectSys;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     别名生成器，既可用于生成关联树节点的别名，也可用于生成该节点相关投影列的别名。
    /// </summary>
    public class AliasGenerator : AssociationTreeNodeAliasGenerator,
        IParameterizedAssociationTreeUpwardVisitor<string, string>
    {
        /// <summary>
        ///     获取或设置属性或标识成员的映射目标，基于其生成投影列。
        /// </summary>
        private string _fieldName;

        /// <summary>
        ///     属性或标识成员的映射目标，基于其生成投影列。
        /// </summary>
        public string FieldName
        {
            get => _fieldName;
            set => _fieldName = value;
        }

        /// <summary>
        ///     获取遍历关联树的结果。
        /// </summary>
        public new string Result
        {
            get
            {
                //获取基类结果
                var nodeAlias = base.Result;
                if (string.IsNullOrEmpty(_fieldName)) return nodeAlias;

                if (nodeAlias == null)
                    return null;
                return $"{nodeAlias}_{_fieldName}";
            }
        }


        /// <summary>
        ///     为即将开始的遍历操作设置参数。
        /// </summary>
        /// <param name="argument">参数值。</param>
        public void SetArgument(string argument)
        {
            _fieldName = argument;
        }

        /// <summary>
        ///     重置
        /// </summary>
        public override void Reset()
        {
            _fieldName = null;
        }
    }
}