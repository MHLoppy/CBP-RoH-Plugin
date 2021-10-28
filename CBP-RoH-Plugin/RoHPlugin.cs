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
        public string PluginVersion => "0.5.1";
        public string PluginAuthor => "MHLoppy";
        public bool CBPCompatible => false;
        public string PluginDescription => "A loader/unloader for the mod \"Rise of Humankind - The Calm and The Storm\" by Tark.\n\nSource code: https://github.com/MHLoppy/CBP-RoH-Plugin";
        public bool IsSimpleMod => true;
        public string LoadResult { get; set; }

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
                try
                {
                    File.WriteAllText(loadedRoH, "0");
                    LoadResult = (PluginTitle + " completed first time setup successfully.");
                    MessageBox.Show(PluginTitle + " detected for first time. Created file:\n" + loadedRoH);
                }
                catch (Exception ex)
                {
                    LoadResult = (PluginTitle + ": error writing first-time file:\n\n" + ex);
                    MessageBox.Show(PluginTitle + ": error writing first-time file:\n\n" + ex);
                    // loading plugins tab from this point will probably cause an error because the messagebox will interrupt control generation
                    // I tried using dispatcher but it didn't seem to provide the expected low-effort resolution https://stackoverflow.com/questions/23452864/wpf-dispatcher-processing-has-been-suspended-but-messages-are-still-being-pro
                }
            }
            else
            {
                Console.WriteLine(loadedRoH + " already exists.");
            }

            CheckIfLoaded();//this can be important to do here, otherwise the bool might be accessed without a value depending on how other stuff is set up
        }

        public bool CheckIfLoaded()
        {
            //this doesn't seem to solve what I wanted it to solve (unexpectedly getting a null value on the IsLoaded bool after not calling DoSomething first)
            /*if (localRoH == null)
            {
                localRoH = Path.Combine(Directory.GetCurrentDirectory(), "CBP", "riseofhumankind.txt");
                //MessageBox.Show("new path");
            }*/
            //MessageBox.Show(localRoH);//just debug

            if (File.ReadAllText(loadedRoH) != "0")
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
            if (MessageBox.Show("Please confirm that you want to install Rise of Humankind to this location:\n" + localRoH, "Confirm Mod Installation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    DirectoryCopy(workshopRoH, localRoH, true, false);
                    File.WriteAllText(loadedRoH, "1");
                    CheckIfLoaded();
                    LoadResult = (PluginTitle + " was installed successfully.");
                    MessageBox.Show("Rise of Humankind - The Calm and The Storm has been installed.");
                }
                catch (Exception ex)
                {
                    LoadResult = (PluginTitle + " had an error copying to local mods folder: " + ex);
                    MessageBox.Show("Error copying RoH to local mods folder:\n\n" + ex);
                }
            }
            else
                MessageBox.Show("No action has been taken.");
        }

        public void UpdatePlugin(string workshopModsPath, string localModsPath)
        {
            if (CheckIfLoaded())
            {
                try
                {
                    DirectoryCopy(workshopRoH, localRoH, true, true);
                    File.WriteAllText(loadedRoH, "1");
                    CheckIfLoaded();
                    LoadResult = (PluginTitle + " re-installed (updated) successfully.");
                    MessageBox.Show("Rise of Humankind has been re-installed (updated).");
                }
                catch (Exception ex)
                {
                    LoadResult = (PluginTitle + " had an error updating files: " + ex);
                    MessageBox.Show(PluginTitle + ": error updating files:\n\n" + ex);
                }
            }
        }

        /*private void ForceUpdatePluginIfLoaded()
        {
            // only force update if already loaded
            if (CheckIfLoaded())
            {
                try
                {
                    // only update it if it hasn't been updated already, by using "2" to signify an update has already been done
                    int number = Convert.ToInt32(File.ReadAllText(loadedRoH));
                    if (number > 1)
                    {
                        File.WriteAllText(loadedRoH, "1");
                    }
                    else
                    {
                        
                        DirectoryCopy(workshopRoH, localRoH, true, true);
                        File.WriteAllText(loadedRoH, "2");
                    }

                    DirectoryCopy(workshopRoH, localRoH, true, true);
                    File.WriteAllText(loadedRoH, "1");
                    CheckIfLoaded();
                    Console.WriteLine("Rise of Humankind has been re-installed (updated).");
                }
                catch (Exception ex)
                {
                    // because this is called from DoSomething, can't use messagebox
                    Console.WriteLine("Error updating RoH in local mods folder: " + ex);
                }
            }
        }*/

        public void UnloadPlugin(string workshopModsPath, string localModsPath)
        {
            if (MessageBox.Show("Please confirm that you want to remove Rise of Humankind from this location:\n" + localRoH, "Confirm Mod Removal", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.Delete(localRoH, true);
                    File.WriteAllText(loadedRoH, "0");
                    CheckIfLoaded();
                    LoadResult = (PluginTitle + " local mod files were successfully removed.");
                    MessageBox.Show("Rise of Humankind - The Calm and The Storm has been uninstalled.");
                }
                catch (Exception ex)
                {
                    LoadResult = (PluginTitle + " errored while removing local mod files: " + ex);
                    MessageBox.Show("Error removing local copy of RoH:\n\n" + ex);
                }
            }
            else
                MessageBox.Show("No action has been taken.");
        }

        // MS reference implementation, but with an extra bool to specify file overwriting since the force update needs that
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
