
namespace Nester.Admin
{
    public enum LogSeverity
    {
        LogSeverityInfo = 0,
        LogSeverityWarning = 1,
        LogSeverityCritical = 2
    };

    public interface ILogService
    {
        string Path
        {
            get;
        }

        long MaxSize
        {
            get;
            set;
        }

        long MaxFiles
        {
            get;
            set;
        }

        LogSeverity Severity
        {
            get;
            set;
        }

        void Trace(
            string info, 
            string location = "",
            LogSeverity severity = LogSeverity.LogSeverityInfo);
    }
}