using System;
using System.Collections.Generic;

namespace TW.Vault.Scaffold
{
    public partial class NotificationRequestTarget
    {
        public long Id { get; set; }
        public int SmsId { get; set; }
        public long NotificationId { get; set; }

        public NotificationRequest Notification { get; set; }
        public NotificationPhoneNumber Sms { get; set; }
    }
}
