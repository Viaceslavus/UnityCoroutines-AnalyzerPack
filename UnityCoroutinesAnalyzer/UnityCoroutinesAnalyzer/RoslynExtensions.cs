using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityCoroutinesAnalyzer
{
    public static class RoslynExtensions
    {
        public static TResultSyntax FindChildNode<TResultSyntax>(this CSharpSyntaxNode expression, Predicate<SyntaxNode> predicate = null) where TResultSyntax : CSharpSyntaxNode
        {
            int? TryParseSyntaxToEnum()
            {
                string typeName = typeof(TResultSyntax).Name;
                int charToDeleteCount = "Syntax".Length;
                object syntaxKind = Enum.Parse(typeof(SyntaxKind), typeName.Remove(typeName.Length - (charToDeleteCount + 1)));
                return (int?)syntaxKind ?? throw new InvalidOperationException("Unable to parse the type automaticaly. Try to pass a predicate explicitly");
            }

            return expression.ChildNodes().Where(n => predicate == null ? n.IsKind((SyntaxKind)TryParseSyntaxToEnum()) : predicate(n)).First() as TResultSyntax;
        }

        public static TResultSyntax FindChildNodeWithSyntaxKind<TResultSyntax>(this SyntaxNode expression, SyntaxKind syntaxKind) where TResultSyntax : SyntaxNode
        {
            return expression.ChildNodes().Where(n => n.IsKind(syntaxKind)).FirstOrDefault() as TResultSyntax;
        }

        public static void ReportNewDiagnostic(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor rule, params object[] args)
        {
            context.ReportDiagnostic(Diagnostic.Create(rule, context.Node.GetLocation(), args));
        }

        public static void ReportDiagnosticWithLocation(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor rule, Location location, params object[] args)
        {
            context.ReportDiagnostic(Diagnostic.Create(rule, location, args));
        }
    }
}
