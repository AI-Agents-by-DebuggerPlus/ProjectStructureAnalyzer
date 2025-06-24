using System;
using System.Collections.Generic; // ДОБАВЛЕНО для HashSet
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ProjectStructureAnalyzer
{
    public class DirectoryAnalyzer
    {
        private readonly string logFilePath = "structure_log.txt";

        public async Task<ProjectItem> AnalyzeDirectoryAsync(string path, string selectedPath)
        {
            var dirInfo = new DirectoryInfo(path);
            var item = new ProjectItem
            {
                Name = dirInfo.Name,
                FullPath = path,
                IsDirectory = true,
                Children = new ObservableCollection<ProjectItem>(),
                IsUserFolder = !IsSystemFolder(dirInfo.Name)
            };

            // --- ИЗМЕНЕНО: Исправлено чтение настроек фильтров ---
            var folderExclusions = (Properties.Settings.Default.FolderFilters ?? "")
                .Split(',')
                .Where(f => !string.IsNullOrEmpty(f))
                .ToHashSet(); // HashSet для более быстрой проверки

            var fileExclusions = (Properties.Settings.Default.FileFilters ?? "")
                .Split(',')
                .Where(f => !string.IsNullOrEmpty(f))
                .ToHashSet();
            // --- КОНЕЦ ИЗМЕНЕНИЙ ---

            Log($"Analyzing directory: {path}");

            try
            {
                // Анализируем все дочерние папки, исключая те, что в folderExclusions
                foreach (var dir in dirInfo.GetDirectories().Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden)))
                {
                    Log($"Checking folder: {dir.Name}");
                    if (!folderExclusions.Contains(dir.Name))
                    {
                        Log($"Folder {dir.Name} not excluded, analyzing...");
                        var childItem = await AnalyzeDirectoryAsync(dir.FullName, selectedPath);
                        if (childItem != null)
                        {
                            item.Children.Add(childItem);
                            Log($"Added folder: {dir.Name}");
                        }
                    }
                    else
                    {
                        Log($"Folder {dir.Name} excluded by filter.");
                    }
                }

                // Добавляем файлы, исключая те, что в fileExclusions
                if (path == selectedPath || !folderExclusions.Contains(dirInfo.Name))
                {
                    Log($"Processing files in {dirInfo.Name}");
                    foreach (var file in dirInfo.GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)))
                    {
                        Log($"Checking file: {file.Name}");
                        // ИСПРАВЛЕНО: Убрана проверка на fileExclusions.Count, Contains корректно работает с пустым HashSet
                        if (!fileExclusions.Contains(file.Extension.ToLower()))
                        {
                            Log($"File {file.Name} not excluded, adding...");
                            item.Children.Add(new ProjectItem
                            {
                                Name = file.Name,
                                FullPath = file.FullName,
                                IsDirectory = false,
                                Size = file.Length,
                                Extension = file.Extension.ToLower()
                            });
                        }
                        else
                        {
                            Log($"File {file.Name} excluded by filter.");
                        }
                    }
                }
                else
                {
                    Log($"No file processing for {dirInfo.Name} as it is excluded by folder filter.");
                }

                item.FileCount = CountFiles(item);
            }
            catch (UnauthorizedAccessException)
            {
                Log($"Unauthorized access to {path}, skipping.");
                return null;
            }

            // Возвращаем item, если он корневой или имеет детей/файлы
            bool hasChildren = item.Children.Any();
            bool isExcludedFolder = folderExclusions.Contains(dirInfo.Name);
            bool hasFiles = item.FileCount > 0;
            Log($"Evaluation: HasChildren={hasChildren}, IsExcludedFolder={isExcludedFolder}, HasFiles={hasFiles}");

            // Упрощенная логика возврата
            if (path == selectedPath) return item;
            if (hasChildren) return item;

            return null;
        }

        // Вспомогательные методы
        private void Log(string message)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log error: {ex.Message}");
            }
        }

        private int CountFiles(ProjectItem node)
        {
            if (!node.IsDirectory) return 1;

            int count = 0;
            foreach (var child in node.Children)
            {
                count += CountFiles(child);
            }
            return count;
        }

        private bool IsSystemFolder(string folderName)
        {
            string[] systemFolders = { "bin", "obj", "Debug", "Release", ".vs", "packages" };
            return systemFolders.Contains(folderName.ToLower());
        }
    }
}