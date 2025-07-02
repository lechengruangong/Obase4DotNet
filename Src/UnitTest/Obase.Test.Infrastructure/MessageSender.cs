using System.IO;
using Newtonsoft.Json;
using Obase.Core.MappingPipeline;

namespace Obase.Test.Infrastructure;

/// <summary>
///     模拟的消息发送器 保存于文件中
/// </summary>
public class MessageSender : IChangeNoticeSender
{
    /// <summary>
    ///     用于模拟消息队列的Txt文件路径
    /// </summary>
    private readonly string _path;

    /// <summary>
    ///     构造消息发送器
    /// </summary>
    public MessageSender()
    {
        _path = $"{Directory.GetCurrentDirectory()}\\TestQueue.txt";
    }

    /// <summary>
    ///     发送变更通知
    /// </summary>
    /// <param name="notice">变更通知</param>
    public void Send(ChangeNotice notice)
    {
        //用文件模拟的消息队列
        using var writer = new StreamWriter(new FileStream(_path, FileMode.OpenOrCreate));
        writer.WriteLine(JsonConvert.SerializeObject(notice));
        writer.Flush();
    }
}