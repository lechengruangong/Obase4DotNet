/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：归并排序执行器,提供基于文件的归并排序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:41:52
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Obase.Core.Collections
{
    /// <summary>
    ///     归并排序执行器 基于文件的归并排序
    /// </summary>
    public class MergeSortExecutor<TItem>
    {
        /// <summary>
        ///     每块
        /// </summary>
        private readonly List<TItem> _blockItems;

        /// <summary>
        ///     每块的大小 当读入数据超过此值时 写至文件
        /// </summary>
        private readonly int _blockSize;

        /// <summary>
        ///     元素的比较器
        /// </summary>
        private readonly IComparer<TItem> _comparer;

        /// <summary>
        ///     元素的比较委托
        /// </summary>
        private readonly Comparison<TItem> _comparison;

        /// <summary>
        ///     二进制序列化器
        /// </summary>
        private readonly IFormatter _formatter = new BinaryFormatter();

        /// <summary>
        ///     是否为倒序
        /// </summary>
        private readonly bool _isDesc;

        /// <summary>
        ///     每块的对象表示
        /// </summary>
        private readonly List<MergeSortFileBlock<TItem>> _mergeSortFileBlocks;

        /// <summary>
        ///     排序结果文件路径
        /// </summary>
        private readonly string _resultFilePath;

        /// <summary>
        ///     每个元素在块文件内的位置
        /// </summary>
        private readonly List<long> _resultItemPosition;

        /// <summary>
        ///     存储块元素的临时文件名称
        /// </summary>
        private readonly string _tempFilePath;

        /// <summary>
        ///     共有多少块
        /// </summary>
        private int _blockCount;

        /// <summary>
        ///     当前元素起始位置索引 未读取前为-1 完全读取后为元素个数
        /// </summary>
        private int _currentPosition = -1;

        /// <summary>
        ///     是否为初次读取
        /// </summary>
        private bool _isFirstRead = true;

        /// <summary>
        ///     构造一个归并排序执行器 并指定每块大小
        /// </summary>
        /// <param name="isDesc">是否倒排</param>
        /// <param name="blockSize">每个块的大小 默认:100000</param>
        private MergeSortExecutor(bool isDesc, int blockSize)
        {
            _isDesc = isDesc;
            _blockSize = blockSize;
            _blockItems = new List<TItem>(blockSize);
            //排序用临时文件路径
            var fileName = $"{Guid.NewGuid().ToString().Replace("-", "")}";
            _tempFilePath = $"{Directory.GetCurrentDirectory()}\\MergeSortTempFile\\{fileName}";
            //临时文件所对应的块
            _mergeSortFileBlocks = new List<MergeSortFileBlock<TItem>>();
            //排序结果文件路径
            _resultFilePath = $"{Directory.GetCurrentDirectory()}\\{fileName}";
            _resultItemPosition = new List<long>();
            //创建临时文件夹
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\MergeSortTempFile"))
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\MergeSortTempFile");
        }

        /// <summary>
        ///     构造一个归并排序执行器 并指定比较器 每块大小 是否为倒序排序
        /// </summary>
        /// <param name="comparer">比较器</param>
        /// <param name="isDesc">是否为倒序排序</param>
        /// <param name="blockSize">每块大小</param>
        public MergeSortExecutor(IComparer<TItem> comparer, bool isDesc = false, int blockSize = 100000) : this(isDesc,
            blockSize)
        {
            _comparer = comparer;
        }

        /// <summary>
        ///     构造一个归并排序执行器 并指定比较委托 每块大小 是否为倒序排序
        /// </summary>
        /// <param name="comparison">比较委托</param>
        /// <param name="isDesc">是否为倒序排序</param>
        /// <param name="blockSize">每块大小</param>
        public MergeSortExecutor(Comparison<TItem> comparison, bool isDesc = false, int blockSize = 100000) : this(
            isDesc,
            blockSize)
        {
            _comparison = comparison;
        }

        /// <summary>
        ///     共有多少块
        /// </summary>
        public int BlockCount => _blockCount;

        /// <summary>
        ///     将元素放入排序器 当放入的元素存满一块时 将存入文件
        /// </summary>
        /// <param name="item">元素</param>
        public void PutIn(TItem item)
        {
            //当前块是否还有余量
            if (_blockItems.Count < _blockSize)
            {
                _blockItems.Add(item);
            }
            else
            {
                SaveBlock();
                _blockItems.Add(item);
            }
        }

        /// <summary>
        ///     结束放入 并触发排序
        ///     <para>注意:如在放入时不正确指示结束放入 可能会丢失元素</para>
        /// </summary>
        public void EndPutIn()
        {
            //是否有剩余的未保存至外存的数据
            if (_blockItems.Count > 0) SaveBlock();

            Sort();
        }

        /// <summary>
        ///     保存某块的数据至外存
        /// </summary>
        private void SaveBlock()
        {
            //每块排序
            if (_comparison != null)
                _blockItems.Sort(_comparison);
            else if (_comparer != null)
                _blockItems.Sort(_comparer);
            else
                _blockItems.Sort(Comparer<TItem>.Default);


            //每个元素在块文件内的位置
            var itemPosition = new List<long>();

            //存储至每块文件
            var path = $"{_tempFilePath}{_blockCount}";
            WriteInFile(_blockItems, path, itemPosition);

            //构造成文件块
            _mergeSortFileBlocks.Add(new MergeSortFileBlock<TItem>(_blockCount, $"{_tempFilePath}{_blockCount}",
                itemPosition));

            //块数增加
            _blockCount++;
            //清除此块内容
            _blockItems.Clear();
        }

        /// <summary>
        ///     对读入的结果进行排序
        /// </summary>
        private void Sort()
        {
            //最小堆
            MinHeap<MergeSortFileBlock<TItem>.BlockItem> minHeap;
            if (_comparison != null)
                minHeap = new MinHeap<MergeSortFileBlock<TItem>.BlockItem>((x, y) => _comparison.Invoke(x.Item, y.Item),
                    _mergeSortFileBlocks.Count);
            else if (_comparer != null)
                minHeap = new MinHeap<MergeSortFileBlock<TItem>.BlockItem>(
                    new MergeSortFileBlock<TItem>.BlockItemComparer(_comparer), _mergeSortFileBlocks.Count);
            else
                minHeap = new MinHeap<MergeSortFileBlock<TItem>.BlockItem>(
                    new MergeSortFileBlock<TItem>.BlockItemComparer(Comparer<TItem>.Default),
                    _mergeSortFileBlocks.Count);
            //读取每个里面最小的元素
            foreach (var fileBlock in _mergeSortFileBlocks)
            {
                var blockItem = fileBlock.Read(out var isSuccess);
                if (isSuccess) minHeap.Enqueue(blockItem);
            }

            //临时存储用
            var tempList = new List<TItem>();
            //读到全部读完
            while (!_mergeSortFileBlocks.All(p => p.IsEnded))
            {
                //弹出 放入临时存储
                var min = minHeap.Dequeue();
                tempList.Add(min.Item);
                //放入对应的新元素
                var dequeueFrom = _mergeSortFileBlocks.First(p => p.Sequence == min.Sequence);
                //从序号相同的块内读取
                var blockItem = dequeueFrom.Read(out var isSuccess);
                if (isSuccess) minHeap.Enqueue(blockItem);
                //如果读满临时存储的数量 则写入硬盘
                if (tempList.Count >= 5000)
                {
                    WriteInFile(tempList, _resultFilePath, _resultItemPosition);
                    //存完这一批 清空
                    tempList.Clear();
                }
            }

            //补上最后一组
            if (tempList.Count > 0) WriteInFile(tempList, _resultFilePath, _resultItemPosition);

            //清除临时文件
            DeleteFileByDirectory(new DirectoryInfo($"{Directory.GetCurrentDirectory()}\\MergeSortTempFile"));
        }

        /// <summary>
        ///     将某一集合写入文件
        /// </summary>
        /// <param name="tempList">可枚举集合</param>
        /// <param name="path">路径</param>
        /// <param name="itemPosition">元素位置集合</param>
        private void WriteInFile(List<TItem> tempList, string path, List<long> itemPosition)
        {
            //存储至文件
            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(path, FileMode.Append))
                {
                    //循环元素
                    foreach (var tempItem in tempList)
                    {
                        //记录起始位置
                        itemPosition.Add(fileStream.Length);
                        //序列化
                        _formatter.Serialize(memoryStream, tempItem);
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
        ///     取出结果
        /// </summary>
        /// <param name="isSucess">是否成功</param>
        /// <returns></returns>
        public TItem TakeOut(out bool isSucess)
        {
            //没有任何元素记录
            if (_resultItemPosition.Count <= 0) throw new InvalidOperationException("未执行排序,无可用结果,请检查放入时是否指定结束元素.");

            //初次读取 重置读取器
            if (_isFirstRead)
            {
                Reset();
                _isFirstRead = false;
            }

            //正序还是倒序读取
            return !_isDesc ? Read(out isSucess) : ReverselyRead(out isSucess);
        }

        /// <summary>
        ///     重置读取器
        /// </summary>
        private void Reset()
        {
            if (!_isDesc)
                _currentPosition = -1;
            else
                _currentPosition = _resultItemPosition.Count;
        }

        /// <summary>
        ///     正序读取一个元素
        /// </summary>
        /// <returns></returns>
        private TItem Read(out bool isSucess)
        {
            //没有任何元素记录
            if (_resultItemPosition.Count <= 0)
            {
                isSucess = false;
                return default;
            }

            //真正的长度
            var realReadCount = 1;
            if (_currentPosition + 1 > _resultItemPosition.Count - 1)
                realReadCount = _resultItemPosition.Count - 1 - _currentPosition;
            //没有元素 返回空
            if (realReadCount <= 0)
            {
                //已无元素可读 游标推进至总个数之后
                if (_currentPosition == _resultItemPosition.Count - 1) _currentPosition++;
                isSucess = false;
                return default;
            }

            var result = default(TItem);

            //从文件中读取
            using (var fileStream = new FileStream(_resultFilePath, FileMode.Open))
            {
                //先寻址到当前元素位置
                if (_currentPosition > -1) fileStream.Seek(_resultItemPosition[_currentPosition + 1], SeekOrigin.Begin);
                for (var i = 0; i < realReadCount; i++)
                {
                    //对象长度
                    long itemLength;
                    if (_currentPosition + 2 < _resultItemPosition.Count)
                    {
                        if (_currentPosition == -1)
                            itemLength = _resultItemPosition[_currentPosition + 2];
                        else
                            itemLength = _resultItemPosition[_currentPosition + 2] -
                                         _resultItemPosition[_currentPosition + 1];
                    }
                    else
                    {
                        itemLength = fileStream.Length - _resultItemPosition[_currentPosition + 1];
                    }

                    var itemBytes = new byte[itemLength];
                    //读取
                    var readed = fileStream.Read(itemBytes, 0, itemBytes.Length);
                    //反序列化
                    using (var memoryStream = new MemoryStream(itemBytes, 0, readed))
                    {
                        result = (TItem)_formatter.Deserialize(memoryStream);
                    }

                    //当前元素++
                    _currentPosition++;
                }
            }

            isSucess = true;
            return result;
        }

        /// <summary>
        ///     倒序读取一个元素
        /// </summary>
        /// <param name="isSucess"></param>
        /// <returns></returns>
        private TItem ReverselyRead(out bool isSucess)
        {
            //没有任何元素记录
            if (_resultItemPosition.Count <= 0)
            {
                isSucess = false;
                return default;
            }

            //真正的长度
            var realReadCount = 1;
            if (_currentPosition + 1 - 1 <= 0) realReadCount = _currentPosition;
            //没有元素 返回空
            if (realReadCount <= 0)
            {
                //已无元素可读 游标移动到-1
                if (_currentPosition == 0) _currentPosition--;
                isSucess = false;
                return default;
            }

            var result = default(TItem);

            //从文件中读取
            using (var fileStream = new FileStream(_resultFilePath, FileMode.Open))
            {
                for (var i = 0; i < realReadCount; i++)
                {
                    //对象长度
                    long itemLength;
                    if (_currentPosition - 1 >= 0)
                    {
                        //先寻址到当前元素位置
                        fileStream.Seek(_resultItemPosition[_currentPosition - 1], SeekOrigin.Begin);
                        if (_currentPosition > _resultItemPosition.Count - 1)
                            //求出元素长度
                            itemLength = fileStream.Length - _resultItemPosition[_currentPosition - 1];
                        else
                            //求出元素长度
                            itemLength = _resultItemPosition[_currentPosition] -
                                         _resultItemPosition[_currentPosition - 1];
                    }
                    else
                    {
                        itemLength = _resultItemPosition[_currentPosition + 1];
                    }

                    var itemBytes = new byte[itemLength];
                    //读取
                    var readed = fileStream.Read(itemBytes, 0, itemBytes.Length);
                    //反序列化
                    using (var memoryStream = new MemoryStream(itemBytes, 0, readed))
                    {
                        result = (TItem)_formatter.Deserialize(memoryStream);
                    }

                    //当前元素--
                    _currentPosition--;
                }
            }

            isSucess = true;
            return result;
        }

        /// <summary>
        ///     递归删文件夹和文件
        /// </summary>
        /// <param name="info">根目录</param>
        private static void DeleteFileByDirectory(DirectoryInfo info)
        {
            foreach (var newInfo in info.GetDirectories()) DeleteFileByDirectory(newInfo);
            foreach (var newInfo in info.GetFiles())
            {
                newInfo.Attributes = newInfo.Attributes &
                                     ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                newInfo.Delete();
            }

            info.Attributes = info.Attributes &
                              ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
            info.Delete();
        }


        /// <summary>
        ///     表示一个归并排序用到的文件块
        /// </summary>
        private class MergeSortFileBlock<T>
        {
            /// <summary>
            ///     文件路径
            /// </summary>
            private readonly string _filePath;

            /// <summary>
            ///     二进制序列化器
            /// </summary>
            private readonly IFormatter _formatter = new BinaryFormatter();

            /// <summary>
            ///     元素起始位置集合
            /// </summary>
            private readonly List<long> _itemPositions;

            /// <summary>
            ///     块序号
            /// </summary>
            private readonly int _sequence;

            /// <summary>
            ///     当前元素起始位置索引 未读取前为-1 完全读取后为元素个数
            /// </summary>
            private int _currentPosition = -1;

            /// <summary>
            ///     是否已读完
            /// </summary>
            private bool _isEnded;

            /// <summary>
            ///     构造一个归并排序用到的文件块
            /// </summary>
            /// <param name="sequence">序号</param>
            /// <param name="filePath">文件路径</param>
            /// <param name="itemPositions">每个元素在块内的位置</param>
            public MergeSortFileBlock(int sequence, string filePath, List<long> itemPositions)
            {
                _sequence = sequence;
                _filePath = filePath;
                _itemPositions = itemPositions;
            }

            /// <summary>
            ///     块序号
            /// </summary>
            public int Sequence => _sequence;

            /// <summary>
            ///     是否已读完
            /// </summary>
            public bool IsEnded => _isEnded;

            /// <summary>
            ///     从文件块内读取一个块元素
            /// </summary>
            /// <param name="isSuccess">是否成功</param>
            /// <returns></returns>
            public BlockItem Read(out bool isSuccess)
            {
                //没有任何元素记录
                if (_itemPositions.Count <= 0)
                {
                    isSuccess = false;
                    return default;
                }

                //真正的长度
                var realReadCount = 1;
                if (_currentPosition + 1 > _itemPositions.Count - 1)
                    realReadCount = _itemPositions.Count - 1 - _currentPosition;
                //没有元素 返回空
                if (realReadCount <= 0)
                {
                    //已无元素可读 游标推进至总个数之后
                    if (_currentPosition == _itemPositions.Count - 1) _currentPosition++;
                    isSuccess = false;
                    _isEnded = true;
                    return default;
                }

                using (var fileStream = new FileStream(_filePath, FileMode.Open))
                {
                    //先寻址到当前元素位置
                    if (_currentPosition > -1) fileStream.Seek(_itemPositions[_currentPosition + 1], SeekOrigin.Begin);
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
                    //当前元素++
                    _currentPosition++;

                    //反序列化
                    using (var memoryStream = new MemoryStream(itemBytes, 0, readed))
                    {
                        isSuccess = true;
                        return new BlockItem((T)_formatter.Deserialize(memoryStream), _sequence);
                    }
                }
            }

            /// <summary>
            ///     表示一个块内元素 包括所属块的序号和元素
            /// </summary>
            public struct BlockItem
            {
                /// <summary>
                ///     所属的块的序号
                /// </summary>
                public int Sequence { get; }

                /// <summary>
                ///     块内元素
                /// </summary>
                public T Item { get; }

                /// <summary>
                ///     构造一个块内元素
                /// </summary>
                /// <param name="item">元素</param>
                /// <param name="sequence">所属的块的序号</param>
                public BlockItem(T item, int sequence)
                {
                    Sequence = sequence;
                    Item = item;
                }
            }

            /// <summary>
            ///     块元素比较器
            /// </summary>
            public class BlockItemComparer : IComparer<BlockItem>
            {
                /// <summary>
                ///     元素比较器
                /// </summary>
                private readonly IComparer<T> _comparer;

                /// <summary>
                ///     构造一个块元素比较器
                /// </summary>
                /// <param name="comparer">元素比较器</param>
                public BlockItemComparer(IComparer<T> comparer)
                {
                    _comparer = comparer;
                }

                /// <summary>
                ///     比较块内元素
                /// </summary>
                /// <param name="x"></param>
                /// <param name="y"></param>
                /// <returns></returns>
                public int Compare(BlockItem x, BlockItem y)
                {
                    return _comparer.Compare(x.Item, y.Item);
                }
            }
        }
    }
}