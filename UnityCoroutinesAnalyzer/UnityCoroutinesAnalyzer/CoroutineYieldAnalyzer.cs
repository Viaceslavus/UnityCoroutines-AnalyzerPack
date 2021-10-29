using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Collections;

namespace UnityCoroutinesAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CoroutineYieldAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CoroutineYieldAnalyzer";
        internal static readonly LocalizableString Title = "CoroutineYieldAnalyzer Title";
        internal static readonly LocalizableString MessageFormat = "CoroutineYieldAnalyzer '{0}'";
        internal const string Category = "CoroutineYieldAnalyzer Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeOperation, SyntaxKind.MethodDeclaration);
        }

        System.Collections.IEnumerator Some()
        {
            while(true)
            {
                int s = 0;
                Console.WriteLine("fewe");
                yield return null;
            }
        }

        private static bool IsCoroutineMethod(MethodDeclarationSyntax method)
        {
            var idName = method.FindChildNodeWithSyntaxKind<IdentifierNameSyntax>(SyntaxKind.IdentifierName);
            if(idName != null)
            {
                if(idName.ToString() == typeof(IEnumerator).Name)
                {
                    return true;
                }
            }
            var qualifiedName = method.FindChildNodeWithSyntaxKind<QualifiedNameSyntax>(SyntaxKind.QualifiedName);
            if(qualifiedName != null)
            {
                if(qualifiedName.FindChildNodeWithSyntaxKind<IdentifierNameSyntax>(SyntaxKind.IdentifierName)?.ToString() == typeof(IEnumerator).Name)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool WhileBlockHasNoYieldReturn(WhileStatementSyntax whileStatement)
        {
            var whileBlock = whileStatement.FindChildNodeWithSyntaxKind<BlockSyntax>(SyntaxKind.Block);
            return (whileBlock.FindChildNodeWithSyntaxKind<YieldStatementSyntax>(SyntaxKind.YieldReturnStatement) ??
                whileBlock.FindChildNodeWithSyntaxKind<YieldStatementSyntax>(SyntaxKind.YieldBreakStatement)) == null;
        }

        private void AnalyzeOperation(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;
            var block = method.FindChildNodeWithSyntaxKind<BlockSyntax>(SyntaxKind.Block);

            if(IsCoroutineMethod(method))
            {
                var whileStatement = block.FindChildNodeWithSyntaxKind<WhileStatementSyntax>(SyntaxKind.WhileStatement);
                if(whileStatement != null)
                {
                    if(WhileBlockHasNoYieldReturn(whileStatement))
                    {
                        context.ReportDiagnosticWithLocation(Rule, whileStatement.GetLocation());
                    }
                }
            }
        }
    }
}
