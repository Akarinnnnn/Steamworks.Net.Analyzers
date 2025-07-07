using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steamworks.NET.Analyzers
{
	internal static class AsyncAnalysisHelpers
	{
		public struct MatchResult
		{
			public bool? IsKindMatch { get; set; }
			public bool IsTypeMatch { get; set; }
			public string? ExpectedResultTypeMetadataName { get; set; }
		}

		public static MatchResult MatchCallback(ITypeSymbol asyncResultType, IMethodSymbol issuerMethod, IMethodSymbol setMethod)
		{
			Type? asyncResultTypeExpected = null;
			bool? kindMatch = null;
			foreach (var attr in issuerMethod.GetAttributes())
			{
				INamedTypeSymbol attributeClass = attr.AttributeClass;

				if (attributeClass.ContainingNamespace?.MetadataName != MetadataNames.SteamworksNamespace)
					continue;

				if (attributeClass.MetadataName != MetadataNames.GenericCallback)
				{
					if (attr.AttributeClass.MetadataName == MetadataNames.CallResultAttribute)
						kindMatch = false;
					else
						continue;
				}

				asyncResultTypeExpected = VerifyAsyncAttribute(attr);
				if (asyncResultTypeExpected is null)
					continue;

				kindMatch = true;
				break;
			}

			if (asyncResultTypeExpected is null)
				return new MatchResult
				{
					IsTypeMatch = false,
					IsKindMatch = kindMatch,
					ExpectedResultTypeMetadataName = null
				};

			return new MatchResult
			{
				IsTypeMatch = asyncResultType.MetadataName == asyncResultTypeExpected.FullName,
				IsKindMatch = kindMatch,
				ExpectedResultTypeMetadataName = asyncResultTypeExpected.FullName
			};
		}

		public static MatchResult MatchCallResult(ITypeSymbol asyncResultType, IMethodSymbol issuerMethod, IMethodSymbol createOrConstructorMethod)
		{
			Type? asyncResultTypeExpected = null;
			bool? kindMatch = null;
			foreach (var attr in issuerMethod.GetAttributes())
			{
				INamedTypeSymbol attributeClass = attr.AttributeClass;

				if (attributeClass.ContainingNamespace?.MetadataName != MetadataNames.SteamworksNamespace)
					continue;

				if (attributeClass.MetadataName != MetadataNames.GenericCallResult)
				{
					if (attr.AttributeClass.MetadataName == MetadataNames.CallbackAttribute)
						kindMatch = false;
					else
						continue;
				}

				asyncResultTypeExpected = VerifyAsyncAttribute(attr);
				if (asyncResultTypeExpected is null)
					continue;

				kindMatch = true;
				break;
			}

			if (asyncResultTypeExpected is null)
				return new MatchResult
				{
					IsTypeMatch = false,
					IsKindMatch = kindMatch,
					ExpectedResultTypeMetadataName = null
				};

			return new MatchResult
			{
				IsTypeMatch = asyncResultType.MetadataName == asyncResultTypeExpected.FullName,
				IsKindMatch = kindMatch,
				ExpectedResultTypeMetadataName = asyncResultTypeExpected.FullName
			};
		}

		private static Type? VerifyAsyncAttribute(AttributeData attr)
		{
			if (attr.NamedArguments.Length != 0 || attr.ConstructorArguments.Length != 1)
				return null;

			var ctorArg0 = attr.ConstructorArguments[0];
			if (ctorArg0.Value is not Type typeArg)
				return null;

			if (typeArg.Namespace != "Steamworks" && typeArg.FullName.EndsWith("_t"))
				return null;

			return typeArg;
		}
	}
}
