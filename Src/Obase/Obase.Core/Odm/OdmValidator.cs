/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：ODM验证器,用于验证ODM的合法性
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-9-20 16:38:11
└──────────────────────────────────────────────────────────────┘
*/

using System.Text;
using Obase.Core.Odm.Builder;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     ODM验证器,用于验证ODM的合法性
    /// </summary>
    public abstract class OdmValidator
    {
        /// <summary>
        ///     进行验证,返回验证结果
        /// </summary>
        /// <returns>验证结果 未通过时为完整性检查错误信息 通过时为完整模型信息</returns>
        public ValidationResult Validate()
        {
            //创建模型建造器
            var modelBuilder = new ModelBuilder(null);
            modelBuilder.HasIntegrityCheck(true);
            //创建模型配置
            CreateModel(modelBuilder);

            try
            {
                //开始创建模型
                var model = modelBuilder.Build();
                //使用模型查看器输出模型信息
                var message = ObjectDataModelViewer.GetFullObjectDataModelMappingView(model);

                return new ValidationResult { IsValid = true, Message = message.ToString() };
            }
            catch (IntegrityCheckFailException exception)
            {
                var messageBuilder = new StringBuilder();
                //遍历错误信息字典 生成错误信息
                foreach (var errMessage in exception.ErrorMessageDictionary)
                {
                    //类型名称和此类型下的错误个数
                    messageBuilder.Append($"类型{errMessage.Key}存在{errMessage.Value.Count}个完整性检查错误:").AppendLine();
                    //遍历此类型下的所有错误信息 依次输出
                    var seq = 1;
                    foreach (var message in errMessage.Value)
                        messageBuilder.Append($"{seq++}. {message}").AppendLine();
                    //类型之间空一行 便于查看
                    messageBuilder.AppendLine();
                }

                return new ValidationResult { IsValid = false, Message = messageBuilder.ToString() };
            }
        }

        /// <summary>
        ///     使用指定的建模器创建对象数据模型
        /// </summary>
        /// <param name="modelBuilder">对象数据模型建造器</param>
        protected abstract void CreateModel(ModelBuilder modelBuilder);

        /// <summary>
        ///     验证结果
        /// </summary>
        public class ValidationResult
        {
            /// <summary>
            ///     是否验证通过
            /// </summary>
            public bool IsValid { get; internal set; }

            /// <summary>
            ///     验证结果信息
            /// </summary>
            public string Message { get; internal set; }
        }
    }
}