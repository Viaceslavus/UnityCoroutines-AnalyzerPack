using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

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

        public static TResultSyntax FindChildNodeWithSyntaxKind<TResultSyntax>(this CSharpSyntaxNode expression, SyntaxKind syntaxKind) where TResultSyntax : CSharpSyntaxNode
        {
            return expression.ChildNodes().Where(n => n.IsKind(syntaxKind)).First() as TResultSyntax;
        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnityCoroutinesAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UnityCoroutinesAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private readonly string[] CoroutineNames = new string[]
        {
            "StartCoroutine",
            "StopCoroutine",
            "Invoke",
            "InvokeRepeating"
        };

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeOperation, SyntaxKind.ExpressionStatement);
        }

        private bool MethodContainsBadCoroutineInvocation(InvocationExpressionSyntax invocation)
        {
            ArgumentSyntax firstParam = invocation.ArgumentList.Arguments[0];
            InvocationExpressionSyntax firstParamInvocation = firstParam.FindChildNodeWithSyntaxKind<InvocationExpressionSyntax>(SyntaxKind.InvocationExpression);
            if(firstParamInvocation != null)
            {
                if(firstParamInvocation.Expression.ToString().Contains("nameof"))
                {
                    return false;
                }
            }
            return true;
        }

        private bool MethodIsCoroutineInvocation(string methodName) => CoroutineNames.Contains(methodName);

        private void ReportDiagnostic(SyntaxNodeAnalysisContext context)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private void AnalyzeOperation(SyntaxNodeAnalysisContext context)
        {
            var methodRef = (ExpressionStatementSyntax)context.Node;
            var methodInvocation = methodRef.FindChildNodeWithSyntaxKind<InvocationExpressionSyntax>(SyntaxKind.InvocationExpression);
            string methodName = methodInvocation.FindChildNodeWithSyntaxKind<IdentifierNameSyntax>(SyntaxKind.IdentifierName).ToString();

            if(MethodIsCoroutineInvocation(methodName))
            {
                if(MethodContainsBadCoroutineInvocation(methodInvocation))
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
