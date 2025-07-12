/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：完整性检查未通过异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-12 11:26:23
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     完整性检查未通过异常
    /// </summary>
    public class IntegrityCheckFailException : Exception
    {
        /// <summary>
        ///     初始化完整性检查未通过异常
        /// </summary>
        /// <param name="errorMessageDictionary">完整性检查错误信息字典</param>
        public IntegrityCheckFailException(Dictionary<string, List<string>> errorMessageDictionary)
        {
            ErrorMessageDictionary = errorMessageDictionary;
        }

        /// <summary>
        ///     完整性检查错误信息字典
        ///     Key为类型名 键为此类型的模型类型中检查出的错误
        /// </summary>
        public Dictionary<string, List<string>> ErrorMessageDictionary { get; }

        /// <summary>
        ///     异常消息
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"完整性检查未通过,请参考{nameof(ErrorMessageDictionary)}内容修改模型配置或者关闭完整性检查.";
        }
    }
}