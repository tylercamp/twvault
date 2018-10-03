using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.Scaffolding.NavigationPropertyFixup
{
    // https://github.com/aspnet/EntityFrameworkCore/blob/a3f0a78c41fe209924f8fb39fc77c421236f1bbe/src/EFCore.Design/Scaffolding/Internal/CandidateNamingService.cs
    class FixedCandidateNamingService : CandidateNamingService
    {
        public override string GetDependentEndCandidateNavigationPropertyName(IForeignKey foreignKey)
        {
            return base.GetDependentEndCandidateNavigationPropertyName(foreignKey);
        }

        public override string GetPrincipalEndCandidateNavigationPropertyName(IForeignKey foreignKey, string dependentEndNavigationPropertyName)
        {
            return base.GetPrincipalEndCandidateNavigationPropertyName(foreignKey, dependentEndNavigationPropertyName);
        }
    }
}
