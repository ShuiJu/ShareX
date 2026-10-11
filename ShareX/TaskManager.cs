#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using ShareX.HistoryLib;
using ShareX.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Bitmap = SkiaSharp.SKBitmap;

namespace ShareX
{
    public static class TaskManager
    {
        public static event Action<WorkerTask> TaskAdded;
        public static event Action<WorkerTask> TaskRemoved;
        public static event Action<WorkerTask> TaskChanged;
        public static event Action<WorkerTask, Bitmap> TaskImageReady;
        public static event Action TaskCollectionChanged;

        public static List<WorkerTask> Tasks { get; } = new List<WorkerTask>();
        public static RecentTaskManager RecentManager { get; } = new RecentTaskManager();
        public static bool IsBusy => Tasks.Count > 0 && Tasks.Any(task => task.IsBusy);

        private static int lastIconStatus = -1;

        public static void Start(WorkerTask task)
        {
            if (task != null)
            {
                Tasks.Add(task);

                if (task.Status != TaskStatus.History)
                {
                    task.StatusChanged += Task_StatusChanged;
                    task.ImageReady += Task_ImageReady;
                    task.TaskCompleted += Task_TaskCompleted;
                }

                TaskAdded?.Invoke(task);
                TaskCollectionChanged?.Invoke();

                if (task.Status != TaskStatus.History)
                {
                    StartTasks();
                }
            }
        }

        public static void Remove(WorkerTask task)
        {
            if (task != null)
            {
                task.Stop();
                Tasks.Remove(task);

                TaskRemoved?.Invoke(task);
                TaskCollectionChanged?.Invoke();

                task.Dispose();
            }
        }

        private static void StartTasks()
        {
            int workingTasksCount = Tasks.Count(x => x.IsWorking);
            WorkerTask[] inQueueTasks = Tasks.Where(x => x.Status == TaskStatus.InQueue).ToArray();

            if (inQueueTasks.Length > 0)
            {
                int len;

                if (ApplicationState.Settings.TaskLimit == 0)
                {
                    len = inQueueTasks.Length;
                }
                else
                {
                    len = (ApplicationState.Settings.TaskLimit - workingTasksCount).Clamp(0, inQueueTasks.Length);
                }

                for (int i = 0; i < len; i++)
                {
                    inQueueTasks[i].Start();
                }
            }
        }

        public static void StopAllTasks()
        {
            foreach (WorkerTask task in Tasks)
            {
                if (task != null) task.Stop();
            }
        }

        private static void Task_StatusChanged(WorkerTask task)
        {
            DebugHelper.WriteLine("Task status: " + task.Status);

            UpdateProgressUI();
            TaskChanged?.Invoke(task);
        }

        private static void Task_ImageReady(WorkerTask task, Bitmap image)
        {
            TaskChanged?.Invoke(task);
            TaskImageReady?.Invoke(task, image);
        }

        private static void Task_TaskCompleted(WorkerTask task)
        {
            try
            {
                if (task != null)
                {
                    task.KeepImage = false;

                    TaskInfo info = task.Info;

                    if (info != null)
                    {
                        string result = info.ToString();

                        if (!string.IsNullOrEmpty(result))
                        {
                            if (ApplicationState.Settings.HistorySaveTasks && task.Status == TaskStatus.Completed && !string.IsNullOrEmpty(info.FilePath))
                            {
                                HistoryItem historyItem = info.GetHistoryItem();
                                AppendHistoryItemAsync(historyItem);
                            }

                            RecentManager.Add(task);
                        }

                        if (task.Status == TaskStatus.Stopped)
                        {
                            DebugHelper.WriteLine($"Task stopped. File name: {info.FileName}");
                        }
                        else if (task.Status == TaskStatus.Failed)
                        {
                            string errors = task.ErrorMessage;

                            DebugHelper.WriteLine($"Task failed. File name: {info.FileName}, Errors:\r\n{errors}");

                            TaskHelpers.PlayNotificationSoundAsync(NotificationSound.Error, info.TaskSettings);

                            if (info.TaskSettings.GeneralSettings.ShowToastNotificationAfterTaskCompleted && !string.IsNullOrEmpty(errors) &&
                                (!info.TaskSettings.GeneralSettings.DisableNotificationsOnFullscreen || !CaptureHelpers.IsActiveWindowFullscreen()))
                            {
                                TaskHelpers.ShowNotificationTip(errors, "ShareX - " + Strings.TaskManager_TaskCompleted_Error, 5000);
                            }
                        }
                        else
                        {
                            DebugHelper.WriteLine($"Task completed. File name: {info.FileName}, Duration: {(long)info.TaskDuration.TotalMilliseconds} ms");

                            if (!task.StopRequested && !string.IsNullOrEmpty(result))
                            {
                                TaskHelpers.PlayNotificationSoundAsync(NotificationSound.TaskCompleted, info.TaskSettings);

                                if (info.TaskSettings.GeneralSettings.ShowToastNotificationAfterTaskCompleted && !string.IsNullOrEmpty(result) &&
                                    (!info.TaskSettings.GeneralSettings.DisableNotificationsOnFullscreen || !CaptureHelpers.IsActiveWindowFullscreen()))
                                {
                                    task.KeepImage = true;

                                    NotificationWindowConfig toastConfig = new NotificationWindowConfig()
                                    {
                                        Duration = (int)(info.TaskSettings.GeneralSettings.ToastWindowDuration * 1000),
                                        FadeDuration = (int)(info.TaskSettings.GeneralSettings.ToastWindowFadeDuration * 1000),
                                        Placement = info.TaskSettings.GeneralSettings.ToastWindowPlacement,
                                        Size = info.TaskSettings.GeneralSettings.ToastWindowSize,
                                        LeftClickAction = info.TaskSettings.GeneralSettings.ToastWindowLeftClickAction,
                                        RightClickAction = info.TaskSettings.GeneralSettings.ToastWindowRightClickAction,
                                        MiddleClickAction = info.TaskSettings.GeneralSettings.ToastWindowMiddleClickAction,
                                        ActionButtonSize = info.TaskSettings.GeneralSettings.ToastWindowButtonSize,
                                        ActionButtons = NotificationActionButton.CloneButtons(info.TaskSettings.GeneralSettings.ToastWindowButtons),
                                        FilePath = info.FilePath,
                                        Image = task.Image,
                                        Title = "ShareX - " + Strings.TaskManager_TaskCompleted_ShareX___Task_completed,
                                        Text = result
                                    };

                                    NotificationWindow.Show(toastConfig);

                                }
                            }
                        }

                    }
                }
            }
            finally
            {
                TaskChanged?.Invoke(task);

                if (!IsBusy && StartupOptions.AutoClose)
                {
                    ApplicationLifecycle.Exit();
                }
                else
                {
                    StartTasks();
                    UpdateProgressUI();

                    if (ApplicationState.Settings.SaveSettingsAfterTaskCompleted && !IsBusy)
                    {
                        SettingManager.SaveAllSettingsAsync();
                    }
                }
            }
        }


        public static void UpdateProgressUI()
        {
            MainWindowIntegration.SetTitle(ApplicationInfo.Title);
            UpdateTrayIcon();
            TaskbarManager.SetProgressState(IsBusy ? TaskbarProgressBarStatus.Indeterminate : TaskbarProgressBarStatus.NoProgress);
        }

        public static void UpdateTrayIcon(int progress = -1)
        {
            if (ApplicationState.Settings.TrayIconProgressEnabled && ApplicationState.Settings.ShowTray && lastIconStatus != progress)
            {
                byte[] icon;

                if (progress >= 0)
                {
                    try
                    {
                        icon = Helpers.GetProgressIconBytes(progress, Color.FromArgb(16, 116, 193));
                    }
                    catch (Exception e)
                    {
                        DebugHelper.WriteException(e);
                        progress = -1;
                        if (lastIconStatus == progress) return;
                        icon = ShareXResources.IconBytes;
                    }
                }
                else
                {
                    icon = ShareXResources.IconBytes;
                }

                MainWindowIntegration.SetTrayIcon(icon);

                lastIconStatus = progress;
            }
        }

        public static void AddTestTasks(int count)
        {
            for (int i = 0; i < count; i++)
            {
                WorkerTask task = WorkerTask.CreateHistoryTask(new RecentTask()
                {
                    FilePath = @"..\..\..\ShareX.HelpersLib\Resources\ShareX_Logo.png"
                });

                Start(task);
            }
        }

        public static async Task TestTrayIcon()
        {
            for (int i = 0; i <= 100; i++)
            {
                UpdateTrayIcon(i);

                await Task.Delay(50);
            }
        }

        private static void AppendHistoryItemAsync(HistoryItem historyItem)
        {
            Task.Run(() =>
            {
                try
                {
                    ApplicationState.HistoryManager.AppendHistoryItem(historyItem);
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                    e.ShowError();
                }
            });
        }

        public static void AddRecentTasksToMainWindow()
        {
            if (Tasks.Count == 0)
            {
                foreach (RecentTask recentTask in RecentManager.Tasks)
                {
                    WorkerTask task = WorkerTask.CreateHistoryTask(recentTask);
                    Start(task);
                }
            }
        }
    }
}
