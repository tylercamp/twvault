using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TW.Vault.Features.Notifications
{
    public static class SMS
    {
        public static void Init()
        {
            TwilioClient.Init(
                Configuration.Behavior.Notifications.TwilioClientKey,
                Configuration.Behavior.Notifications.TwilioClientSecret
            );
        }

        public static void Send(String phoneNumber, String message)
        {
            MessageResource.Create(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(Configuration.Behavior.Notifications.TwilioSourcePhoneNumber),
                body: message
            );
        }
    }
}
