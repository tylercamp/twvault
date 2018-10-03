using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.Scaffolding.NavigationPropertyFixup
{
    // https://github.com/aspnet/EntityFrameworkCore/blob/a3f0a78c41fe209924f8fb39fc77c421236f1bbe/src/EFCore.Design/Scaffolding/Internal/RelationalScaffoldingModelFactory.cs
    class FixedRelationalScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        public FixedRelationalScaffoldingModelFactory([NotNull] IOperationReporter reporter, [NotNull] ICandidateNamingService candidateNamingService, [NotNull] IPluralizer pluralizer, [NotNullAttribute] ICSharpUtilities cSharpUtilities, [NotNull] IScaffoldingTypeMapper scaffoldingTypeMapper)
            : base(reporter, candidateNamingService, pluralizer, cSharpUtilities, scaffoldingTypeMapper)
        {
        }

        protected override string GetPropertyName([NotNull] DatabaseColumn column)
        {
            return base.GetPropertyName(column);
        }

        protected override void AddNavigationProperties([NotNull] IMutableForeignKey foreignKey)
        {
            base.AddNavigationProperties(foreignKey);
        }
    }
}
