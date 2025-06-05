using System;
using SmartTuningSystem.Global;
using static Model.Log;

namespace SmartTuningSystem.Utils
{
    public class CNCCommunicationHelps
    {
        public static decimal GetCncValue(string ip, string paramAddress)
        {
            decimal value = 0;
            try
            {

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
