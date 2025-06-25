using System.IO;
using System.Text;

namespace ProjectStructureAnalyzer
{
    public static class ExportService
    {
        public static void ExportProjectStructure(string filePath, ProjectItem rootItem, string rootPath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                ExportItem(writer, rootItem, 0, rootPath);
            }
        }

        private static void ExportItem(StreamWriter writer, ProjectItem item, int indentLevel, string rootPath)
        {
            string indent = new string(' ', indentLevel * 2);
            string relativePath = item.FullPath.Replace(rootPath, "").TrimStart('\\');
            writer.WriteLine($"{indent}{(item.IsDirectory ? "📁" : "📄")} {relativePath}");

            foreach (var child in item.Children)
            {
                ExportItem(writer, child, indentLevel + 1, rootPath);
            }
        }
    }
}