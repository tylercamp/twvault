using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class NotificationSettings
    {
        [Required]
        public int? SendNotificationBeforeMinutes { get; set; }
    }
}
