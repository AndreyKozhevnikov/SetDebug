﻿//------------------------------------------------------------------------------
// <copyright file="SetDebug.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Timers;

namespace SetDebug {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SetDebug {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("8cd8045b-088e-4347-8476-cf49e7c5bb76");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDebug"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private SetDebug(Package package, EnvDTE.DTE _dte) {
            dte = _dte;
            var ev = dte.Events.BuildEvents;
            ev.OnBuildDone += Ev_OnBuildDone;
            if (package == null) {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null) {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SetDebug Instance {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider {
            get {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package, EnvDTE.DTE _dte) {
            Instance = new SetDebug(package, _dte);

        }
        EnvDTE.DTE dte;
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e) {
            var curr = dte.Solution.SolutionBuild.ActiveConfiguration.Name;
            if (curr == "Release") {
                var cnt = dte.Solution.SolutionBuild.SolutionConfigurations.Count;
                for (int i = 1; i <= cnt; i++) {
                    var it = dte.Solution.SolutionBuild.SolutionConfigurations.Item(i);
                    if (it.Name == "Debug") {
                        it.Activate();
                        return;
                    }
                }
            }
            // Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        Timer timer;
        private void Ev_OnBuildDone(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action) {
            timer = new Timer(500);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            timer.Stop();
            var curMode = this.dte.Solution.DTE.Debugger.CurrentMode;
            if (curMode == EnvDTE.dbgDebugMode.dbgDesignMode) {
                MenuItemCallback(null, null);
            }
        }
    }
}
