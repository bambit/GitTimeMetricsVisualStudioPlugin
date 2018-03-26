using System;
using System.Diagnostics;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Process = System.Diagnostics.Process;

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

        public class TextUpdatedEventArgs : EventArgs
        {
            public string Text { get; set; }
        }

        public EventHandler<TextUpdatedEventArgs> TextUpdated;
        private GitTimeMetricsActions()
        {

        }

        protected void OnTextUpdated(string text)
        {
            EventHandler<TextUpdatedEventArgs> handles = TextUpdated;
            if (TextUpdated != null)
                TextUpdated(this, new TextUpdatedEventArgs {Text = text});
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

        public void RecordProjectEntry(string projectName)
        {
            string solutionName = Dte.Solution.FullName;
            if (string.IsNullOrEmpty(solutionName))
                return ;
            string solutionPath = Path.GetDirectoryName(solutionName);
            if (string.IsNullOrEmpty(solutionPath))
                return ;
            string path = Path.Combine(solutionPath, projectName);
            RecordEntry(path);
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
            string args = $"record --status \"{fileName}\"";
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = $"\"{GitTimeMetricsExecutablePath}\"",
                Arguments = args,
                CreateNoWindow=true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput=true,
            };
            Process recordProcess = new Process
            {
                StartInfo=processStartInfo,
            };
            recordProcess.OutputDataReceived += RecordProcess_OutputDataReceived;
            CurrentFile = fileName;
            LastTimeStamp = DateTime.Now;
            recordProcess.Start();
            string output = recordProcess.StandardOutput.ReadToEnd();
            OnTextUpdated($"Git Time Metric: {output}");
            recordProcess.WaitForExit();
        }

        private void RecordProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string outputRecevied = e.Data;
        }
    }
}
