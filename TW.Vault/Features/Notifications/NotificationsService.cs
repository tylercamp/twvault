using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TW.Vault.Features.Notifications
{
    public class NotificationsService : BackgroundService
    {
        private ILogger logger;
        private IServiceScopeFactory scopeFactory;

        private int refreshDelay = Configuration.Behavior.Notifications.NotificationCheckInterval;

        public NotificationsService(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            this.scopeFactory = scopeFactory;
            this.logger = loggerFactory.CreateLogger<NotificationsService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug("Clearing expired notifications on first run...");
            await WithVaultContext(ClearExpiredNotifications);

            var refreshDelayTimeSpan = TimeSpan.FromMilliseconds(refreshDelay);

            logger.LogDebug("Starting notifications service loop");
            while (!stoppingToken.IsCancellationRequested)
            {
                await WithVaultContext(async (context) =>
                {
                    var now = CurrentServerTime;
                    var upcomingNotifications = await (
                            from notification in context.NotificationRequest
                            join user in context.User.Include(u => u.NotificationPhoneNumber) on notification.Uid equals user.Uid
                            join userSettings in context.NotificationUserSettings on user.Uid equals userSettings.Uid

                            where notification.EventOccursAt - userSettings.NotificationHeadroom - refreshDelayTimeSpan < now
                            select new { Notification = notification, PhoneNumbers = user.NotificationPhoneNumber }
                        ).ToListAsync();

                    if (!upcomingNotifications.Any())
                        return;

                    var notificationsByUser = upcomingNotifications.Select(r => r.Notification.Uid).Distinct().ToDictionary(
                            uid => uid,
                            uid => upcomingNotifications.Where(r => r.Notification.Uid == uid && r.Notification.Enabled).ToList()
                        );

                    foreach (var userNotifications in notificationsByUser)
                    {
                        var notifications = userNotifications.Value;
                        var notificationText = BuildNotificationText(notifications.Select(r => r.Notification).ToList());
                        foreach (var phoneNumber in userNotifications.Value[0].PhoneNumbers.Select(pn => pn.PhoneNumber))
                        {
                            try
                            {
                                SMS.Send(phoneNumber, notificationText);
                            }
                            catch (Exception e)
                            {
                                logger.LogWarning(
                                    "Unable to send notification for notification IDs {0} due to exception:\n" + e,
                                    String.Join(',', notifications.Select(r => r.Notification.Id))
                                );
                            }
                        }
                    }

                    context.NotificationRequest.RemoveRange(upcomingNotifications.Select(r => r.Notification));
                    await context.SaveChangesAsync();
                });

                await Task.Delay(refreshDelay, stoppingToken);
            }
        }

        //  TODO - Change this on a per-world basis
        private DateTime CurrentServerTime => DateTime.UtcNow + TimeSpan.FromHours(1);

        private String BuildNotificationText(List<Scaffold.NotificationRequest> requests)
        {
            var now = CurrentServerTime;

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

                var remainingMinutes = (time - now).TotalMinutes.ToString("0.#");

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

        private async Task WithVaultContext(Func<Scaffold.VaultContext, Task> action)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<Scaffold.VaultContext>())
                {
                    await action(context);
                }
            }
        }

        private async Task ClearExpiredNotifications(Scaffold.VaultContext context)
        {
            var now = CurrentServerTime;
            var expiredNotifications = await (
                    from notification in context.NotificationRequest
                    where notification.EventOccursAt < CurrentServerTime
                    select notification
                ).ToListAsync();

            context.RemoveRange(expiredNotifications);
            await context.SaveChangesAsync();
        }
    }
}
