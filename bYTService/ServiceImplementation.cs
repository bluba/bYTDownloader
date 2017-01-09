using bYTApi;
using bYTService.Framework;
using System;
using System.IO;
using System.ServiceProcess;
using System.Timers;

namespace bYTService
{
    /// <summary>
    /// The actual implementation of the windows service goes here...
    /// </summary>
    [WindowsService("bYTService",
        DisplayName = "bYTService",
        Description = "The description of the bYTService service.",
        EventLogSource = "bYTService",
        StartMode = ServiceStartMode.Automatic)]
    public class ServiceImplementation : IWindowsService
    {
        private Timer readNewTimer = null;
        private Timer updateTimer = null;
        private bool readNewIsRunning = false;
        private bool updateIsRunning = false;
        private bYTManager manager;
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        /// <summary>
        /// This method is called when the service gets a request to start.
        /// </summary>
        /// <param name="args">Any command line arguments</param>
        public void OnStart(string[] args)
        {
            Configuration.Initialize();

            manager = new bYTManager(Configuration.Instance.ApiKey);

            readNewTimer = new Timer(Configuration.Instance.ReadNewCycleMs);
            readNewTimer.Elapsed += ReadNewTimer_Elapsed;
            readNewTimer.Start();

            updateTimer = new Timer(Configuration.Instance.UpdateCycleMs);
            updateTimer.Start();
            updateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(updateIsRunning)
            {
                Logger.Debug("Service already processing");
                return;
            }
            updateIsRunning = true;

            foreach (var item in Configuration.Instance.UpdateHistory)
            {
                Logger.Debug(string.Format("Updating playlist {0}", item.PlaylistKey));
                var updated = manager.UpdatePlaylist(item.PlaylistKey, item.LastEntry, item.Path);
                if(updated!=item.LastEntry)
                {
                    Logger.Debug(string.Format("Item updated. {2} new videos added.", updated-item.LastEntry));
                }
                item.LastEntry = updated;
                Configuration.Save();
            }

            updateIsRunning = false;
        }

        private void ReadNewTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.Debug("Running cycle");
            if (readNewIsRunning)
            {
                Logger.Debug("Service already processing");
                return;
            }
            readNewIsRunning = true;

            foreach (var file in Directory.GetFiles(Configuration.Instance.VideoDL))
            {
                var data = File.ReadAllLines(file);
                foreach (var line in data)
                {
                    Logger.Debug("Executing video dl:{0}".FormatWith(line));
                    var parts = line.Split(';');
                    try
                    {
                        manager.DownloadVideo(parts.Length == 2 ? parts[1] : null, parts[0]);
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorException("Error while downloading video", ex);
                    }
                }
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(Configuration.Instance.PlaylistDL))
            {
                var data = File.ReadAllLines(file);
                foreach (var line in data)
                {
                    Logger.Debug("Executing playlist dl:{0}".FormatWith(line));
                    var parts = line.Split(';');
                    try
                    {
                        var count = manager.DownloadPlaylist(parts[1], parts.Length > 2 ? int.Parse(parts[2]) : 0, parts.Length > 1 ? parts[0] : null);
                        var entry = Configuration.Instance.UpdateHistory.Find(x => x.PlaylistKey == parts[1]);
                        if(entry!=null)
                        {
                            entry.Path = parts.Length > 1 ? parts[0] : null;
                            entry.LastEntry = count;
                        }
                        else
                        {
                            entry = new PlaylistCache();
                            entry.Path = parts.Length > 1 ? parts[0] : null;
                            entry.PlaylistKey = parts[1];
                            entry.LastEntry = count;
                            Configuration.Instance.UpdateHistory.Add(entry);
                        }
                        Configuration.Save();
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorException("Error while downloading playlist", ex);
                    }

                }
                File.Delete(file);
            }
            readNewIsRunning = false;
            Logger.Debug("Cycle stopped");
        }

        /// <summary>
        /// This method is called when the service gets a request to stop.
        /// </summary>
        public void OnStop()
        {
        }

        /// <summary>
        /// This method is called when a service gets a request to pause,
        /// but not stop completely.
        /// </summary>
        public void OnPause()
        {
        }

        /// <summary>
        /// This method is called when a service gets a request to resume 
        /// after a pause is issued.
        /// </summary>
        public void OnContinue()
        {
        }

        /// <summary>
        /// This method is called when the machine the service is running on
        /// is being shutdown.
        /// </summary>
        public void OnShutdown()
        {
        }

        /// <summary>
        /// This method is called when a custom command is issued to the service.
        /// </summary>
        /// <param name="command">The command identifier to execute.</param >
        public void OnCustomCommand(int command)
        {
            if(command==0)
            {
                ReadNewTimer_Elapsed(null, null);
            }
            if (command == 1)
            {
                UpdateTimer_Elapsed(null, null);
            }
        }
    }
}
