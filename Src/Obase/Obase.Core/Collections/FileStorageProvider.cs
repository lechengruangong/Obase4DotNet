/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：基于文件的后备存储区的提供程序,提供使用文件的后备存储.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:20:59
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     基于文件的后备存储区的提供程序
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class FileStorageProvider<T> : IBackupStorageProvider<T>
    {
        /// <summary>
        ///     用于存储的文件路径
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        ///     byte[]序列化程序
        /// </summary>
        private readonly BinaryFormatter _formatter;

        /// <summary>
        ///     元素起始位置集合
        /// </summary>
        private readonly List<long> _itemPositions;

        /// <summary>
        ///     当前元素起始位置索引 未读取前为-1 完全读取后为元素个数
        /// </summary>
        private int _currentPosition = -1;

        /// <summary>
        ///     构造一个基于文件的后备存储提供程序
        /// </summary>
        /// <param name="filePath">文件路径 传入空字符串则默认为当前运行文件夹下Obase/BackStorage/{<see cref="Guid" />.NewGuid()}.storage</param>
        public FileStorageProvider(string filePath = "")
        {
            //无文件路径 构造文件路径
            if (string.IsNullOrEmpty(filePath))
            {
                var fileName = $"{Guid.NewGuid().ToString().Replace("-", "")}.storage";
                filePath = $"{Directory.GetCurrentDirectory()}\\{fileName}";
            }

            //文件路径
            _filePath = filePath;
            if (File.Exists(filePath)) File.Delete(filePath);

            //序列化程序
            _formatter = new BinaryFormatter();
            //元素起始位置集合
            _itemPositions = new List<long>();
        }

        /// <summary>
        ///     从后备存储区当前位置读取指定个数的元素。
        /// </summary>
        /// <returns>读取到的元素的集合，未读取到任何元素返回null。当后备存储区中当前位置之后的元素数少于请求数时，实际读取到的元素数会小于请求数。</returns>
        /// <param name="count">要读取的元素个数。</param>
        public T[] Read(int count)
        {
            //读取0个
            if (count == 0) return null;

            //没有任何元素记录
            if (_itemPositions.Count <= 0) return null;

            //真正的长度
            var realReadCount = count;
            if (_currentPosition + count > _itemPositions.Count - 1)
                realReadCount = _itemPositions.Count - 1 - _currentPosition;
            //没有元素 返回空
            if (realReadCount <= 0)
            {
                //已无元素可读 游标推进至总个数之后
                if (_currentPosition == _itemPositions.Count - 1) _currentPosition++;
                return null;
            }

            var result = new List<T>();

            //从文件中读取
            using (var fileStream = new FileStream(_filePath, FileMode.Open))
            {
                //先寻址到当前元素位置
                if (_currentPosition > -1) fileStream.Seek(_itemPositions[_currentPosition + 1], SeekOrigin.Begin);
                for (var i = 0; i < realReadCount; i++)
                {
                    //对象长度
                    long itemLength;
                    if (_currentPosition + 2 < _itemPositions.Count)
                    {
                        if (_currentPosition == -1)
                            itemLength = _itemPositions[_currentPosition + 2];
                        else
                            itemLength = _itemPositions[_currentPosition + 2] - _itemPositions[_currentPosition + 1];
                    }
                    else
                    {
                        itemLength = fileStream.Length - _itemPositions[_currentPosition + 1];
                    }

                    var itemBytes = new byte[itemLength];
                    //读取
                    var readed = fileStream.Read(itemBytes, 0, itemBytes.Length);
                    //反序列化
                    using (var memoryStream = new MemoryStream(itemBytes, 0, readed))
                    {
                        result.Add((T)_formatter.Deserialize(memoryStream));
                    }

                    //当前元素++
                    _currentPosition++;
                }
            }


            return result.ToArray();
        }

        /// <summary>
        ///     从后备存储区当前位置反向读取（从后往前）指定个数的元素。
        /// </summary>
        /// <returns>
        ///     读取到的元素的集合，未读取到任何元素返回null。当后备存储区中当前位置之前的元素数少于请求数时，实际读取到的元素数会小于请求数。
        ///     实施建议：
        ///     在追加元素时顺带记录下每个元素在文件流中的起始位置，反序读取利用这些位置信息在流中移动读取。
        /// </returns>
        /// <param name="count">要读取的元素个数。</param>
        public T[] ReverselyRead(int count)
        {
            //读取0个
            if (count == 0) return null;

            //没有任何元素记录
            if (_itemPositions.Count <= 0) return null;

            //真正的长度
            var realReadCount = count;
            if (_currentPosition + 1 - count <= 0) realReadCount = _currentPosition;
            //没有元素 返回空
            if (realReadCount <= 0)
            {
                //已无元素可读 游标移动到-1
                if (_currentPosition == 0) _currentPosition--;
                return null;
            }

            var result = new List<T>();

            //从文件中读取
            using (var fileStream = new FileStream(_filePath, FileMode.Open))
            {
                for (var i = 0; i < realReadCount; i++)
                {
                    //对象长度
                    long itemLength;
                    if (_currentPosition - 1 >= 0)
                    {
                        //先寻址到当前元素位置
                        fileStream.Seek(_itemPositions[_currentPosition - 1], SeekOrigin.Begin);
                        if (_currentPosition > _itemPositions.Count - 1)
                            //求出元素长度
                            itemLength = fileStream.Length - _itemPositions[_currentPosition - 1];
                        else
                            //求出元素长度
                            itemLength = _itemPositions[_currentPosition] - _itemPositions[_currentPosition - 1];
                    }
                    else
                    {
                        itemLength = _itemPositions[_currentPosition + 1];
                    }

                    var itemBytes = new byte[itemLength];
                    //读取
                    var readed = fileStream.Read(itemBytes, 0, itemBytes.Length);
                    //反序列化
                    using (var memoryStream = new MemoryStream(itemBytes, 0, readed))
                    {
                        result.Add((T)_formatter.Deserialize(memoryStream));
                    }

                    //当前元素--
                    _currentPosition--;
                }
            }


            return result.ToArray();
        }

        /// <summary>
        ///     检测后备存储区中是否存在指定的元素。
        /// </summary>
        /// <returns>如果存在返回true，否则返回false。</returns>
        /// 实施建议：
        /// 在追加元素时顺带记录下每个元素在文件流中的起始位置，执行包含检测时顺次定位到各元素起始点，然后逐字节比对，发现差异后跳跃到下一元素起始点再次比对。
        /// <param name="item">元素</param>
        public bool Contains(T item)
        {
            using (var memoryStream = new MemoryStream())
            {
                //序列化当前元素
                _formatter.Serialize(memoryStream, item);
                var targetItemBytes = memoryStream.ToArray();
                //读取 比较
                using (var fileStream = new FileStream(_filePath, FileMode.Open))
                {
                    //循环元素
                    for (var i = 0; i < _itemPositions.Count; i++)
                    {
                        //对象长度
                        long itemLength;
                        if (i + 1 < _itemPositions.Count)
                            itemLength = _itemPositions[i + 1] - _itemPositions[i];
                        else
                            itemLength = fileStream.Length - _itemPositions[i];
                        var itemBytes = new byte[itemLength];
                        //读取
                        var readed = fileStream.Read(itemBytes, 0, itemBytes.Length);
                        //确保只比较实际读取的字节
                        itemBytes = itemBytes.Take(readed).ToArray();
                        //挨个比较
                        if (itemBytes.SequenceEqual(targetItemBytes)) return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     向后备存储区添加元素。
        /// </summary>
        /// <param name="item">元素</param>
        public void Append(IEnumerable<T> item)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(_filePath, FileMode.Append))
                {
                    //循环元素
                    foreach (var i in item)
                    {
                        //记录起始位置
                        _itemPositions.Add(fileStream.Length);
                        //序列化
                        _formatter.Serialize(memoryStream, i);
                        var iBytes = memoryStream.ToArray();
                        //写入文件
                        fileStream.Write(iBytes, 0, iBytes.Length);
                        //重置内存流
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.SetLength(0);
                    }
                }
            }
        }


        /// <summary>
        ///     将后备存储区中的位置移动到存储区开始处，即第一个元素之前。
        /// </summary>
        public void Reset()
        {
            _currentPosition = -1;
        }

        /// <summary>
        ///     将后备存储区中的位置移动到存储区末尾，即最后一个元素之后。
        /// </summary>
        public void ReverselyReset()
        {
            _currentPosition = _itemPositions.Count;
        }
    }
}