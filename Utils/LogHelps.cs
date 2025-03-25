using BLL;
using Model;
using SmartTuningSystem.Global;
using static Model.Log;

namespace SmartTuningSystem.Utils
{
    public class LogHelps
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static readonly LogManager LogManager = new LogManager();

        public static void Trace(string strMsg)
        {
            Logger.Trace(strMsg);
        }

        public static void Debug(string strMsg)
        {
            Logger.Debug(strMsg);
        }

        public static void Info(string strMsg)
        {
            Logger.Info(strMsg);
        }

        public static void Warn(string strMsg)
        {
            Logger.Warn(strMsg);
        }

        public static void Error(string strMsg)
        {
            Logger.Error(strMsg);
        }

        public static void Fatal(string strMsg)
        {
            Logger.Fatal(strMsg);
        }

        public static void WriteLogToDb(string logStr, LogLevel logType)
        {
            LogManager.AddLog(new Log
            {
                LogStr = logStr,
                LogType = logType,
                CreateName = UserGlobal.CurrUser.UserName,
                CreateNo = UserGlobal.CurrUser.UserNo
            });
        }
    }
}
