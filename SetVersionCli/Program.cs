using System;
using System.IO;
using System.Linq;
using System.Xml;
using PowerArgs;

namespace SetVersionCli
{
    public class SetVersionParams
    {
        [ArgRequired(PromptIfMissing = true)]
        [ArgShortcut("v")]
        [ArgPosition(0)]
        public string Version { get; set; }

        [ArgShortcut("d")]
        [ArgPosition(1)] 
        public string WorkingDirectory { get; set; } = Environment.CurrentDirectory;
        
        [ArgShortcut("cs")]
        [ArgPosition(2)]
        [ArgDefaultValue(true)]
        public bool UpdateCsProjFiles { get; set; }

        [ArgShortcut("nu")]
        [ArgPosition(3)]
        [ArgDefaultValue(true)]
        public bool UpdateNuSpecFiles { get; set; }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            var parsed = Args.Parse<SetVersionParams>(args);

            SetVersion(parsed);
        }


        internal static int SetVersion(SetVersionParams args)
        {
            var version = Version.Parse(args.Version);
            var workingDirectory = args.WorkingDirectory;

            Console.WriteLine($"Set assembly version to {version} in {workingDirectory}");
            DirectoryIterator.IterateProjectFiles(workingDirectory, (file, document, project) =>
            {
                if (file.EndsWith("csproj") && args.UpdateCsProjFiles)
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
                else if (file.EndsWith("nuspec") && args.UpdateNuSpecFiles)
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
