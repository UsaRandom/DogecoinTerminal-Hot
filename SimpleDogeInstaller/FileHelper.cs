using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDogeInstaller
{
    internal class FileHelper
    {
        public static void TryDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to delete file: ${path}");
                Console.WriteLine(ex);
            }
        }

        public static void TryDeleteDirectoryContents(string path, string fileToSkip = "")
        {
            try
            {
                // Get all files in the directory
                var files = Directory.GetFiles(path);

                // Delete all files except the specified file
                foreach (var file in files)
                {
                    if (Path.GetFileName(file) != fileToSkip)
                    {
                        File.Delete(file);
                    }
                }

                // Get all subdirectories
                var directories = Directory.GetDirectories(path);

                // Delete all subdirectories
                foreach (var directory in directories)
                {
                    Directory.Delete(directory, true);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete directory: ${path}");
                Console.WriteLine(ex);
            }
        }

        public static void TryDeleteDirectory(string path)
        {
            try
            {
                    Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete directory: ${path}");
                Console.WriteLine(ex);
            }
        }
    }
}
