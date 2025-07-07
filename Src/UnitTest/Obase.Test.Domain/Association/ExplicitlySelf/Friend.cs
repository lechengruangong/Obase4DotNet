namespace Obase.Test.Domain.Association.ExplicitlySelf
{
    /// <summary>
    ///     宾客的朋友关系
    /// </summary>
    public class Friend
    {
        /// <summary>
        ///     朋友
        /// </summary>
        private Guest _friendGuest;

        /// <summary>
        ///     朋友的ID
        /// </summary>
        private int _friendId;

        /// <summary>
        ///     于哪个游戏里遇见的
        /// </summary>
        private string _meetIn;

        /// <summary>
        ///     自己
        /// </summary>
        private Guest _mySelf;

        /// <summary>
        ///     自己的ID
        /// </summary>
        private int _mySelfId;

        /// <summary>
        ///     自己
        /// </summary>
        public Guest MySelf
        {
            get => _mySelf;
            set => _mySelf = value;
        }


        /// <summary>
        ///     于哪个游戏里遇见的
        /// </summary>
        public string MeetIn
        {
            get => _meetIn;
            set => _meetIn = value;
        }

        /// <summary>
        ///     朋友
        /// </summary>
        public Guest FriendGuest
        {
            get => _friendGuest;
            set => _friendGuest = value;
        }

        /// <summary>
        ///     自己的ID
        /// </summary>
        public int MySelfId
        {
            get => _mySelfId;
            set => _mySelfId = value;
        }

        /// <summary>
        ///     朋友的ID
        /// </summary>
        public int FriendId
        {
            get => _friendId;
            set => _friendId = value;
        }
    }
}