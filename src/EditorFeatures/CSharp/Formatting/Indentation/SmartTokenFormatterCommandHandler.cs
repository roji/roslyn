﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Indentation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editor.Implementation.Formatting.Indentation;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Formatting.Rules;
using Microsoft.CodeAnalysis.Indentation;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Roslyn.Utilities;
using VSCommanding = Microsoft.VisualStudio.Commanding;

namespace Microsoft.CodeAnalysis.Editor.CSharp.Formatting.Indentation
{
    [Export(typeof(VSCommanding.ICommandHandler))]
    [ContentType(ContentTypeNames.CSharpContentType)]
    [Name(PredefinedCommandHandlerNames.Indent)]
    [Order(After = PredefinedCommandHandlerNames.Rename)]
    [Order(Before = PredefinedCommandHandlerNames.Completion)]
    [Order(Before = PredefinedCompletionNames.CompletionCommandHandler)]
    internal class SmartTokenFormatterCommandHandler :
        AbstractSmartTokenFormatterCommandHandler
    {
        [ImportingConstructor]
        public SmartTokenFormatterCommandHandler(
            ITextUndoHistoryRegistry undoHistoryRegistry,
            IEditorOperationsFactoryService editorOperationsFactoryService)
            : base(undoHistoryRegistry, editorOperationsFactoryService)
        {
        }

        protected override ISmartTokenFormatter CreateSmartTokenFormatter(OptionSet optionSet, IEnumerable<AbstractFormattingRule> formattingRules, SyntaxNode root)
        {
            return new CSharpSmartTokenFormatter(optionSet, formattingRules, (CompilationUnitSyntax)root);
        }

        protected override bool UseSmartTokenFormatter(SyntaxNode root, TextLine line, IEnumerable<AbstractFormattingRule> formattingRules, OptionSet options, CancellationToken cancellationToken)
        {
            return CSharpIndentationService.ShouldUseSmartTokenFormatterInsteadOfIndenter(formattingRules, (CompilationUnitSyntax)root, line, options, cancellationToken);
        }

        protected override IEnumerable<AbstractFormattingRule> GetFormattingRules(Document document, int position)
        {
            var workspace = document.Project.Solution.Workspace;
            var formattingRuleFactory = workspace.Services.GetService<IHostDependentFormattingRuleFactoryService>();
            return formattingRuleFactory.CreateRule(document, position).Concat(Formatter.GetDefaultFormattingRules(document));
        }

        protected override bool IsInvalidToken(SyntaxToken token)
        {
            // invalid token to be formatted
            return token.IsKind(SyntaxKind.None) ||
                   token.IsKind(SyntaxKind.EndOfDirectiveToken) ||
                   token.IsKind(SyntaxKind.EndOfFileToken);
        }
    }
}
