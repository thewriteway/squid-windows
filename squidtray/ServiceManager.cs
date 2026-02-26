/*
 * Copyright (C) 2015 Diladele B.V.
 *
 * Diladele Squid Installer software is distributed under GPL license.
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Diladele.Squid.Tray
{
    internal sealed class ServiceManager : IDisposable
    {
        private ServiceController controller;
        private const string ServiceName = "squidsrv";

        public ServiceManager()
        {
            this.controller = new ServiceController(ServiceName);
        }

        public void StopService()
        {
            if (!Exists)
            {
                ShowError("Squid service does not exist.");
                return;
            }

            try
            {
                controller.Refresh();

                if (controller.Status == ServiceControllerStatus.Stopped ||
                    controller.Status == ServiceControllerStatus.StopPending)
                {
                    return;
                }

                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access denied
            {
                ElevateAndRun("stop");
            }
            catch (InvalidOperationException ex)
            {
                HandleException(ex);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        public void StartService()
        {
            if (!Exists)
            {
                ShowError("Squid service does not exist.");
                return;
            }

            try
            {
                controller.Refresh();

                if (controller.Status == ServiceControllerStatus.Running ||
                    controller.Status == ServiceControllerStatus.StartPending)
                {
                    return;
                }

                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20));
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access denied
            {
                ElevateAndRun("start");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ElevateAndRun(string action)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c net {action} {ServiceName}",
                    Verb = "runas", // triggers UAC
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var p = Process.Start(psi))
                {
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                    {
                        ShowError($"Failed to {action} service. Exit code: {p.ExitCode}");
                    }
                }
            }
            catch (Win32Exception ex)
            {
                // User cancelled UAC
                if (ex.NativeErrorCode == Constants.OperationCancelled)
                {
                    MessageBox.Show(
                        "Operation cancelled by user.",
                        "Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    ShowError(ex.Message);
                }
            }
        }

        public ServiceControllerStatus GetStatus()
        {
            if (!Exists)
                return ServiceControllerStatus.Stopped;

            controller.Refresh();
            return controller.Status;
        }

        public bool NotAvailable
        {
            get
            {
                if (!Exists)
                    return true;

                const string basepathStr = @"System\CurrentControlSet\services\";
                string subKeyStr = basepathStr + ServiceName;

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKeyStr))
                {
                    return (int)key.GetValue("Start") == 4;
                }
            }
        }

        private bool Exists
        {
            get
            {
                return ServiceController
                    .GetServices()
                    .Any(s => s.ServiceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void HandleException(InvalidOperationException ex)
        {
            if (ex.InnerException is Win32Exception win32 && win32.NativeErrorCode == 2)
            {
                ShowError("Service not found.");
            }
            else
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        public void Dispose()
        {
            controller?.Dispose();
            controller = null;
        }
    }
}