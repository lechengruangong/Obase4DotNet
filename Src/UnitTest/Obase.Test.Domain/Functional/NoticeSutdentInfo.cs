namespace Obase.Test.Domain.Functional
{
    /// <summary>
    ///     用于通知的学生信息
    /// </summary>
    public class NoticeSutdentInfo
    {
        /// <summary>
        ///     学生背景
        /// </summary>
        private string _background;

        /// <summary>
        ///     学生详细描述
        /// </summary>
        private string _description;

        /// <summary>
        ///     学生id
        /// </summary>
        private long _studentId;

        /// <summary>
        ///     学生id
        /// </summary>
        public long StudentId
        {
            get => _studentId;
            set => _studentId = value;
        }

        /// <summary>
        ///     学生详细描述
        /// </summary>
        public string Description
        {
            get => _description;
            set => _description = value;
        }

        /// <summary>
        ///     学生背景
        /// </summary>
        public string Background
        {
            get => _background;
            set => _background = value;
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"StudentInfo:{{StudentId-{_studentId},Description-\"{_description}\",Background-\"{_background}\"}}";
        }
    }
}