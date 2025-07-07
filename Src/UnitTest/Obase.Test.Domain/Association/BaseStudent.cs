namespace Obase.Test.Domain.Association
{
    /// <summary>
    ///     学生基类
    /// </summary>
    public abstract class BaseStudent
    {
        /// <summary>
        ///     学生名称
        /// </summary>
        private string _name;

        /// <summary>
        ///     学生id
        /// </summary>
        private long _studentId;

        /// <summary>
        ///     学生详细信息
        /// </summary>
        private StudentInfo _studentInfo;

        /// <summary>
        ///     学生id
        /// </summary>
        public long StudentId
        {
            get => _studentId;
            set => _studentId = value;
        }

        /// <summary>
        ///     详细信息
        /// </summary>
        /// <value></value>
        public virtual StudentInfo StudentInfo
        {
            get => _studentInfo;
            set => _studentInfo = value;
        }

        /// <summary>
        ///     学生名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }
    }
}
