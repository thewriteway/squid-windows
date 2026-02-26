/*
 * Copyright (C) 2015 Diladele B.V.
 *
 * Diladele Squid Installer software is distributed under GPL license.
 */

using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceProcess;
using Diladele.Squid.Tray;

namespace Diladele.Squid.Service
{
    public partial class SquidService : ServiceBase
    {
        private Process squid;
        private System.Threading.Timer timer;
        private readonly object locker;

        public SquidService()
        {
            InitializeComponent();

            if (!EventLog.SourceExists("Squid Service Source"))
            {
                EventLog.CreateEventSource("Squid Service Source", "Squid Service Log");
            }

            this.eventLog.Source = "Squid Service Source";
            this.eventLog.Log = "Squid Service Log";
            this.locker = new object();
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.WriteLine("Press enter to finish");
            Console.ReadLine();
            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                this.eventLog.WriteEntry("Squid is starting...", EventLogEntryType.Information);

                StartSquidProcess();

                this.timer = new System.Threading.Timer(this.OnTimer, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Squid could not be started: " + e.Message, EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                this.eventLog.WriteEntry("Squid is stopping...", EventLogEntryType.Information);

                timer?.Dispose();
                timer = null;

                lock (this.locker)
                {
                    // Step 1: graceful shutdown
                    StopSquidGracefully();

                    // Step 2: wait and force kill if needed
                    var processes = Process.GetProcessesByName("squid");

                    foreach (var p in processes)
                    {
                        try
                        {
                            if (!p.WaitForExit(5000))
                            {
                                this.eventLog.WriteEntry(
                                    $"Force killing squid process '{p.Id}'.",
                                    EventLogEntryType.Warning);

                                p.Kill();
                            }
                        }
                        catch
                        {
                            this.eventLog.WriteEntry(
                                $"Could not terminate squid process '{p.Id}'.",
                                EventLogEntryType.Warning);
                        }
                    }

                    this.squid = null;
                }

                this.eventLog.WriteEntry("Squid stopped.", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Squid could not be stopped: " + e.Message, EventLogEntryType.Error);
                throw;
            }
        }

        private void OnTimer(object state)
        {
            try
            {
                lock (this.locker)
                {
                    var processes = Process.GetProcessesByName("squid");
                    if (processes == null || processes.Length == 0)
                    {
                        eventLog.WriteEntry("Cannot find a squid process. Trying to start it...", EventLogEntryType.Information);
                        this.squid = null;
                        StartSquidProcess();
                    }
                }
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Squid could not be restarted: " + e.Message, EventLogEntryType.Error);
                throw;
            }
        }

        private void StartSquidProcess()
        {
            lock (this.locker)
            {
                this.squid = new Process();
                this.squid.StartInfo.FileName = PredefinedPaths.InstallationFolder + @"\bin\squid.exe";
                this.squid.StartInfo.CreateNoWindow = true;
                this.squid.StartInfo.UseShellExecute = false;
                this.squid.StartInfo.WorkingDirectory = PredefinedPaths.InstallationFolder + @"\bin";
                this.squid.StartInfo.Arguments = "-N";

                this.squid.Start();

                this.eventLog.WriteEntry(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Squid started: process id '{0}'.",
                        this.squid.Id));
            }
        }

        private void StopSquidGracefully()
        {
            try
            {
                var shutdown = new Process();
                shutdown.StartInfo.FileName = PredefinedPaths.InstallationFolder + @"\bin\squid.exe";
                shutdown.StartInfo.Arguments = "-k shutdown";
                shutdown.StartInfo.CreateNoWindow = true;
                shutdown.StartInfo.UseShellExecute = false;
                shutdown.StartInfo.WorkingDirectory = PredefinedPaths.InstallationFolder + @"\bin";

                shutdown.Start();

                this.eventLog.WriteEntry("Sent graceful shutdown signal to squid.", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                this.eventLog.WriteEntry("Graceful shutdown failed: " + e.Message, EventLogEntryType.Warning);
            }
        }
    }
}