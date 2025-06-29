using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectStructureAnalyzer
{
    public class DirectoryAnalyzer
    {
        public List<string> FolderFilters { get; set; } = new List<string>();
        public List<string> FileFilters { get; set; } = new List<string>();

        public async Task<ProjectItem> AnalyzeDirectoryAsync(string path, string rootPath)
        {
            var item = new ProjectItem
            {
                Name = Path.GetFileName(path),
                FullPath = path,
                IsDirectory = true,
                Size = 0,
                Extension = string.Empty,
                FileCount = 0
            };

            // Проверяем, нужно ли пропустить папку
            if (FolderFilters.Any(f => path.Contains(f, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.LogInfo($"Directory {path} skipped due to filter.");
                return null;
            }

            try
            {
                var directories = Directory.GetDirectories(path);
                foreach (var dir in directories)
                {
                    var subItem = await AnalyzeDirectoryAsync(dir, rootPath);
                    if (subItem != null)
                    {
                        item.Children.Add(subItem);
                        item.FileCount += subItem.FileCount; // Суммируем количество файлов
                        item.Size += subItem.Size; // Суммируем размеры (если реализовано)
                    }
                }

                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    if (FileFilters.Any(f => file.EndsWith(f, StringComparison.OrdinalIgnoreCase) ||
                                          Path.GetFileName(file) == f))
                    {
                        Logger.LogInfo($"File {file} skipped due to filter.");
                        continue;
                    }
                    var fileItem = new ProjectItem
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false,
                        Size = new FileInfo(file).Length,
                        Extension = Path.GetExtension(file),
                        FileCount = 0
                    };
                    item.Children.Add(fileItem);
                    item.FileCount++;
                    item.Size += fileItem.Size;
                }

                return item;
            }
            catch (UnauthorizedAccessException)
            {
                Logger.LogInfo($"Access denied to directory: {path}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error analyzing directory {path}", ex);
                return null;
            }
        }
    }
}