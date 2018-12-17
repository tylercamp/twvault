using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TW.Vault.Services.SMS
{
    public class NotificationsService
    {
        private ILogger logger = Log.ForContext<NotificationsService>();

        public void RunOnce(Scaffold.VaultContext context, TimeSpan window, CancellationToken cancellationToken)
        {
            /*
                * Would like to do exact filtering for which notifications need
                * to be sent, but there's an issue with the PSQL library being used here
                * in DateTime comparisons:
                * 
                *    where notification.EventOccursAt - userSettings.NotificationHeadroom - refreshDelayTimeSpan < now + worldSettings.UtcOffset
                *    
                * ^ this throws:
                * 
                *    System.InvalidCastException: Can't write CLR type System.DateTime with handler type IntervalHandler
                * 
                * Instead do a broad query for notifications in the next 24 hours, then do the correct comparisons locally
                * 
                */

            var now = DateTime.UtcNow;
            var tomorrow = now + TimeSpan.FromDays(1);
            //  General check
            var possibleNotifications = (
                    from notification in context.NotificationRequest
                    join user in context.User.Include(u => u.NotificationPhoneNumber) on notification.Uid equals user.Uid
                    join userSettings in context.NotificationUserSettings on user.Uid equals userSettings.Uid
                    join userWorld in context.World on user.WorldId equals userWorld.Id
                    join worldSettings in context.WorldSettings on userWorld.Id equals worldSettings.WorldId

                    where notification.Enabled
                    where notification.EventOccursAt < tomorrow
                    select new { Notification = notification, Settings = userSettings, PhoneNumbers = user.NotificationPhoneNumber, ServerTimeOffset = worldSettings.UtcOffset }
                ).ToListAsync().ResultWithToken(cancellationToken);

            if (!possibleNotifications.Any())
                return;

            //  Accurate check
            var upcomingNotifications = (
                from notification in possibleNotifications
                where notification.Notification.EventOccursAt - notification.Settings.NotificationHeadroom - window < now + notification.ServerTimeOffset
                select notification
            ).ToList();

            if (upcomingNotifications.Count == 0)
            {
                logger.Warning("Found {0} possible notifications but will only be running {1}", possibleNotifications.Count, upcomingNotifications.Count);
            }

            var notificationsByUser = upcomingNotifications.Select(r => r.Notification.Uid).Distinct().ToDictionary(
                    uid => uid,
                    uid => upcomingNotifications.Where(r => r.Notification.Uid == uid && r.Notification.Enabled).ToList()
                );

            foreach (var userNotifications in notificationsByUser)
            {
                var notifications = userNotifications.Value;

                if (notifications.Count == 0)
                {
                    logger.Warning("Notifications are being processed for UID={0} but no notifications were selected in this run", userNotifications.Key);
                }

                var timeOffset = notifications.First().ServerTimeOffset;
                var notificationText = BuildNotificationText(notifications.Select(r => r.Notification).ToList(), DateTime.UtcNow + timeOffset);
                var phoneNumbers = notifications[0].PhoneNumbers.Select(pn => pn.PhoneNumber).ToList();
                if (phoneNumbers.Count == 0)
                {
                    logger.Warning("Notification was requested but no phone numbers were found for UID={0}", userNotifications.Key);
                }

                foreach (var phoneNumber in phoneNumbers)
                {
                    try
                    {
                        Features.Notifications.SMS.Send(phoneNumber, notificationText);
                    }
                    catch (Exception e)
                    {
                        logger.Warning(
                            "Unable to send notification for notification IDs {0} due to exception:\n" + e,
                            String.Join(',', notifications.Select(r => r.Notification.Id))
                        );
                    }
                }
            }

            context.NotificationRequest.RemoveRange(upcomingNotifications.Select(r => r.Notification));
            context.SaveChangesAsync().Wait(cancellationToken);
        }

        private String BuildNotificationText(List<Scaffold.NotificationRequest> requests, DateTime serverTime)
        {
            String FormatNotificationMessage(Scaffold.NotificationRequest request)
            {
                String ToStringMin2Numbers(object value)
                {
                    var str = value.ToString();
                    while (str.Length < 2)
                        str = '0' + str;
                    return str;
                }

                var time = request.EventOccursAt;
                var hour = ToStringMin2Numbers(time.Hour);
                var minute = ToStringMin2Numbers(time.Minute);
                var second = ToStringMin2Numbers(time.Second);
                var day = ToStringMin2Numbers(time.Day);
                var month = ToStringMin2Numbers(time.Month);

                var remainingMinutes = (time - serverTime).TotalMinutes.ToString("0.#");

                String formattedTime = $"At {hour}:{minute}:{second} on {day}/{month}/{time.Year} (in {remainingMinutes} minutes)";
                return $"{formattedTime}: {request.Message}";
            }

            StringBuilder notificationMessage = new StringBuilder();
            if (requests.Count == 1)
            {
                notificationMessage.Append(FormatNotificationMessage(requests[0]));
            }
            else
            {
                var maxNotifications = Configuration.Behavior.Notifications.MaxNotificationsPerMessage;
                for (int i = 0; i < requests.Count && i < maxNotifications; i++)
                {
                    notificationMessage.Append($"{i+1}. ");
                    notificationMessage.Append(FormatNotificationMessage(requests[i]));

                    if (i < requests.Count - 1)
                    {
                        notificationMessage.AppendLine();
                        notificationMessage.AppendLine();
                    }
                }

                if (requests.Count > maxNotifications)
                {
                    notificationMessage.Append($"({requests.Count - maxNotifications} more not shown)");
                }
            }

            return notificationMessage.ToString();
        }

        public void ClearExpiredNotifications(Scaffold.VaultContext context, CancellationToken cancellationToken)
        {
            var yesterday = DateTime.UtcNow - TimeSpan.FromDays(1);
            var expiredNotifications = (
                    from notification in context.NotificationRequest
                    where notification.EventOccursAt < yesterday
                    select notification
                ).ToListAsync().ResultWithToken(cancellationToken);

            context.RemoveRange(expiredNotifications);
            context.SaveChanges();
        }
    }
}
