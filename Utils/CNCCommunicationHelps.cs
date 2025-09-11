using System;
using System.Collections.Generic;
using System.Linq;
using SmartTuningSystem.Global;
using static Model.Log;

namespace SmartTuningSystem.Utils
{
    public class CNCCommunicationHelps
    {
        //默认端口号：8193（FOCAS2 协议常用）
        public static decimal? GetCncValue(string ip, string paramAddress)
        {
            decimal? value = null;
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

        //批量获取
        public static Dictionary<string, decimal> GetCncValue(string ip, List<string> paramAddress)
        {
            Dictionary<string, decimal> value = new Dictionary<string, decimal>();
            try
            {
                if (Focas64.cnc_allclibhndl3(ip, 8193, 3, out var temp) == Focas64.EW_OK) //连接设备
                {
                    Focas64.IODBMR iOdbmr = new Focas64.IODBMR();
                    foreach (var p in paramAddress)
                    {
                        if (Focas64.cnc_rdmacror(temp, Convert.ToInt16(p), Convert.ToInt16(p), 1000, iOdbmr) == Focas64.EW_OK) //读取设备
                            value.Add(p, Convert.ToDecimal(iOdbmr.data.data1.mcr_val / Math.Pow(10, iOdbmr.data.data1.dec_val)));
                        else
                        {
                            LogHelps.Error($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{p}],报错原因：读值失败");
                            LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{p}],报错原因：读值失败", LogLevel.Error);
                        }
                    }

                    Focas64.cnc_freelibhndl(temp);//断开设备
                }
                else
                {
                    LogHelps.Error($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{string.Join(",", paramAddress)}],报错原因：连接失败");
                    LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{string.Join(",", paramAddress)}],报错原因：连接失败", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelps.Error($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{string.Join(",", paramAddress)}],报错原因：{ex.Message + ex.StackTrace}");
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{string.Join(",", paramAddress)}],报错原因：{ex.Message + ex.StackTrace}"
                    , LogLevel.Error);
            }

            return value;
        }

        public static decimal GetCncValue(Dictionary<string, ushort> dicIps, string ip, string paramAddress)
        {
            decimal value = 0;
            try
            {
                foreach (var i in dicIps)
                {
                    if (i.Key != ip) continue;
                    Focas64.IODBMR iOdbmr = new Focas64.IODBMR();
                    if (Focas64.cnc_rdmacror(i.Value, Convert.ToInt16(paramAddress), Convert.ToInt16(paramAddress), 1000, iOdbmr) == Focas64.EW_OK) //读取设备
                        value = Convert.ToDecimal(iOdbmr.data.data1.mcr_val / Math.Pow(10, iOdbmr.data.data1.dec_val));
                    else
                    {
                        LogHelps.Error($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],报错原因：读值失败");
                        LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}获取CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],报错原因：读值失败", LogLevel.Error);
                    }
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

        //批量写入
        public static void SetCncValue(string ip, Dictionary<string, decimal> value)
        {
            try
            {
                if (Focas64.cnc_allclibhndl3(ip, 8193, 3, out var temp) == Focas64.EW_OK) //连接设备
                {
                    foreach (var v in value)
                    {
                        Focas64.IODBMR iOdbmr = new Focas64.IODBMR
                        {
                            datano_s = Convert.ToInt16(v.Key), //起始宏编号
                            datano_e = Convert.ToInt16(v.Key)
                        };
                        iOdbmr.data.data1.mcr_val = (int)(v.Value * 100000);
                        iOdbmr.data.data1.dec_val = 5;

                        if (Focas64.cnc_wrmacror(temp, (short)(8 + sizeof(double) * 1), iOdbmr) != 0)
                        {
                            LogHelps.Error($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{v.Key}],参数值：[{v.Value}],报错原因：写入失败");
                            LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{v.Key}],参数值：[{v.Value}],报错原因：写入失败", LogLevel.Error);
                        }
                    }

                    Focas64.cnc_freelibhndl(temp);//断开设备
                }
                else
                {
                    LogHelps.Error($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{string.Join(",", value.Keys.ToList())}],参数值：[{string.Join(",", value.Values.ToList())}],报错原因：连接失败");
                    LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{string.Join(",", value.Keys.ToList())}],参数值：[{string.Join(",", value.Values.ToList())}],报错原因：连接失败", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelps.Error($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{string.Join(",", value.Keys.ToList())}],参数值：[{string.Join(",", value.Values.ToList())}],报错原因：{ex.Message + ex.StackTrace}");
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{string.Join(",", value.Keys.ToList())}],参数值：[{string.Join(",", value.Values.ToList())}],报错原因：{ex.Message + ex.StackTrace}"
                    , LogLevel.Error);
            }
        }

        public static void SetCncValue(Dictionary<string, ushort> dicIps, string ip, string paramAddress, decimal value)
        {
            try
            {
                foreach (var i in dicIps)
                {
                    if (i.Key != ip) continue;
                    Focas64.IODBMR iOdbmr = new Focas64.IODBMR
                    {
                        datano_s = Convert.ToInt16(paramAddress), //起始宏编号
                        datano_e = Convert.ToInt16(paramAddress)
                    };
                    iOdbmr.data.data1.mcr_val = (int)(value * 100000);
                    iOdbmr.data.data1.dec_val = 5;

                    if (Focas64.cnc_wrmacror(i.Value, (short)(8 + sizeof(double) * 1), iOdbmr) != 0)
                    {
                        LogHelps.Error($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：写入失败");
                        LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：写入失败", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelps.Error($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：{ex.Message + ex.StackTrace}");
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台IP：[{ip}],参数地址：[{paramAddress}],参数值：[{value}],报错原因：{ex.Message + ex.StackTrace}"
                    , LogLevel.Error);
            }
        }

        //批量连接机台设备
        public static Dictionary<string, ushort> ConnectCnc(List<string> ip)
        {
            Dictionary<string, ushort> rst = new Dictionary<string, ushort>();
            try
            {
                foreach (var i in ip)
                {
                    if (Focas64.cnc_allclibhndl3(i, 8193, 3, out var temp) == Focas64.EW_OK) //连接设备
                    {
                        rst.Add(i, temp);
                    }
                    else
                    {
                        LogHelps.Error($@"{UserGlobal.CurrUser.UserName}批量连接机台报错：机台IP：[{i}],报错原因：连接失败");
                        LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}批量连接机台报错：机台IP：[{i}],报错原因：连接失败", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelps.Error($@"{UserGlobal.CurrUser.UserName}批量连接机台报错：机台IP：[{string.Join(",", ip)}],报错原因：{ex.Message + ex.StackTrace}");
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}批量连接机台报错：机台IP：[{string.Join(",", ip)}],报错原因：{ex.Message + ex.StackTrace}", LogLevel.Error);
            }

            return rst;
        }

        //批量断开机台设备
        public static void DisConnectCnc(Dictionary<string, ushort> value)
        {
            try
            {
                foreach (var v in value.Where(v => Focas64.cnc_freelibhndl(v.Value) != Focas64.EW_OK))
                {
                    LogHelps.Error($@"{UserGlobal.CurrUser.UserName}批量断开机台报错：机台IP：[{v.Key}],报错原因：断开失败");
                    LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}批量断开机台报错：机台IP：[{v.Key}],报错原因：断开失败", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelps.Error($@"{UserGlobal.CurrUser.UserName}批量断开机台报错：机台IP：[{string.Join(",", value.Keys.ToList())}],报错原因：{ex.Message + ex.StackTrace}");
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}批量断开机台报错：机台IP：[{string.Join(",", value.Keys.ToList())}],报错原因：{ex.Message + ex.StackTrace}"
                    , LogLevel.Error);
            }
        }
    }
}
