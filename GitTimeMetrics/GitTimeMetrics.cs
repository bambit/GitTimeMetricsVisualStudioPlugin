//------------------------------------------------------------------------------
// <copyright file="GitTimeMetrics.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitTimeMetrics
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(GitTimeMetrics.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(GitTimeMetricsOptions), "GitTimeMetrics Information", "General", 0, 0, true, new[] { "GitTimeMetrics", })]
    public sealed class GitTimeMetrics : Package
    {

        private Lazy<Events> events;
        private Lazy<TextEditorEvents> textEditorEvents;
        private readonly Lazy<DTE2> dte;


        private readonly Lazy<GitTimeMetricsOptions> options;
        private Lazy<DocumentEvents> documentEvents;

        private Lazy<IVsStatusbar> StatusBar;
        private Lazy<BuildEvents> buildEvents;

        /// <summary>
        /// GitTimeMetrics GUID string.
        /// </summary>
        public const string PackageGuidString = "aaad6bc6-db19-431e-9fa6-3e1d3b80ba9f";

        /// <summary>
        /// Initializes a new instance of the <see cref="GitTimeMetrics"/> class.
        /// </summary>
        public GitTimeMetrics()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            dte = new Lazy<DTE2>(() => ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as DTE2);
            options = new Lazy<GitTimeMetricsOptions>(() => GetDialogPage(typeof(GitTimeMetricsOptions)) as GitTimeMetricsOptions, true);
            events = new Lazy<EnvDTE.Events>(() => dte.Value.Events);
            textEditorEvents = new Lazy<EnvDTE.TextEditorEvents>(() => events.Value.TextEditorEvents);
            documentEvents = new Lazy<DocumentEvents>(() => events.Value.DocumentEvents);
            buildEvents = new Lazy<BuildEvents>(() => events.Value.BuildEvents);
            StatusBar = new Lazy<IVsStatusbar>(() => GetService(typeof(SVsStatusbar)) as IVsStatusbar);
        }

        #region Package Members

        private void SetStatusText(string text)
        {
            int frozen;
            StatusBar.Value.IsFrozen(out frozen);
            if (frozen == 0)
            {
                StatusBar.Value.SetText(text);
            }
        }
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
         
            textEditorEvents.Value.LineChanged += LineChangedEvent;
            documentEvents.Value.DocumentSaved += DocumentSavedEvents;
            documentEvents.Value.DocumentOpened += DocumentSavedEvents;
            buildEvents.Value.OnBuildProjConfigBegin += ProjectBuildBegin;
            buildEvents.Value.OnBuildProjConfigDone += ProjectBuildDone;
            GitTimeMetricsActions.Instance.GitTimeMetricsExecutablePath = options.Value.GitTimeMetricsExecutablePath;
            GitTimeMetricsActions.Instance.Dte = dte.Value;
            GitTimeMetricsActions.Instance.TextUpdated += (sender, args) => { SetStatusText(args.Text); };
        }

        

        private void ProjectBuildDone(string project, string projectconfig, string platform, string solutionconfig, bool success)
        {
            GitTimeMetricsActions.Instance.RecordProjectEntry(project);
        }

        private void ProjectBuildBegin(string project, string projectconfig, string platform, string solutionconfig)
        {
            GitTimeMetricsActions.Instance.RecordProjectEntry(project);
        }

        private void DocumentSavedEvents(Document document)
        {

            GitTimeMetricsActions.Instance.RecordEntry(document.FullName);
        }

        private void LineChangedEvent(TextPoint startpoint, TextPoint endpoint, int hint)
        {
            GitTimeMetricsActions.Instance.RecordEntry();
        }
        
        #endregion
        }
}
