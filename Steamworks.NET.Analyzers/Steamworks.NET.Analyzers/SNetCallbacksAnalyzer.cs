using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;


namespace Steamworks.NET.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SNetCallbacksAnalyzer : DiagnosticAnalyzer
	{
		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.SNet0001Title), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.SNet0001MessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.SNet0001Description), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Steamworks-Callbacks";

		private static readonly DiagnosticDescriptor RetrievedByCallbackSNet0001 = new("SNET0001", Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
		private static readonly DiagnosticDescriptor RetrievedByCallResultSNet0002 = new(
			"SNET0002",
			new LocalizableResourceString(nameof(Resources.SNet0002Title), Resources.ResourceManager, typeof(Resources)),
			new LocalizableResourceString(nameof(Resources.SNet0002MessageFormat), Resources.ResourceManager, typeof(Resources)),
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);
		// todo localize them
		private static readonly DiagnosticDescriptor CallResultTypeMismatchSNet0003 = new(
			"SNET0003",
			"Call result type mismatch.",
			"Should use \"CallResult<{0}>\" or \"CallResultAwaitable<{0}>\"(if enabled) to receive call result from this steam method",
			Category,
			DiagnosticSeverity.Warning,
			true
		);
		private static readonly DiagnosticDescriptor UnacceptableResultSNet0101 = new(
			"SNET0101",
			"Trying to receive a non-existent callback result.",
			"Type \"{0}\" is not acceptable for Steamworks.NET callback system.",
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: ""
		);



		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get
			{ 
				return ImmutableArray.Create(
					RetrievedByCallbackSNet0001,
					RetrievedByCallResultSNet0002,
					UnacceptableResultSNet0101
				);
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(AnalyzeSetMethodCallFor0001, SyntaxKind.Block);
			context.RegisterSyntaxNodeAction(AnalyzeSteamInterfaceAccesses, SyntaxKind.SimpleMemberAccessExpression);
			// context.RegisterSyntaxNodeAction(AnalyzeCreateMethodCallFor0001, SyntaxKind.Block);
			// todo context.RegisterSyntaxNodeAction(AnalyzeBlockFor0002, SyntaxKind.Block);
		}

		private void AnalyzeSteamInterfaceAccesses(SyntaxNodeAnalysisContext context)
		{
			var memberAccessSyntax = (MemberAccessExpressionSyntax)context.Node;

			// exclude Steamworks internal methods
		}

		private static void AnalyzeSetMethodCallFor0001(SyntaxNodeAnalysisContext context)
		{
			// analyze code block that may have invocations to CallResult
			BlockSyntax analyzingBlock = (BlockSyntax)context.Node;


			context.CancellationToken.ThrowIfCancellationRequested();
			// analyze `xxx.Set(h)` or `xxx.Set(h, method)`
			IEnumerable<InvocationExpressionSyntax> invocationExpressions = analyzingBlock.DescendantNodes()
				.OfType<InvocationExpressionSyntax>();

			var callResultSetInvocations = invocationExpressions
				.Where(invocation =>
				{
					var memberAccesses = invocation.Expression.DescendantNodes().Where(n => n.IsKind(SyntaxKind.SimpleMemberAccessExpression));
					foreach (MemberAccessExpressionSyntax memberAccess in memberAccesses.Cast<MemberAccessExpressionSyntax>())
					{
						if (memberAccess.Name.Identifier.Text == "Set")
						{
							var setSymbol = context.SemanticModel.GetDeclaredSymbol(memberAccess.Name);

							if (setSymbol.Kind != SymbolKind.Method)
								continue;

							var setMethodSymbol = (IMethodSymbol)setSymbol;

							if (setMethodSymbol.ContainingType is not INamedTypeSymbol declTypeSymbol)
								continue;

							//if (declTypeSymbol.MetadataName != MetadataNames.GenericCallResult)
							//{
							//	if (declTypeSymbol.MetadataName == MetadataNames.GenericCallback)
							//	{
							//		var callbackTypeSymbol = declTypeSymbol;
							//		Debug.Assert(callbackTypeSymbol.Arity == 1, "Callback class named 'Callback`1' but generic args count is not 1.");
							//		var callresultResultTypeSymbol = callbackTypeSymbol.TypeArguments.Single();

							//		string callresultName = callresultResultTypeSymbol.Name;
							//		bool nameEmpty = string.IsNullOrEmpty(callresultName);

							//		if (nameEmpty)
							//		{
							//			Diagnostic.Create(UnacceptableResultSNet0101, callresultResultTypeSymbol.Locations.Single(), "Unnamed type");
							//			callresultName = "Unnamed result which should be an error";
							//		}

							//		var diagnosticSNet0001 = Diagnostic.Create(RetrieveByCallbackSNet0001, memberAccess.GetLocation(), callresultName);
							//		context.ReportDiagnostic(diagnosticSNet0001);
							//	}
							//	else
							//	{
							//		continue;
							//	}
							//}

							return true;
						}

						return false;
						//var lastSyntax = memberAccess.ChildNodes().Last();
						//if (lastSyntax is IdentifierNameSyntax identifierLast)
						//{
						//	identifierLast.
						//}
					}

					return true; // todo
				});

			var steamMethodInvocations = invocationExpressions.Where(invocation =>
			{
				var memberAccesses = invocation.Expression.DescendantNodes().Where(n => n.IsKind(SyntaxKind.SimpleMemberAccessExpression));
				foreach (MemberAccessExpressionSyntax memberAccess in memberAccesses.Cast<MemberAccessExpressionSyntax>())
				{
					if (memberAccess.Name.Identifier.Text == "Set")
					{
						var setSymbol = context.SemanticModel.GetDeclaredSymbol(memberAccess.Name);

						if (setSymbol.Kind != SymbolKind.Method)
							continue;

						var setMethodSymbol = (IMethodSymbol)setSymbol;

						if (setMethodSymbol.ContainingType is not INamedTypeSymbol declTypeSymbol)
							continue;


						return true;
					}
				}

				return false;
			});

			foreach (var crSetInvocation in callResultSetInvocations)
			{
				ArgumentListSyntax args;
				try
				{
					args = (ArgumentListSyntax)crSetInvocation.ChildNodes().Single(n => n.IsKind(SyntaxKind.ArgumentList));

				}
				catch (InvalidOperationException)
				{
					// method signature not match
					continue;
				}

				// args.
				AsyncAnalysisHelpers.MatchCallResult(context.SemanticModel.GetSymbolInfo(), (IMethodSymbol)context.SemanticModel.GetSymbolInfo(args.Parent, context.CancellationToken).Symbol);

			}
		}

		private static bool CheckShouldExcludeFromBlockAnalysis(BlockSyntax block, SyntaxNodeAnalysisContext context)
		{
			// find owning method
			SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(block);
			if (symbolInfo.Symbol is ISymbol symbol)
			{
				if (symbol is not IMethodSymbol methodSymbol)
					return false;

				INamespaceSymbol checkingNamespace = methodSymbol.ContainingNamespace;

				return checkingNamespace.Name == MetadataNames.SteamworksNamespace;
			}
			else
			{
				return true;
			}
		}
	}
}
