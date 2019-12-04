using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace SetVersionCli
{
    internal static class DirectoryIterator
    {
        internal static void IterateProjectFiles(string curdir, Action<string, XmlDocument, XmlNode> action)
        {
            IterateFiles(curdir, action);
            IterateProjectFiles(Directory.GetDirectories(curdir), action);
        }
        internal static void IterateProjectFiles(string[] dirs, Action<string, XmlDocument, XmlNode> action)
        {
            foreach (var dir in dirs)
            {
                IterateFiles(dir, action);
                IterateProjectFiles(Directory.GetDirectories(dir), action);
            }
        }

        internal static void IterateFiles(string dir, Action<string, XmlDocument, XmlNode> action)
        {
            var files = Directory.GetFiles(dir, "*.csproj").ToList();
            files.AddRange(Directory.GetFiles(dir, "*.nuspec"));

            foreach (var file in files)
            {
                var xml = "";
                using (var stream = new StreamReader(file))
                {
                    xml = stream.ReadToEnd();
                }
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                var project = document.FirstChild;

                if (document.ChildNodes.Count == 2)
                {
                    project = document.ChildNodes[1];
                }

                action.Invoke(file, document, project);
            }
        }
    }
}
