using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PrivCh.Utility
{
    internal static class SystemUtil
    {
        #region Process
        public static bool IsProcessExist(string procName)
        {
            Process[] list = Process.GetProcessesByName(procName);
            if (list != null && list.Length > 1)
            {
                foreach (Process process in list)
                {
                    process.Dispose();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static void KillProcess(string procName, string filePath)
        {
            // list contains all "privoxy" process
            Process[] list = Process.GetProcessesByName(procName);
            if (list != null && list.Length > 0)
            {
                foreach (Process process in list)
                {
                    // kill app's privoxy process
                    if (process.MainModule.FileName == filePath)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                        catch { }
                    }

                    process.Dispose();
                }
            }
        }
        #endregion

        #region Shortcut
        // 33 ms
        public static void CheckOrCreateUserStartupShortcut()
        {
            if (FindUserStartupShortcuts(App.FileApplication).Count > 0)
            {
                return;
            }

            string startUpDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutName = App.Name;

            // Create the shortcut
            WshShell wshShell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(
                $@"{startUpDirectory}\{shortcutName}.lnk");

            shortcut.TargetPath = App.FileApplication;
            shortcut.WorkingDirectory = App.DirectoryApplication;
            shortcut.Description = shortcutName;
            //shortcut.IconLocation = Application.StartupPath + @"\App.ico";
            shortcut.Save();
        }

        public static void DeleteUserStartupShortcuts()
        {
            List<string> shortcuts = FindUserStartupShortcuts(App.FileApplication);
            foreach (string shortcut in shortcuts)
            {
                System.IO.File.Delete(shortcut);
            }
        }

        public static List<string> FindUserStartupShortcuts(string targetPath)
        {
            string startUpDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            FileInfo[] shortcutFiles = new DirectoryInfo(startUpDirectory).GetFiles("*.lnk");

            List<string> result = new List<string>();
            foreach (FileInfo shortcut in shortcutFiles)
            {
                string shortcutTargetPath = GetShortcutTargetPath(shortcut.FullName);

                if (shortcutTargetPath.EndsWith(targetPath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(shortcut.FullName);
                }
            }

            return result;
        }

        // 8 ms
        public static string GetShortcutTargetPath(string shortcutFile)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFile);
            string filenameOnly = Path.GetFileName(shortcutFile);

            Shell32.Shell shell = new Shell32.ShellClass();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return string.Empty; // Not found
        }
        #endregion
    }
}
