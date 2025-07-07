namespace Obase.Test.Domain.Functional.DataError
{
    /// <summary>
    ///     数据错误的学生信息测试类
    ///     用于测试引用是一对一但实际数据确是一对多的情况
    /// </summary>
    public class DataErrorStudentInfo
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
        ///     学生详细信息ID
        /// </summary>
        private long _studentInfoId;

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
        ///     学生详细信息ID
        /// </summary>
        public long StudentInfoId
        {
            get => _studentInfoId;
            set => _studentInfoId = value;
        }
    }
}