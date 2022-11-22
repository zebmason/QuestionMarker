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

        private readonly string _enable = "#nullable enable\n";

        private string? _filePath;

        private string? _source;

        private SyntaxTree? _tree;

        public Parse(bool writeAST, NLog.ILogger logger)
        {
            _logger = logger;
            _writeAST = writeAST;
            _libs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        }

        public void Read(string path)
        {
            _filePath = path;
            CreateTrees();
            Process();
        }

        private void CreateTrees()
        {
            var options = new CSharpParseOptions(LanguageVersion.CSharp8, DocumentationMode.Parse, SourceCodeKind.Regular, new List<string>());
            _source = System.IO.File.ReadAllText(_filePath);
            var contents = _enable + _source;
            _tree = CSharpSyntaxTree.ParseText(contents, options, _filePath);
        }

        private void Process()
        {
            var codes = new List<string> { "CS8618", "CS8625" };
            var compilation = CSharpCompilation.Create("Compilation", syntaxTrees: new List<SyntaxTree> { _tree }, references: _libs);
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
                var model = compilation.GetSemanticModel(_tree);
                Printer.Process(_tree, _filePath, model, _logger);
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
                    var index = loc - _enable.Length - 1;
                    _source = _source.Insert(index, "?");
                }

                File.WriteAllText(_filePath, _source);
            }
        }
    }
}
