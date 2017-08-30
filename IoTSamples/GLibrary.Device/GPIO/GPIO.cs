using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace GLibrary.Device.GPIO
{
    public class GPIO:CommonBase
    {

        #region 私有属性
        private GpioController controller = GpioController.GetDefault();
        private GpioPin Pin { get; set; }
        #endregion

        #region 属性

        /// <summary>
        /// 引脚号
        /// </summary>
        public int PinNumber { get; set; }

        /// <summary>
        /// 消抖时间
        /// </summary>
        public TimeSpan DebounceTimeout { get { return Pin.DebounceTimeout; } set { Pin.DebounceTimeout = value; } }

        /// <summary>
        /// 共享模式
        /// </summary>
        public GpioSharingMode SharingMode { get; set; }
        #endregion 属性

        #region 构造函数
        /// <summary>
        /// 使用引脚号初始化一个IO
        /// </summary>
        /// <param name="PinNumber">引脚号</param>
        public GPIO(int PinNumber)
        {
            try
            {
                this.Pin = controller.OpenPin(PinNumber);
                this.Pin.ValueChanged += Pin_ValueChanged;
            }
            catch (Exception ex)
            {
                RaiseOnException(this, ex);
            }

        }

        /// <summary>
        /// 使用引脚号和IO共享模式初始化一个IO
        /// </summary>
        /// <param name="PinNumber">引脚号</param>
        /// <param name="SharingMode">共享模式</param>
        public GPIO(int PinNumber, GpioSharingMode SharingMode)
        {
            try
            {
                this.Pin = controller.OpenPin(PinNumber, SharingMode);
                this.Pin.ValueChanged += Pin_ValueChanged;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// 设定GPIO的驱动模式
        /// </summary>
        /// <param name="PinDriveMode">IO驱动模式</param>
        public void SetDriveMode(GpioPinDriveMode PinDriveMode)
        {
            if (this.Pin == null) throw new NullReferenceException();
            Pin.SetDriveMode(PinDriveMode);
        }

        public override void Dispose()
        {
            this.Pin.Dispose();
            this.Dispose();
        }

        /// <summary>
        /// 获取GPIO的驱动模式
        /// </summary>
        /// <returns>IO驱动模式</returns>
        public GpioPinDriveMode GetDriveMode()
        {
            return Pin.GetDriveMode();
        }


        /// <summary>
        /// 获得GPIO当前电平状态
        /// </summary>
        /// <returns>IO电平状态</returns>
        public GpioPinValue Read()
        {
            return this.Pin.Read();
        }

        /// <summary>
        /// 设定IO电平状态
        /// </summary>
        /// <param name="Value">设置电平状态</param>
        public void Write(GpioPinValue Value)
        {
            this.Pin.Write(Value);
        }

        #endregion 方法

        #region 事件

        /// <summary>
        /// 在引脚值发生改变时发生
        /// </summary>
        public event TypedEventHandler<GPIO, GpioPinValueChangedEventArgs> PinValueChanged;


        #endregion 事件

        

        #region 私有方法
        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            RaiseValueChanged(args);
        }

        private void RaiseValueChanged(GpioPinValueChangedEventArgs args)
        {
            PinValueChanged?.Invoke(this, args);
        }


        #endregion  私有方法
    }
}
