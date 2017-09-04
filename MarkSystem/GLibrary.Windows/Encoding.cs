using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows
{
    /// <summary>
    /// 字符编码方式
    /// </summary>
    public enum Encoding
    {
        /// <summary>
        /// 十六进制
        /// </summary>
        Hex,

        /// <summary>
        /// 字符串
        /// </summary>
        String
    }

    /// <summary>
    /// 分割字符
    /// </summary>
    public class SplitChar
    {
        /// <summary>
        /// 逗号 
        /// </summary>
        public const char Comma = ',';
        /// <summary>
        /// 空格 
        /// </summary>
        public const char Space = ' ';
        /// <summary>
        /// 或号 
        /// </summary>
        public const char Or = '|';
        /// <summary>
        /// 与号 
        /// </summary>
        public const char And = '&';
        /// <summary>
        /// 句号 
        /// </summary>
        public const char Period = '.';
        /// <summary>
        /// 连字符 
        /// </summary>
        public const char Dash = '-';
    }
}
