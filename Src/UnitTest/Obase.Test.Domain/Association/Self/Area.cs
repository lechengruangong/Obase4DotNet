using System.Collections.Generic;

namespace Obase.Test.Domain.Association.Self
{
    /// <summary>
    ///     表示一个区域
    /// </summary>
    public class Area
    {
        /// <summary>
        ///     区域代码
        /// </summary>
        private string _code;

        /// <summary>
        ///     友好区域
        /// </summary>
        private List<FriendlyArea> _friendlyAreas;

        /// <summary>
        ///     名字
        /// </summary>
        private string _name;

        /// <summary>
        ///     父级区域
        /// </summary>
        private Area _parentArea;

        /// <summary>
        ///     父级区域代码
        /// </summary>
        private string _parentCode;

        /// <summary>
        ///     子区域
        /// </summary>
        private List<Area> _subAreas;

        /// <summary>
        ///     区域代码
        /// </summary>
        public string Code
        {
            get => _code;
            set => _code = value;
        }

        /// <summary>
        ///     父级区域代码
        /// </summary>
        public string ParentCode
        {
            get => _parentCode;
            set => _parentCode = value;
        }

        /// <summary>
        ///     父级区域
        /// </summary>
        public virtual Area ParentArea
        {
            get => _parentArea;
            set => _parentArea = value;
        }

        /// <summary>
        ///     子区域
        /// </summary>
        public virtual List<Area> SubAreas
        {
            get => _subAreas;
            set => _subAreas = value;
        }

        /// <summary>
        ///     友好区域
        /// </summary>
        public virtual List<FriendlyArea> FriendlyAreas
        {
            get => _friendlyAreas;
            set => _friendlyAreas = value;
        }

        /// <summary>
        ///     名字
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }
    }
}