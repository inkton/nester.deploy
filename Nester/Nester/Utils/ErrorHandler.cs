using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace Nester.Utils
{
    public static class ErrorHandler
    {
        public static void Exception(string detail, string location)
        {
            Admin.ILogService log = DependencyService.Get<Admin.ILogService>();
            log.Trace(detail, location, Admin.LogSeverity.LogSeverityCritical);
        }
    }
}
