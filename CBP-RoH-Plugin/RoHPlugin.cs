/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Windows;
using CBPSDK;

namespace CBPRoHPlugin
{
    public class RoHPlugin : IPluginCBP
    {
        public string PluginTitle => "Rise of Humankind (loader/unloader)";
        public string PluginVersion => "0.4.0";
        public string PluginAuthor => "MHLoppy";
        public bool CBPCompatible => false;
        public string PluginDescription => "A loader/unloader for the mod \"Rise of Humankind - The Calm and The Storm\" by Tark.";
        public bool IsSimpleMod => true;

        private string workshopRoH;
        private string localRoH;
        private string loadedRoH;

        public void DoSomething(string workshopModsPath, string localModsPath)
        {
            workshopRoH = Path.Combine(workshopModsPath, "2613116925");
            localRoH = Path.Combine(localModsPath, "Rise of Humankind - The Calm and The Storm");
            loadedRoH = Path.GetFullPath(Path.Combine(localModsPath, @"..\", "CBP", "riseofhumankind.txt"));

            //if file doesn't exist, make one
            if (!File.Exists(loadedRoH))
            {
                File.WriteAllText(loadedRoH, "0");
                MessageBox.Show("Plugin detected for first time. Created file: " + loadedRoH);
            }
            else
            {
                Console.WriteLine(loadedRoH + " already exists.");
            }

            CheckIfLoaded();//this can be important to do here, otherwise the bool might be accessed without a value depending on how other stuff is set up
            ForceUpdatePluginIfLoaded();

            //MessageBox.Show(Directory.GetCurrentDirectory().ToString());
        }

        public bool CheckIfLoaded()
        {
            if (File.ReadAllText(loadedRoH) == "1")
            {
                Console.WriteLine(PluginTitle + "is loaded");
                return true;
            }
            else
            {
                Console.WriteLine(PluginTitle + "is not loaded");
                return false;
            }
        }

        public void LoadPlugin(string workshopModsPath, string localModsPath)
        {
            if (MessageBox.Show("Please confirm that you want to install Rise of Humankind to this location:\n" + localRoH, "Please confirm mod installation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    DirectoryCopy(workshopRoH, localRoH, true, false);
                    File.WriteAllText(loadedRoH, "1");
                    CheckIfLoaded();
                    MessageBox.Show("Rise of Humankind - The Calm and The Storm has been installed.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error copying RoH to local mods folder: " + ex);
                }
            }
            else
                MessageBox.Show("No action has been taken.");
        }

        private void ForceUpdatePluginIfLoaded()
        {
            // only force update if already loaded
            if (CheckIfLoaded())
            {
                try
                {
                    DirectoryCopy(workshopRoH, localRoH, true, true);
                    File.WriteAllText(loadedRoH, "1");
                    CheckIfLoaded();
                    Console.WriteLine("Rise of Humankind has been re-installed (updated).");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating RoH in local mods folder: " + ex);
                }
            }
        }

        public void UnloadPlugin(string workshopModsPath, string localModsPath)
        {
            if (MessageBox.Show("Please confirm that you want to remove Rise of Humankind from this location:\n" + localRoH, "Please confirm mod removal", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.Delete(localRoH, true);
                    File.WriteAllText(loadedRoH, "0");
                    CheckIfLoaded();
                    MessageBox.Show("Rise of Humankind - The Calm and The Storm has been uninstalled.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing local copy of RoH: " + ex);
                }
            }
            else
                MessageBox.Show("No action has been taken.");
        }

        // MS reference implementation, but with an extra bool to specify file overwriting
        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwriteFiles)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source folder does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, overwriteFiles);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs, overwriteFiles);
                }
            }
        }
    }
}
