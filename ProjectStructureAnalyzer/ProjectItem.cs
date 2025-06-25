using System.Collections.ObjectModel;

namespace ProjectStructureAnalyzer
{
    public class ProjectItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public string Extension { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public ObservableCollection<ProjectItem> Children { get; set; } = new ObservableCollection<ProjectItem>();
    }
}