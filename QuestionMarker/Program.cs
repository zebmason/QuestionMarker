using System.IO;

namespace QuestionMarker
{
    internal class Program
    {
        private static NLog.ILogger Logger(string layout)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var consoleTarget = new NLog.Targets.ColoredConsoleTarget("target1")
            {
                Layout = layout
            };
            config.AddTarget(consoleTarget);
#if DEBUG
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, consoleTarget);
#else
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
#endif

            NLog.LogManager.Configuration = config;
            var logger = NLog.LogManager.GetLogger("logger");
            return logger;
        }

        static void Recurse(string path, Parse parse)
        {
            foreach (var fileName in Directory.GetFiles(path))
            {
                if (Path.GetExtension(fileName) != ".cs")
                    continue;

                System.Console.WriteLine(fileName);
                parse.Process(fileName);
            }

            foreach (var direc in Directory.GetDirectories(path))
            {
                if (Path.GetFileName(path) == "obj")
                    continue;

                Recurse(direc, parse);
            }
        }

        static void Main(string[] args)
        {
            var parse = new Parse(false, Logger("${level} ${message} ${exception}"));
            Recurse(args[0], parse);
        }
    }
}