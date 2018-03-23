using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace GitTimeMetrics
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false)]
    [ComVisible(true)]
    public class GitTimeMetricsOptions : DialogPage
    {
        private string DefaultGitTimeMetricsExecutablePath = @"C:\Data\Portable Apps\gitplugins\";


        [Category("GitTimeMetrics Information")]
        [DisplayName("GitTimeMetrics Executable Location")]
        [Description("The location of the gtm.exe file.")]
        public string GitTimeMetricsExecutablePath
        {
            get;
            set;
        }


        public override void LoadSettingsFromStorage()
        {
            GitTimeMetricsExecutablePath = Path.Combine(DefaultGitTimeMetricsExecutablePath, "gtm.exe");
            base.LoadSettingsFromStorage();
        }
    }
}
