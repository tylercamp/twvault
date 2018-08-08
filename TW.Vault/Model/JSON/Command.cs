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
        public long? SourceVillageId { get; set; }
        [Required]
        public long? SourcePlayerId { get; set; }
        [Required]
        public long? TargetVillageId { get; set; }
        [Required]
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
    }
}
