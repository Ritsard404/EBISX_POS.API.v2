using System.Diagnostics;

namespace EBISX_POS.API.Settings
{
    public class FilePaths
    {
        private string _imagePath = string.Empty;
        private string _backUp = string.Empty;

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                Debug.WriteLine($"Setting ImagePath to: {value}");
                _imagePath = GetFullPath(value);
                Debug.WriteLine($"Resolved ImagePath to: {_imagePath}");
            }
        }

        public string BackUp
        {
            get => _backUp;
            set
            {
                Debug.WriteLine($"Setting BackUp to: {value}");
                _backUp = GetFullPath(value);
                Debug.WriteLine($"Resolved BackUp to: {_backUp}");
            }
        }

        private string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.WriteLine("Warning: Empty path provided to FilePaths");
                return string.Empty;
            }

            try
            {
                // If the path is already absolute, return it as is
                if (Path.IsPathRooted(path))
                {
                    Debug.WriteLine($"Path '{path}' is already absolute");
                    return path;
                }

                // Otherwise, make it relative to the application's base directory
                var baseDir = AppContext.BaseDirectory;
                Debug.WriteLine($"Base directory: {baseDir}");
                
                var fullPath = Path.GetFullPath(Path.Combine(baseDir, path));
                Debug.WriteLine($"Resolved path '{path}' to '{fullPath}'");
                
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Debug.WriteLine($"Creating directory: {directory}");
                    Directory.CreateDirectory(directory);
                }
                
                return fullPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resolving path '{path}': {ex}");
                throw;
            }
        }
    }
}
