//------------------------------------------------------------------------------
// <copyright file="Printer.cs" company="Zebedee Mason">
//     Copyright (c) 2017 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace QuestionMarker
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal class Printer : CSharpSyntaxWalker
    {
        private readonly SemanticModel _model;

        private readonly NLog.ILogger _logger;

        public Printer(SemanticModel model, NLog.ILogger logger)
        {
            _model = model;
            _logger = logger;
        }

        public override void Visit(SyntaxNode? node)
        {
            if (node is null)
                return;

            var message = new System.Text.StringBuilder();
            foreach (var anc in node.Ancestors())
            {
                message.Append("  ");
            }

            message.Append(node.Kind().ToString());
            message.Append(' ');

            var symbol = _model.GetSymbolInfo(node).Symbol;
            if (symbol != null)
            {
                message.Append(symbol.ToString());
            }
#if false
            else
            {
                var csNode = node as CSharpSyntaxNode;
                if (csNode != null)
                {
                    var text = csNode.ToFullString().Trim();
                    var i = text.IndexOf('\n');
                    if (i != -1)
                    {
                        text = text[..i];
                    }

                    if (text.Length > 80)
                    {
                        text = text[..80];
                    }

                    _logger.Info(text);
                }
            }
#endif

            _logger.Info(message);

            base.Visit(node);
        }

        internal static void Process(SyntaxTree tree, string filename, SemanticModel model, NLog.ILogger logger)
        {
            logger.Info(filename);
            var printer = new Printer(model, logger);
            printer.Visit(tree.GetRoot());
        }
    }
}
