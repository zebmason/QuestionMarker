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

        static void Main(string[] args)
        {
            var parse = new Parse(false, Logger("${level} ${message} ${exception}"));
            var path = @"C:\Projects\GitHub\QuestionMarker\ModelInformationResultsDto.cs";
            parse.Read(path);
        }
    }
}