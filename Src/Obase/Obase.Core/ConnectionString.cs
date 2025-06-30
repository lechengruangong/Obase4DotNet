/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结构化表示的连接字符串.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:48:21
└──────────────────────────────────────────────────────────────┘
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Obase.Core
{
    /// <summary>
    ///     结构化表示的连接字符串。
    /// </summary>
    public class ConnectionString
    {
        /// <summary>
        ///     连接字符串的片段集。每个片段由一个健/值对构成。
        /// </summary>
        private readonly Dictionary<string, string> _keyWordDictionary = new Dictionary<string, string>();

        /// <summary>
        ///     原始字符串。
        /// </summary>
        private readonly string _originalString;

        /// <summary>
        ///     使用指定的连接字符串构造 ConnectionString 实例。
        /// </summary>
        /// <param name="connString">连接字符串。</param>
        public ConnectionString(string connString)
        {
            _originalString = connString;
            try
            {
                ResolveConnectionString(_originalString);
            }
            catch (Exception e)
            {
                throw new ArgumentException("无法解析字符串", e);
            }
        }

        /// <summary>
        ///     获取原始字符串。
        /// </summary>
        public string OriginalString => _originalString;

        /// <summary>
        ///     获取或设置服务器名称或地址。
        /// </summary>
        public string Server { get; private set; }

        /// <summary>
        ///     获取或设置服务器通讯端口。
        /// </summary>
        public int ServerPort { get; private set; } = -1;

        /// <summary>
        ///     获取或设置用户名。
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        ///     获取或设置密码。
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        ///     获取或设置连接超时时间。
        /// </summary>
        public int Timeout { get; private set; }

        /// <summary>
        ///     获取或设置指定名称的关键字的值。注：关键字名称不区分大小写。未取到则返回""空字符串
        /// </summary>
        public string this[string keyWord]
        {
            get
            {
                var getKey = keyWord.ToUpper();
                return _keyWordDictionary.TryGetValue(getKey, out var value) ? value : "";
            }
        }

        /// <summary>
        ///     处理连接字符串
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        private void ResolveConnectionString(string connectionString)
        {
            var explicitDef = true;
            var upperStr = connectionString.ToUpper();
            //是否显示指定了服务器关键字
            if (!upperStr.Contains("SERVER=") && !upperStr.Contains("DATA SOURCE=") && !upperStr.Contains("SERVER =")
                && !upperStr.Contains("DATASOURCE =") && !upperStr.Contains("DATA SOURCE ="))
                explicitDef = false;
            //显式指定 加入;用于切分
            if (explicitDef)
                //加入一个前导匹配符号
                connectionString = $";{connectionString}";

            //处理连接字符串
            var splitPairs = Regex.Split(connectionString, @"\;([\w \t\r\n]+)=", RegexOptions.Multiline);

            //如果没有显示指定 则首个必须为服务器地址
            if (!explicitDef)
            {
                var host = splitPairs[0];

                if (string.IsNullOrEmpty(host)) throw new ArgumentException("连接字符串未显式指定服务器关键字时必须以服务器地址作为首部");

                //尝试用,分割
                var spilts = host.Split(',');

                if (spilts.Length > 1)
                {
                    Server = spilts[0];
                    int.TryParse(spilts[1], out var result);
                    ServerPort = result;
                }
                else
                {
                    Server = spilts[0];
                }
            }

            //每两个一组
            for (var i = 1; i < splitPairs.Length; i += 2)
            {
                //统一使用大写做KEY 加入字典
                var tuple = new Tuple<string, string>(splitPairs[i].ToUpper(), splitPairs[i + 1]);
                _keyWordDictionary.Add(tuple.Item1, tuple.Item2.TrimEnd(';'));

                //处理常用的关键字
                switch (tuple.Item1)
                {
                    case "SERVER":
                    case "DATA SOURCE":
                    case "DATASOURCE":
                    {
                        var serverStr = tuple.Item2.Split(',');
                        if (serverStr.Length > 1)
                        {
                            int.TryParse(serverStr[1], out var result);
                            ServerPort = result;
                        }

                        Server = serverStr[0];
                        break;
                    }
                    //用户名
                    case "UID":
                    case "USERNAME":
                    {
                        UserName = tuple.Item2;
                        break;
                    }
                    //密码
                    case "PWD":
                    case "PASSWORD":
                    {
                        Password = tuple.Item2;
                        break;
                    }
                    //超时时间
                    case "CONNECT TIMEOUT":
                    case "TIMEOUT":
                    {
                        int.TryParse(tuple.Item2, out var result);
                        Timeout = result;
                        break;
                    }
                    case "PORT":
                    {
                        int.TryParse(tuple.Item2, out var result);
                        ServerPort = result;
                        break;
                    }
                }
            }

            //检查解析结果
            if (string.IsNullOrEmpty(Server)) throw new ArgumentException("连接字符串未能解析服务器地址.");
            if (ServerPort == -1) throw new ArgumentException("连接字符串未能解析服务器端口.");
            if (string.IsNullOrEmpty(UserName)) throw new ArgumentException("连接字符串未能解析用户名.");
            if (string.IsNullOrEmpty(Password)) throw new ArgumentException("连接字符串未能解析密码.");
        }

        /// <summary>
        ///     将 ConnectionString 实例表示成等效的文本表示形式。
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();
            //去除自定义的字段
            foreach (var keyword in _keyWordDictionary.Where(keyword =>
                         keyword.Key != "TARGETSOURCE" && keyword.Key != "TARGET SOURCE"))
                builder.Append($"{keyword.Key}={keyword.Value};");

            return builder.ToString();
        }

        /// <summary>
        ///     向连接字符串加入一个关键字和值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void AddKeyWord(string key, string value)
        {
            var getKey = key.ToUpper();
            _keyWordDictionary[getKey] = value;
        }

        /// <summary>
        ///     从连接字符串内移除一个关键字
        /// </summary>
        /// <param name="key">关键字</param>
        public void RemoveKeyWord(string key)
        {
            var getKey = key.ToUpper();
            if (_keyWordDictionary.ContainsKey(getKey))
                _keyWordDictionary.Remove(getKey);
        }
    }
}