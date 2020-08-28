using log4net;
using System;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace BradescoPGP.Common.Logging
{
    public class Log
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Log()
        {

        }

        public static void Debug(object messageOrEntity)
        {
            log.Debug(messageOrEntity);
        }

        public static void Debug(object messageOrEntity, Exception ex)
        {
            log.Debug(messageOrEntity);
        }

        public static void DebugFormat(string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }

        public static void Information(object messageOrEntity)
        {
            log.Info(messageOrEntity);
        }

        public static void Information(object messageOrEntity, Exception ex)
        {
            log.Info(messageOrEntity, ex);
        }


        public static void InformationFormat(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        public static void Warning(object messageOrEntity)
        {
            log.Warn(messageOrEntity);
        }

        public static void Warning(object messageOrEntity, Exception ex)
        {
            log.Warn(messageOrEntity, ex);
        }

        public static void WarningFormat(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public static void Error(object messageOrEntity)
        {
            log.Error(messageOrEntity);
        }

        public static void Error(object messageOrEntity, Exception ex)
        {
            log.Error(messageOrEntity, ex);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        public static void Fatal(object messageOrEntity)
        {
            log.Fatal(messageOrEntity);
        }

        public static void Fatal(object messageOrEntity, Exception ex)
        {
            log.Fatal(messageOrEntity, ex);
        }

        public static void FatalFormat(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }
    }
}
