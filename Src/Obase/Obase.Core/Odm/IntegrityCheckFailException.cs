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
using System.Linq;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     完整性检查未通过异常
    /// </summary>
    public class IntegrityCheckFailException : Exception
    {
        /// <summary>
        ///     对象数据模型(ODM)完整性检查错误信息前缀
        /// </summary>
        public const string OdmMessagePrefix = "[ODM]";

        /// <summary>
        ///     序列化对象数据模型(SODM)完整性检查错误信息前缀
        /// </summary>
        public const string SodmMessagePrefix = "[SODM]";

        /// <summary>
        ///     初始化完整性检查未通过异常
        /// </summary>
        /// <param name="errorMessageDictionary">完整性检查错误信息字典</param>
        /// <param name="messagePrefix">
        ///     错误信息前缀,用于区分错误信息的来源(ODM或SODM),
        ///     不指定时直接使用原始的错误信息
        /// </param>
        public IntegrityCheckFailException(Dictionary<string, List<string>> errorMessageDictionary,
            string messagePrefix = null)
        {
            //不需要前缀 直接使用原始的错误信息字典
            if (string.IsNullOrEmpty(messagePrefix))
            {
                ErrorMessageDictionary = errorMessageDictionary;
            }
            //需要前缀 为每一条错误信息添加前缀 便于区分错误信息的来源
            else
            {
                ErrorMessageDictionary = errorMessageDictionary.ToDictionary(p => p.Key,
                    p => p.Value.Select(q => $"{messagePrefix}{q}").ToList());
            }
        }

        /// <summary>
        ///     完整性检查错误信息字典
        ///     Key为类型名 键为此类型的模型类型中检查出的错误
        /// </summary>
        public Dictionary<string, List<string>> ErrorMessageDictionary { get; }

        /// <summary>
        ///     返回异常消息
        /// </summary>
        public override string Message => ToString();

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