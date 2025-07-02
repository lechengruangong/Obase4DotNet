/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的数组哈希生成器,提供生成哈希代码的默认方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:11:39
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Text;

namespace Obase.Core
{
    /// <summary>
    ///     默认的数组哈希生成器
    ///     提供生成哈希代码的默认方法。
    /// </summary>
    public class DefaultArrayHashGenerator : IArrayHashGenerator
    {
        /// <summary>
        ///     生成哈希代码。
        /// </summary>
        /// <param name="members">标识成员序列。</param>
        public int Generator(object[] members)
        {
            //基础值
            var outInt = 2;

            foreach (var item in members)
            {
                //先将基础类型转为byte处理
                byte[] itemByteArray;
                int itemOutInt;
                //模式匹配 每个都转为byte[]
                switch (item)
                {
                    case int newItem1:
                        itemByteArray = BitConverter.GetBytes(newItem1);
                        itemOutInt = MergeByteInt(itemByteArray);
                        outInt ^= itemOutInt;
                        break;
                    case string newItem2:
                        itemByteArray = Encoding.UTF8.GetBytes(newItem2);
                        itemOutInt = MergeByteInt(itemByteArray);
                        outInt ^= itemOutInt;
                        break;
                    case double newItem3:
                        itemByteArray = BitConverter.GetBytes(newItem3);
                        itemOutInt = MergeByteInt(itemByteArray);
                        outInt |= itemOutInt;
                        break;
                    case float newItem4:
                        itemByteArray = BitConverter.GetBytes(newItem4);
                        itemOutInt = MergeByteInt(itemByteArray);
                        outInt ^= itemOutInt;
                        break;
                    case bool newItem:
                        itemByteArray = BitConverter.GetBytes(newItem);
                        itemOutInt = MergeByteInt(itemByteArray);
                        outInt ^= itemOutInt;
                        break;
                    default:
                        itemOutInt = item.GetHashCode();
                        outInt ^= itemOutInt;
                        break;
                }
            }

            return outInt;
        }

        /// <summary>
        ///     将byte[]进行Hash操作
        /// </summary>
        /// <param name="itemByteArray">项转换出来的byte数组</param>
        /// <returns></returns>
        private static int MergeByteInt(byte[] itemByteArray)
        {
            var itemOutInt = 2;
            //用移位操作将byte[]转换为int
            for (var i = 0; i < itemByteArray.Length; i++)
                if (i != 0)
                {
                    // &&&&& -- > 00000000
                    if (itemByteArray[i] == 0)
                        itemOutInt = ~itemOutInt;
                    else
                        // &&&&& -- > 00000000  |||||||| --> 111111111
                        itemOutInt ^= itemByteArray[i];
                }
                else
                {
                    itemOutInt = itemByteArray[i];
                }

            return itemOutInt;
        }
    }
}