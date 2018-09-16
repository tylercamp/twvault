using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TW.Vault.Model.Convert
{
    public static class NotificationConvert
    {
        public static Scaffold.NotificationRequest JsonToModel(JSON.Notification notification, Scaffold.NotificationRequest existingNotification, Scaffold.VaultContext context = null)
        {
            Scaffold.NotificationRequest result;
            if (existingNotification != null)
            {
                result = existingNotification;
            }
            else
            {
                result = new Scaffold.NotificationRequest();
                if (context != null)
                    context.Add(result);
            }

            result.EventOccursAt = notification.EventOccursAt.Value;
            result.Message = notification.Message;

            return result;
        }

        public static JSON.Notification ModelToJson(Scaffold.NotificationRequest request)
        {
            var result = new JSON.Notification();

            result.Message = request.Message;
            result.EventOccursAt = request.EventOccursAt;

            return result;
        }

        public static JSON.PhoneNumber ModelToJson(Scaffold.NotificationPhoneNumber phoneNumber)
        {
            var result = new JSON.PhoneNumber();

            result.Number = phoneNumber.PhoneNumber;
            result.Label = phoneNumber.Label;
            result.Id = phoneNumber.Id;

            return result;
        }

        public static Scaffold.NotificationUserSettings JsonToModel(JSON.NotificationSettings settings, Scaffold.NotificationUserSettings existingSettings, Scaffold.VaultContext context = null)
        {
            Scaffold.NotificationUserSettings result;
            if (existingSettings != null)
            {
                result = existingSettings;
            }
            else
            {
                result = new Scaffold.NotificationUserSettings();
                if (context != null)
                    context.Add(result);
            }

            result.NotificationHeadroom = TimeSpan.FromMinutes(settings.SendNotificationBeforeMinutes.Value);

            return result;
        }

        public static JSON.NotificationSettings ModelToJson(Scaffold.NotificationUserSettings settings)
        {
            return new JSON.NotificationSettings
            {
                SendNotificationBeforeMinutes = (int)settings.NotificationHeadroom.TotalMinutes
            };
        }

        private static Regex NotANumberRegex = new Regex(@"[^\d]");

        public static String ReFormatPhoneNumber(String phoneNumber)
        {
            phoneNumber = NotANumberRegex.Replace(phoneNumber, String.Empty);

            //  ie +1 234-567-8901
            if (phoneNumber.Length != 11)
                return null;

            String countryCode = phoneNumber.Substring(0, 1);
            String areaCode = phoneNumber.Substring(1, 3);
            String prefix = phoneNumber.Substring(4, 3);
            String lineNumber = phoneNumber.Substring(7, 4);

            return $"+{countryCode} {areaCode}-{prefix}-{lineNumber}";
        }
    }
}
