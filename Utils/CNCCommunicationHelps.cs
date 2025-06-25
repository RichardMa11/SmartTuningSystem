using System;
using SmartTuningSystem.Global;
using static Model.Log;

namespace SmartTuningSystem.Utils
{
    public class CNCCommunicationHelps
    {
        //默认端口号：8193（FOCAS2 协议常用）
        public static decimal GetCncValue(string ip, string paramAddress)
        {
            decimal value = 0;
            try
            {
                if (Focas64.cnc_allclibhndl3(ip, 8193, 3, out var temp) == Focas64.EW_OK) //连接设备
                {
                    Focas64.IODBMR iOdbmr = new Focas64.IODBMR();
                    if (Focas64.cnc_rdmacror(temp, Convert.ToInt16(paramAddress), Convert.ToInt16(paramAddress), 1000, iOdbmr) == Focas64.EW_OK) //读取设备
                        value = Convert.ToDecimal(iOdbmr.data.data1.mcr_val / Math.Pow(10, iOdbmr.data.data1.dec_val));
                    else
                    {
                        LogHelps.Error($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],报错原因：读值失败");
                        LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],报错原因：读值失败", LogLevel.Error);
                    }

                    Focas64.cnc_freelibhndl(temp);//断开设备
                }
                else
                {
                    LogHelps.Error($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],报错原因：连接失败");
                    LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],报错原因：连接失败", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelps.Error($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],报错原因：{ex.Message + ex.StackTrace}");
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],报错原因：{ex.Message + ex.StackTrace}"
                    , LogLevel.Error);
            }
            return value;
        }

        public static void SetCncValue(string ip, string paramAddress, decimal value)
        {
            try
            {
                if (Focas64.cnc_allclibhndl3(ip, 8193, 3, out var temp) == Focas64.EW_OK) //连接设备
                {
                    Focas64.IODBMR iOdbmr = new Focas64.IODBMR
                    {
                        datano_s = Convert.ToInt16(paramAddress), //起始宏编号
                        datano_e = Convert.ToInt16(paramAddress)
                    };
                    iOdbmr.data.data1.mcr_val = (int)(value * 100000);
                    iOdbmr.data.data1.dec_val = 5;

                    if (Focas64.cnc_wrmacror(temp, (short)(8 + sizeof(double) * 1), iOdbmr) != 0)
                    {
                        LogHelps.Error($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：写入失败");
                        LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：写入失败", LogLevel.Error);
                    }

                    Focas64.cnc_freelibhndl(temp);//断开设备
                }
                else
                {
                    LogHelps.Error($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：连接失败");
                    LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：连接失败", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelps.Error($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：{ex.Message + ex.StackTrace}");
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：{ex.Message + ex.StackTrace}"
                    , LogLevel.Error);
            }
        }
    }
}
