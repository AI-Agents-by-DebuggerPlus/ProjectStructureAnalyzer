using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectStructureAnalyzer
{
    public class DirectoryAnalyzer
    {
        public async Task<ProjectItem?> AnalyzeDirectoryAsync(string path, string rootPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                if (!dirInfo.Exists)
                {
                    Logger.LogError($"Directory does not exist: {path}", new DirectoryNotFoundException($"Directory not found: {path}"));
                    return null;
                }

                string[] folderFilters = Properties.Settings.Default.FolderFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                if (folderFilters.Any(filter => dirInfo.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.LogInfo($"Directory {dirInfo.Name} skipped due to filter.");
                    return null;
                }

                var item = new ProjectItem
                {
                    Name = dirInfo.Name,
                    FullPath = dirInfo.FullName,
                    IsDirectory = true,
                    Children = new System.Collections.ObjectModel.ObservableCollection<ProjectItem>()
                };

                var children = new ConcurrentBag<ProjectItem>();

                await Task.Run(() =>
                {
                    Parallel.ForEach(dirInfo.GetDirectories(), subDir =>
                    {
                        var childItem = AnalyzeSubDirectory(subDir, rootPath);
                        if (childItem != null)
                        {
                            children.Add(childItem);
                            item.FileCount += childItem.FileCount;
                        }
                    });
                });

                foreach (var file in dirInfo.GetFiles())
                {
                    string[] fileFilters = Properties.Settings.Default.FileFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    if (fileFilters.Any(filter => file.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                    {
                        Logger.LogInfo($"File {file.Name} skipped due to filter.");
                        continue;
                    }

                    var fileItem = new ProjectItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        IsDirectory = false,
                        Size = file.Length,
                        Extension = file.Extension
                    };
                    children.Add(fileItem);
                    item.FileCount++;
                }

                item.Children = new System.Collections.ObjectModel.ObservableCollection<ProjectItem>(
                    children.OrderBy(c => !c.IsDirectory).ThenBy(c => c.Name));

                return item.Children.Any() || item.FileCount > 0 ? item : null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error analyzing directory: {path}", ex);
                return null;
            }
        }

        private ProjectItem? AnalyzeSubDirectory(DirectoryInfo dirInfo, string rootPath)
        {
            try
            {
                string[] folderFilters = Properties.Settings.Default.FolderFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                if (folderFilters.Any(filter => dirInfo.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.LogInfo($"Directory {dirInfo.Name} skipped due to filter.");
                    return null;
                }

                var item = new ProjectItem
                {
                    Name = dirInfo.Name,
                    FullPath = dirInfo.FullName,
                    IsDirectory = true,
                    Children = new System.Collections.ObjectModel.ObservableCollection<ProjectItem>()
                };

                var children = new ConcurrentBag<ProjectItem>();

                foreach (var subDir in dirInfo.GetDirectories())
                {
                    var childItem = AnalyzeSubDirectory(subDir, rootPath);
                    if (childItem != null)
                    {
                        children.Add(childItem);
                        item.FileCount += childItem.FileCount;
                    }
                }

                foreach (var file in dirInfo.GetFiles())
                {
                    string[] fileFilters = Properties.Settings.Default.FileFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    if (fileFilters.Any(filter => file.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                    {
                        Logger.LogInfo($"File {file.Name} skipped due to filter.");
                        continue;
                    }

                    var fileItem = new ProjectItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        IsDirectory = false,
                        Size = file.Length,
                        Extension = file.Extension
                    };
                    children.Add(fileItem);
                    item.FileCount++;
                }

                item.Children = new System.Collections.ObjectModel.ObservableCollection<ProjectItem>(
                    children.OrderBy(c => !c.IsDirectory).ThenBy(c => c.Name));

                return item.Children.Any() || item.FileCount > 0 ? item : null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error analyzing subdirectory: {dirInfo.FullName}", ex);
                return null;
            }
        }
    }
}