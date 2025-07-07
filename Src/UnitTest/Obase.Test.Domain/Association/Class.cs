using System.Collections.Generic;
using System.Linq;

namespace Obase.Test.Domain.Association
{
    /// <summary>
    ///     班级
    /// </summary>
    public class Class
    {
        /// <summary>
        ///     班级id
        /// </summary>
        private long _classId;

        /// <summary>
        ///     班级任课老师
        /// </summary>
        private List<ClassTeacher> _classTeachers;

        /// <summary>
        ///     班级名称
        /// </summary>
        private string _name;

        /// <summary>
        ///     学校
        /// </summary>
        private School _school;

        /// <summary>
        ///     学校ID
        /// </summary>
        private long _schoolId;

        /// <summary>
        ///     学生
        /// </summary>
        private List<Student> _students;

        /// <summary>
        ///     班级id
        /// </summary>
        public long ClassId
        {
            get => _classId;
            set => _classId = value;
        }

        /// <summary>
        ///     班级名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     学校ID
        /// </summary>
        public long SchoolId
        {
            get => _schoolId;
            set => _schoolId = value;
        }

        /// <summary>
        ///     学校
        /// </summary>
        public virtual School School
        {
            get => _school;
            set => _school = value;
        }

        /// <summary>
        ///     学生
        /// </summary>
        public virtual List<Student> Students => _students;

        /// <summary>
        ///     任课教师
        /// </summary>
        public virtual List<ClassTeacher> ClassTeachers
        {
            get => _classTeachers;
            set => _classTeachers = value;
        }

        public virtual List<Teacher> Teachers => ClassTeachers?.Select(p => p.Teacher).ToList();

        /// <summary>
        ///     设置任课老师
        /// </summary>
        /// <param name="classTeacher"></param>
        public void SetTeacher(ClassTeacher classTeacher)
        {
            if (_classTeachers == null) _classTeachers = new List<ClassTeacher>();
            _classTeachers.Add(classTeacher);
        }

        /// <summary>
        ///     设置学生
        /// </summary>
        /// <param name="student"></param>
        public void SetStudent(Student student)
        {
            if (_students == null) _students = new List<Student>();
            _students.Add(student);
        }

        /// <summary>
        ///     转换为字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Class:{{ClassId-{_classId},Name-\"{_name}\",SchoolId-{_schoolId}}}";
        }
    }
}