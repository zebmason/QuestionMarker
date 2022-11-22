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

    public class Parse
    {
        private readonly NLog.ILogger _logger; 

        private readonly Dictionary<string, IEnumerable<string>> _csfiles = new Dictionary<string, IEnumerable<string>>();

        private readonly bool _writeAST;

        private readonly Dictionary<string, SyntaxTree> _map = new Dictionary<string, SyntaxTree>();

        private readonly List<SyntaxTree> _trees = new List<SyntaxTree>();

        private readonly List<PortableExecutableReference> _libs = new List<PortableExecutableReference>();

        public Parse(bool writeAST, NLog.ILogger logger)
        {
            _logger = logger;
            _writeAST = writeAST;
        }

        public void ReadFile(string fileName, IEnumerable<string> compileDefinitions)
        {
            _csfiles[fileName] = compileDefinitions;
        }

        private void CreateTrees()
        {
            foreach (var csfile in _csfiles)
            {
                var options = new CSharpParseOptions(LanguageVersion.Default, DocumentationMode.Parse, SourceCodeKind.Regular, csfile.Value);
                var contents = System.IO.File.ReadAllText(csfile.Key);
                var tree = CSharpSyntaxTree.ParseText(contents, options, csfile.Key);
                _trees.Add(tree);
                _map[csfile.Key] = tree;
            }
        }

        private void References(List<string> dlls)
        {
            for (int i = 0; i < dlls.Count; ++i)
            {
                _libs.Add(MetadataReference.CreateFromFile(dlls[i]));
            }

            _libs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        }

        private void Process()
        {
            var compilation = CSharpCompilation.Create("Compilation", syntaxTrees: _trees, references: _libs);
            var diags = compilation.GetDiagnostics();
            foreach (var diag in diags)
            {
                _logger.Debug(diag.ToString());
            }

            foreach (var csfile in _map.Keys)
            {
                var tree = _map[csfile];
                var model = compilation.GetSemanticModel(tree);

                if (_writeAST)
                {
                    Printer.Process(tree, csfile, model, _logger);
                }
                else
                {
                }
            }
        }

        public void Finalise(List<string> dlls)
        {
            CreateTrees();

            References(dlls);

            Process();
        }
    }
}
