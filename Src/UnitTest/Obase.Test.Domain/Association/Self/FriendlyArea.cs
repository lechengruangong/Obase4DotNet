using System;

namespace Obase.Test.Domain.Association.Self
{
    /// <summary>
    ///     友好区域
    /// </summary>
    public class FriendlyArea
    {
        /// <summary>
        ///     区域
        /// </summary>
        private Area _area;

        /// <summary>
        ///     区域代码
        /// </summary>
        private string _areaCode;

        /// <summary>
        ///     友好区域
        /// </summary>
        private Area _friend;

        /// <summary>
        ///     友好区域代码
        /// </summary>
        private string _friendlyAreaCode;

        /// <summary>
        ///     友好关系开始时间
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        ///     区域代码
        /// </summary>
        public string AreaCode
        {
            get => _areaCode;
            set => _areaCode = value;
        }

        /// <summary>
        ///     区域
        /// </summary>
        public virtual Area Area
        {
            get => _area;
            set => _area = value;
        }

        /// <summary>
        ///     友好区域代码
        /// </summary>
        public string FriendlyAreaCode
        {
            get => _friendlyAreaCode;
            set => _friendlyAreaCode = value;
        }

        /// <summary>
        ///     友好区域
        /// </summary>
        public virtual Area Friend
        {
            get => _friend;
            set => _friend = value;
        }

        /// <summary>
        ///     友好关系开始时间
        /// </summary>
        public DateTime StartTime
        {
            get => _startTime;
            set => _startTime = value;
        }
    }
}