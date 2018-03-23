using System;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using EnvDTE80;

namespace GitTimeMetrics
{
    public class GitTimeMetricsActions
    {
        private DateTime LastTimeStamp = DateTime.MinValue;
        private string CurrentFile;
        private int UpdateInterval = 30;
        public string GitTimeMetricsExecutablePath { get; set; }
        public DTE2 Dte;
        public static GitTimeMetricsActions Instance = new GitTimeMetricsActions();
        private GitTimeMetricsActions()
        {

        }

        protected bool IsValidPath()
        {
            return File.Exists(GitTimeMetricsExecutablePath);
        }

        public void RecordEntry()
        {
            Document doc = Dte.ActiveDocument;
            if (doc == null) return;
            RecordEntry(doc.FullName);
        }

        public void RecordEntry(string fileName)
        {
            if (!IsValidPath())
                return;
            if (Dte == null)
                return;
            if (string.IsNullOrEmpty(fileName))
                return;
            if (fileName == CurrentFile && (DateTime.Now - LastTimeStamp).TotalSeconds < UpdateInterval)
                return;
            string args = $"record \"{fileName}\"";
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = $"\"{GitTimeMetricsExecutablePath}\"",
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            System.Diagnostics.Process.Start(processStartInfo);
            CurrentFile = fileName;
            LastTimeStamp = DateTime.Now;
        }

    }
}
