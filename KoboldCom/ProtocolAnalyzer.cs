using System;
using System.Collections.Generic;
using System.Threading;

namespace KoboldCom
{
    /// <summary>
    /// 数据解析抽象类
    /// </summary>
    /// <typeparam name="T">解析结果类</typeparam>
    public abstract class ProtocolAnalyzer<T> : IAnalyzer where T : new()
    {
        private bool _valid;
        private byte[] _raw;
        private T _data;
        private int _passMiliSecond;
        private int _timeOut;
        private readonly Timer _timer;

        /// <summary>
        /// 分析出新数据对象时触发的委托函数
        /// </summary>
        /// <param name="m"></param>
        public delegate void DataAnalyzedHandler(ProtocolAnalyzer<T> m);

        /// <summary>
        /// 分析出新数据对象时触发的事件
        /// </summary>
        public event DataAnalyzedHandler OnDataAnalyzed;

        /// <summary>
        /// 协议分析方法,可以在里面处理数据模型转换等
        /// </summary>
        public abstract void Analyze();

        /// <summary>
        /// 数据解包方法，用来处理数据字节流到协议的匹配转换
        /// </summary>
        /// <param name="buffer">要分析的数据</param>
        /// <returns>分析结果</returns>
        public abstract SearchResult SearchBuffer(List<byte> buffer);

        /// <summary>
        /// constructor
        /// </summary>
        protected ProtocolAnalyzer()
        {
            TimerCallback callback = null;
            this._data = (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
            this._timeOut = 0x7d0;
            callback = new TimerCallback(this.Callback);
            this._timer = new Timer(callback, null, -1, -1);
            this._data = (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
        }


        private void Callback(object obj)
        {
            if (Valid)
            {
                _timer.Change(-1, -1);
                _data = (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
                Valid = false;
            }
        }

        /// <summary>
        /// 隐式类型转换
        /// </summary>
        /// <param name="ar">数据解析类</param>
        public static implicit operator T(ProtocolAnalyzer<T> ar)
        {
            return ar.Data;
        }

        /// <summary>
        /// 字符串转换
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Data.ToString();
        }

        /// <summary>
        /// 处理后的数据内容
        /// </summary>
        public T Data
        {
            get
            {
                return this._data;
            }
            set
            {
                this._data = value;
            }
        }

        /// <summary>
        /// 计时
        /// </summary>
        public int PassMiliSecond
        {
            get
            {
                return this._passMiliSecond;
            }
            set
            {
                this._passMiliSecond = value;
            }
        }
        /// <summary>
        /// 未经协议转换处理的原始数据
        /// </summary>
        public byte[] Raw
        {
            get
            {
                if (this._raw != null)
                {
                    return this._raw;
                }
                return new byte[0];
            }
            set
            {
                this._raw = value;
            }
        }
        /// <summary>
        /// 超时时间
        /// </summary>
        public int TimeOut
        {
            get
            {
                if (this._timeOut != -1)
                {
                    return (this._timeOut / 0x3e8);//0x3e8 = 1000
                }
                return this._timeOut;
            }
            set
            {
                this._timeOut = (value == -1) ? value : (value * 0x3e8);
            }
        }

        /// <summary>
        /// 验证数据
        /// </summary>
        public bool Valid
        {
            get
            {
                return this._valid;
            }
            set
            {
                this._valid = value;
                if (value)
                {
                    this._passMiliSecond = Environment.TickCount;
                    this._timer.Change(-1, -1);
                    this._timer.Change(this._timeOut, -1);
                }
                if (this.OnDataAnalyzed != null)
                {
                    List<DataAnalyzedHandler> list = new List<DataAnalyzedHandler>();
                    Delegate[] invocationList = this.OnDataAnalyzed.GetInvocationList();
                    int index = 0;
                    while (true)
                    {
                        if (index >= invocationList.Length)
                        {
                            break;
                        }
                        DataAnalyzedHandler item = (DataAnalyzedHandler)invocationList[index];
                        try
                        {
                            item((ProtocolAnalyzer<T>)this);
                        }
                        catch (InvalidOperationException)
                        {
                            list.Add(item);
                        }
                        index++;
                    }
                    foreach (DataAnalyzedHandler handler in list)
                    {
                        this.OnDataAnalyzed = (DataAnalyzedHandler)Delegate.Remove(this.OnDataAnalyzed, handler);
                    }
                }
            }
        }
    }
}
