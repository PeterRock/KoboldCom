using System;
using System.Collections.Generic;

namespace KoboldCom
{
    /// <summary>
    /// 串口通讯具体实现类
    /// </summary>
    public class Communicator
    {
        private readonly List<byte> _dataList = new List<byte>();
        private readonly List<RawDataReceivedHandler> _listRawDataReceivedHandler = new List<RawDataReceivedHandler>();

        /// <summary>
        /// 在串口接收到字节数据时候触发的事件
        /// </summary>
        public event RawDataReceivedHandler OnRawDataReceived;

        /// <summary>
        /// 在串口接收到字节数据时候触发的事件委托
        /// </summary>
        /// <param name="bytes"></param>
        public delegate void RawDataReceivedHandler(byte[] bytes);

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="com">串口配置</param>
        /// <param name="analyzers">消息解析方法集合</param>
        public Communicator(ICommunication com, IAnalyzerCollection analyzers)
        {
            Com = com ?? throw new NullReferenceException("no reference communication module was initialized");

            Analyzers = analyzers;
            Com.OnDataReceived += new DataReceivedHandler(ComDataReceived);
            ReadBufferSize = 0x800;
        }

        private void ComDataReceived(ICommunication icommunication)
        {
            int bytesToRead = icommunication.BytesToRead;
            var buffer = new byte[bytesToRead];
            icommunication.Read(buffer, 0, bytesToRead);
            if (_dataList.Count > ReadBufferSize)
            {
                _dataList.Clear();
            }
            _dataList.AddRange(buffer);
            if (OnRawDataReceived != null)
            {
                _listRawDataReceivedHandler.Clear();
                Delegate[] invocationList = OnRawDataReceived.GetInvocationList();
                int index = 0;
                while (true)
                {
                    if (index >= invocationList.Length)
                    {
                        break;
                    }
                    RawDataReceivedHandler item = (RawDataReceivedHandler)invocationList[index];
                    try
                    {
                        item(buffer);
                    }
                    catch (InvalidOperationException)
                    {
                        _listRawDataReceivedHandler.Add(item);
                    }
                    index++;
                }
                foreach (RawDataReceivedHandler handler2 in _listRawDataReceivedHandler)
                {
                    OnRawDataReceived = (RawDataReceivedHandler)Delegate.Remove(OnRawDataReceived, handler2);
                }
            }
            bool needContinue = true;
            while(needContinue)
            {
                foreach (IAnalyzer analyzer in Analyzers)
                {
                    analyzer.SearchBuffer(_dataList);
                    if (analyzer.Raw.Length > 0)
                    {
                        analyzer.Analyze();
                        analyzer.Raw = new byte[0];
                        needContinue = _dataList.Count > 0;
                    } else
                    {
                        needContinue = false;
                    }
                }
            }
        }

        /// <summary>
        /// 串口对象
        /// </summary>
        public ICommunication Com { get; set; }

        /// <summary>
        /// 设置或获取缓冲区大小
        /// </summary>
        public int ReadBufferSize { get; set; }

        /// <summary>
        /// 消息解析方法集合
        /// </summary>
        public IAnalyzerCollection Analyzers { get; set; }
    }
}
