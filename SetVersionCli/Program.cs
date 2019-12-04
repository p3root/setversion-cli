using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace SetVersionCli
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine($"Invalid params");
                return;
            }


            string workingDir = Environment.CurrentDirectory;

            if (args.Length >= 2) {
                workingDir = args[1];
            }

            SetVersion(Version.Parse(args[0]), workingDir);
        }


        internal static int SetVersion(Version version, string workingDirectory)
        {
            Console.WriteLine($"Set assembly version to {version} in {workingDirectory}");
            DirectoryIterator.IterateProjectFiles(workingDirectory, (file, document, project) =>
            {
                if (file.EndsWith("csproj"))
                {
                    var props = project.FirstChild;

                    var versionProp = props.SelectNodes("Version");
                    var packageVerson = props.SelectNodes("PackageVersion");

                    if (packageVerson.Count == 1)
                    {
                        packageVerson[0].InnerText = version.ToString();
                    }

                    if (versionProp.Count == 1)
                    {
                        versionProp[0].InnerText = version.ToString();
                    }
                    else
                    {
                        var node = document.CreateNode(XmlNodeType.Element, "Version", null);
                        node.InnerText = version.ToString();

                        props.AppendChild(node);
                    }
                }
                else if (file.EndsWith("nuspec"))
                {
                    var props = project.FirstChild;

                    XmlNode versionNode = null;

                    foreach (var n in props.ChildNodes)
                    {
                        if (n is XmlNode node && node.Name == "version")
                        {
                            versionNode = node;
                            break;
                        }
                    }

                    if (versionNode != null)
                    {
                        versionNode.InnerText = version.ToString();
                    }
                }

                using (var w = new StreamWriter(file))
                    document.Save(w);
            });
            return 0;
        }
    }
}
