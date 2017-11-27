/*
    Copyright (c) 2017 Inkton.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Newtonsoft.Json;
using Xamarin.Forms;
using System.IO.Compression;

[assembly: Dependency(typeof(Inkton.Nester.Admin.LogService))]

namespace Inkton.Nester.Admin
{
    public class LogService : ILogService
    {
        private long _maxSize = 1024 * 256;
        private long _maxFiles = 3;
        private LogSeverity _severity = LogSeverity.LogSeverityInfo;

        public LogService()
        {
            Directory.CreateDirectory(this.Path);
        }

        public string Path
        {
            get
            {
                return System.IO.Path.Combine(
                    ApplicationData.Current.LocalFolder.Path, "nester/log");
            }
        }

        public long MaxSize
        {
            get { return _maxSize; }
            set { _maxSize = value; }
        }

        public long MaxFiles
        {
            get { return _maxFiles; }
            set { _maxFiles = value; }
        }

        public LogSeverity Severity
        {
            get { return _severity; }
            set { _severity = value; }
        }

        public void Trace(
            string info,
            string location = "",
            LogSeverity severity = LogSeverity.LogSeverityInfo)
        {
            if (severity <= _severity)
            {
                return;
            }

            try
            {
                string path = this.Path + @"\current.log";
                StreamWriter writer = null;

                if (File.Exists(path))
                {
                    FileInfo fi = new FileInfo(path);

                    if (fi.Length >= _maxSize)
                    {
                        string newPath = this.Path + string.Format(@"\%s",
                                System.DateTime.UtcNow.ToString("yyyy-MM-ddTHHZ"));

                        File.Move(path, newPath);

                        string[] files = Directory.GetFiles(
                            this.Path, "*.*", SearchOption.TopDirectoryOnly);

                        if (files.Length > _maxFiles)
                        {
                            string oldestFile = null;
                            DateTime earliestTime = DateTime.Now;

                            foreach (string file in files)
                            {
                                if (!file.EndsWith(".log"))
                                {
                                    try
                                    {
                                        DateTime fileTime = DateTime.Parse(
                                            file, null, System.Globalization.DateTimeStyles.RoundtripKind);

                                        if (oldestFile == null ||
                                                fileTime < earliestTime)
                                        {
                                            earliestTime = fileTime;
                                            oldestFile = file;
                                        }
                                    }
                                    catch (FormatException e)
                                    {
                                        writer = File.AppendText(path);
                                        writer.WriteLine(@"{'%s', '%s'}\n", e.Message, "LogService.Trace");
                                        return;
                                    }
                                }
                            }

                            if (oldestFile != null)
                            {
                                File.Delete(this.Path + @"\" + oldestFile);
                            }
                        }
                    }
                }

                writer = File.AppendText(path);
                writer.WriteLine(@"{0}, {1}\n", info, location);
            }
            catch (Exception) { }
        }
    }
}
