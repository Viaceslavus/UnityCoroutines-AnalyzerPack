using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityCoroutinesAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnityCoroutinesAnalyzerCodeFixProvider)), Shared]
    public class UnityCoroutinesAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(UnityCoroutinesAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ExpressionStatementSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedDocument: token => FixCoroutineInvocation(context.Document, declaration, token),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private ArgumentSyntax BuildNewArgument(string identifierName)
        {
            return Argument(
                    InvocationExpression(
                        IdentifierName("nameof"),
                        ArgumentList(
                            new SeparatedSyntaxList<ArgumentSyntax>()
                            .Add(Argument(
                                    IdentifierName(
                                        Identifier(identifierName)
                                        )
                                    )
                                )
                            )
                        )
                    );
        }

        private async Task<Document> FixCoroutineInvocation(Document document, ExpressionStatementSyntax methodInvoke, CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax methodInvocation = methodInvoke.FindChildNodeWithSyntaxKind<InvocationExpressionSyntax>(SyntaxKind.InvocationExpression);
            ArgumentListSyntax arguments = methodInvocation.ArgumentList;
            ArgumentSyntax firstAgument = arguments.Arguments[0];

            ArgumentSyntax newArgument = null;
            SyntaxNode argumentVal = firstAgument.ChildNodes().First();

            if(argumentVal is InvocationExpressionSyntax invocationExp)
            {
                newArgument = BuildNewArgument((invocationExp.FindChildNodeWithSyntaxKind<IdentifierNameSyntax>(SyntaxKind.IdentifierName)).ToString());
            }
            else
            {
                newArgument = BuildNewArgument(firstAgument.WithoutTrivia().ToString());
            }

            ArgumentListSyntax newArguments = methodInvocation.ArgumentList.Update(arguments.OpenParenToken, arguments.Arguments.RemoveAt(0).Insert(0, newArgument), arguments.CloseParenToken);

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode newRoot = oldRoot.ReplaceNode(methodInvocation.ArgumentList, newArguments);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
