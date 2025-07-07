using System.Collections.Generic;

namespace Obase.Test.Domain.Association.ExplicitlySelf
{
    /// <summary>
    ///     宾客
    /// </summary>
    public class Guest
    {
        /// <summary>
        ///     朋友是我的人
        /// </summary>
        private List<Friend> _friendOfmes;

        /// <summary>
        ///     我的朋友
        /// </summary>
        private List<Friend> _friends;

        /// <summary>
        ///     宾客ID
        /// </summary>
        private int _guestId;

        /// <summary>
        ///     宾客姓名
        /// </summary>
        private string _name;

        /// <summary>
        ///     我的朋友
        /// </summary>
        public List<Friend> MyFriends
        {
            get => _friends;
            set => _friends = value;
        }

        /// <summary>
        ///     宾客ID
        /// </summary>
        public int GuestId
        {
            get => _guestId;
            set => _guestId = value;
        }

        /// <summary>
        ///     宾客姓名
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     朋友是我的人
        /// </summary>
        public List<Friend> FriendOfmes
        {
            get => _friendOfmes;
            set => _friendOfmes = value;
        }
    }
}