using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFrameworkCore.Scaffolding.NavigationPropertyFixup
{
    // https://github.com/aspnet/EntityFrameworkCore/blob/a3f0a78c41fe209924f8fb39fc77c421236f1bbe/src/EFCore.Design/Scaffolding/Internal/CandidateNamingService.cs
    class FixedCandidateNamingService : CandidateNamingService
    {
        public override string GetDependentEndCandidateNavigationPropertyName(IForeignKey foreignKey)
        {
            var bestName = foreignKey.Properties.FirstOrDefault(p => !p.Name.Contains("WorldId"))?.Name;
            if (bestName != null && bestName != "Id" && bestName.EndsWith("Id"))
                bestName = bestName.Substring(0, bestName.Length - 2);

            var baseName = base.GetDependentEndCandidateNavigationPropertyName(foreignKey);
            return bestName ?? baseName;
        }

        public override string GetPrincipalEndCandidateNavigationPropertyName(IForeignKey foreignKey, string dependentEndNavigationPropertyName)
        {
            var baseResult = base.GetPrincipalEndCandidateNavigationPropertyName(foreignKey, dependentEndNavigationPropertyName);
            return baseResult;
        }
    }
}
