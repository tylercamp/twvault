using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSON = TW.Vault.Model.JSON;
using static TW.Vault.Model.Convert.ConvertUtil;

namespace TW.Vault.Model.Convert
{
    public static class BuildingConvert
    {
        public static JSON.BuildingLevels ToJSON(this Scaffold.ReportBuilding reportBuildings)
            => ReportBuildingToJson(reportBuildings);

        public static Scaffold.ReportBuilding ToModel(this JSON.BuildingLevels jsonBuildings, short worldId, Scaffold.ReportBuilding existingBuildings = null, Scaffold.VaultContext context = null)
            => JsonToReportBuilding(jsonBuildings, worldId, existingBuildings, context);

        public static JSON.BuildingLevels ReportBuildingToJson(Scaffold.ReportBuilding reportBuildings)
        {
            if (reportBuildings == null)
                return null;

            var result = new JSON.BuildingLevels();

            AddIfNotNull(result, "snob", reportBuildings.Snob);
            AddIfNotNull(result, "barracks", reportBuildings.Barracks);
            AddIfNotNull(result, "church", reportBuildings.Church);
            AddIfNotNull(result, "stone", reportBuildings.Stone);
            AddIfNotNull(result, "farm", reportBuildings.Farm);
            AddIfNotNull(result, "main", reportBuildings.Main);
            AddIfNotNull(result, "hide", reportBuildings.Hide);
            AddIfNotNull(result, "iron", reportBuildings.Iron);
            AddIfNotNull(result, "market", reportBuildings.Market);
            AddIfNotNull(result, "place", reportBuildings.Place);
            AddIfNotNull(result, "smith", reportBuildings.Smith);
            AddIfNotNull(result, "stable", reportBuildings.Stable);
            AddIfNotNull(result, "status", reportBuildings.Statue);
            AddIfNotNull(result, "wood", reportBuildings.Wood);
            AddIfNotNull(result, "wall", reportBuildings.Wall);
            AddIfNotNull(result, "storage", reportBuildings.Storage);
            AddIfNotNull(result, "watchtower", reportBuildings.Watchtower);
            AddIfNotNull(result, "garage", reportBuildings.Garage);

            return result;
        }

        public static JSON.BuildingLevels CurrentBuildingToJson(Scaffold.CurrentBuilding currentBuildings)
        {
            if (currentBuildings == null)
                return null;

            var result = new JSON.BuildingLevels();

            AddIfNotNull(result, "snob", currentBuildings.Snob);
            AddIfNotNull(result, "barracks", currentBuildings.Barracks);
            AddIfNotNull(result, "church", currentBuildings.Church);
            AddIfNotNull(result, "stone", currentBuildings.Stone);
            AddIfNotNull(result, "farm", currentBuildings.Farm);
            AddIfNotNull(result, "main", currentBuildings.Main);
            AddIfNotNull(result, "hide", currentBuildings.Hide);
            AddIfNotNull(result, "iron", currentBuildings.Iron);
            AddIfNotNull(result, "market", currentBuildings.Market);
            AddIfNotNull(result, "place", currentBuildings.Place);
            AddIfNotNull(result, "smith", currentBuildings.Smith);
            AddIfNotNull(result, "stable", currentBuildings.Stable);
            AddIfNotNull(result, "status", currentBuildings.Statue);
            AddIfNotNull(result, "wood", currentBuildings.Wood);
            AddIfNotNull(result, "wall", currentBuildings.Wall);
            AddIfNotNull(result, "storage", currentBuildings.Storage);
            AddIfNotNull(result, "watchtower", currentBuildings.Watchtower);
            AddIfNotNull(result, "garage", currentBuildings.Garage);

            return result;
        }

        public static Scaffold.ReportBuilding JsonToReportBuilding(JSON.BuildingLevels buildingLevels, short worldId, Scaffold.ReportBuilding existingBuildings = null, Scaffold.VaultContext context = null)
        {
            if (buildingLevels == null || buildingLevels.Count == 0)
            {
                if (existingBuildings != null && context != null)
                    context.Remove(existingBuildings);

                return null;
            }

            Scaffold.ReportBuilding result;
            if (existingBuildings != null)
            {
                result = existingBuildings;
            }
            else
            {
                result = new Scaffold.ReportBuilding();
                result.WorldId = worldId;
                context.Add(result);
            }

            result.Barracks   = GetOrNull(buildingLevels, "barracks");
            result.Church     = GetOrNull(buildingLevels, "church");
            result.Farm       = GetOrNull(buildingLevels, "farm");
            result.Garage     = GetOrNull(buildingLevels, "garage");
            result.Hide       = GetOrNull(buildingLevels, "hide");
            result.Iron       = GetOrNull(buildingLevels, "iron");
            result.Main       = GetOrNull(buildingLevels, "main");
            result.Market     = GetOrNull(buildingLevels, "market");
            result.Place      = GetOrNull(buildingLevels, "place");
            result.Smith      = GetOrNull(buildingLevels, "smith");
            result.Snob       = GetOrNull(buildingLevels, "snob");
            result.Stable     = GetOrNull(buildingLevels, "stable");
            result.Statue     = GetOrNull(buildingLevels, "statue");
            result.Stone      = GetOrNull(buildingLevels, "stone");
            result.Storage    = GetOrNull(buildingLevels, "storage");
            result.Wall       = GetOrNull(buildingLevels, "wall");
            result.Watchtower = GetOrNull(buildingLevels, "watchtower");
            result.Wood       = GetOrNull(buildingLevels, "wood");

            // ?
            //result.FirstChurch = GetOrNull(buildingLevels, "First church");

            return result;
        }
    }
}
