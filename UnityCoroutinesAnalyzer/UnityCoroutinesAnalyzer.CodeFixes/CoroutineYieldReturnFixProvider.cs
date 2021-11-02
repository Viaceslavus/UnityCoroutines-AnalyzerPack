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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityCoroutinesAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CoroutineYieldReturnFixProvider)), Shared]
    public class CoroutineYieldReturnFixProvider : CodeFixProvider
    {
        public const string DiagnosticId = CoroutineYieldAnalyzer.DiagnosticId;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public async sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ExpressionStatementSyntax>().First();
            new System.IO.StreamWriter(@"C:\aspnetcoreprojects\debug.txt").WriteLine($"hello {declaration}");
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedDocument: token => AddYieldReturn(context.Document, declaration, token),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private YieldStatementSyntax NewYieldStatement()
        {
            return YieldStatement(
                    SyntaxKind.YieldReturnStatement,
                    Token(SyntaxKind.YieldKeyword),
                    Token(SyntaxKind.YieldReturnStatement),
                    LiteralExpression(SyntaxKind.NullKeyword),
                    Token(SyntaxKind.SemicolonToken)
                );
        }

        private SyntaxNode CreateWhileBlock(WhileStatementSyntax declaration)
        {
            ExpressionStatementSyntax existingStatement = declaration.FindChildNodeWithSyntaxKind<ExpressionStatementSyntax>(SyntaxKind.ExpressionStatement);
            BlockSyntax whileBlock = Block(existingStatement, NewYieldStatement());
            SyntaxNode newRoot = declaration.ReplaceNode(existingStatement, whileBlock);
            return newRoot;
        }

        private Task<Document> AddYieldReturn(Document document, ExpressionStatementSyntax declaration, CancellationToken token)
        {
            /*BlockSyntax whileBlock = declaration.FindChildNodeWithSyntaxKind<BlockSyntax>(SyntaxKind.Block);
            if(whileBlock == null)
            {
                return Task.FromResult(
                    document.WithSyntaxRoot(
                        CreateWhileBlock(declaration)));
            }
            BlockSyntax newWhileBlock = whileBlock.AddStatements(NewYieldStatement());
            newWhileBlock.WriteTo(new StreamWriter(@"C:\aspnetcoreprojects\debug.txt"));
            SyntaxNode newRoot = declaration.ReplaceNode(whileBlock, newWhileBlock);*/
            return Task.FromResult(document);
        }
    }
}
