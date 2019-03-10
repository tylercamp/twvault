using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TW.Vault.Model.JSON
{
    public enum CommandType
    {
        Attack,
        Support
    }

    public class Command
    {
        [Required]
        public long? CommandId { get; set; }
        [Required]
        public String UserLabel { get; set; }
        [Required]
        public long? SourceVillageId { get; set; }
        [Required]
        public long? SourcePlayerId { get; set; }
        [Required]
        public long? TargetVillageId { get; set; }

        public long? TargetPlayerId { get; set; }

        [Required]
        public DateTime? LandsAt { get; set; }
        [Required]
        public bool? IsReturning { get; set; }

        public Army Troops { get; set; }


        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public CommandType? CommandType { get; set; }


        [JsonConverter(typeof(StringEnumConverter))]
        public TroopType? TroopType { get; set; }

        public static bool operator ==(Command a, Command b)
        {
            bool aIsNull = Object.ReferenceEquals(a, null);
            bool bIsNull = Object.ReferenceEquals(b, null);

            if (aIsNull && bIsNull)
                return true;

            if (aIsNull != bIsNull)
                return false;

            if (a.CommandId != b.CommandId ||
                a.SourceVillageId != b.SourceVillageId ||
                a.SourcePlayerId != b.SourcePlayerId ||
                a.TargetVillageId != b.TargetVillageId ||
                a.TargetPlayerId != b.TargetPlayerId ||
                a.LandsAt != b.LandsAt ||
                a.IsReturning != b.IsReturning ||
                a.Troops != b.Troops)
                return false;

            return true;
        }

        public static bool operator !=(Command a, Command b) => !(a == b);

        public override bool Equals(object obj) => this == obj as Command;

        public override int GetHashCode() => base.GetHashCode();
    }
}
