//------------------------------------------------------------------------------
// <copyright file="Parse.cs" company="Zebedee Mason">
//     Copyright (c) 2016-2017 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace QuestionMarker
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Parse
    {
        private readonly NLog.ILogger _logger;

        private readonly bool _writeAST;

        private readonly List<PortableExecutableReference> _libs = new();

        private readonly CSharpParseOptions _options = new CSharpParseOptions(LanguageVersion.CSharp8, DocumentationMode.Parse, SourceCodeKind.Regular, new List<string>());

        private readonly string _enable = "#nullable enable\n";

        public Parse(bool writeAST, NLog.ILogger logger)
        {
            _logger = logger;
            _writeAST = writeAST;
            _libs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        }

        public void Process(string filePath)
        {
            var source = File.ReadAllText(filePath!);
            var contents = _enable + source;
            var tree = CSharpSyntaxTree.ParseText(contents, _options, filePath);
            var codes = new List<string> { "CS8618", "CS8625" };
            var compilation = CSharpCompilation.Create("Compilation", syntaxTrees: new List<SyntaxTree> { tree! }, references: _libs);
            var diags = compilation.GetDiagnostics();
            var nullables = new List<Diagnostic>();
            foreach (var diag in diags)
            {
                if (codes.Contains(diag.Id))
                {
                    nullables.Add(diag);
                    _logger.Debug(diag.ToString());
                }
            }

            if (_writeAST)
            {
                var model = compilation.GetSemanticModel(tree!);
                Printer.Process(tree!, filePath, model, _logger);
            }
            else
            {
                if (!nullables.Any())
                    return;

                var ordered = new List<int>();
                foreach (var diag in nullables)
                {
                    ordered.Add(diag.Location.SourceSpan.Start);
                }

                ordered.Sort();
                ordered.Reverse();

                foreach (var loc in ordered)
                {
                    contents = contents.Insert(loc - 1, "?");
                }

                File.WriteAllText(filePath, source[_enable.Length..]);
            }
        }
    }
}
