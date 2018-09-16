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
    public class RemindersService : BackgroundService
    {
        private ILogger logger;
        private IServiceScopeFactory scopeFactory;

        private int refreshDelay = Configuration.Behavior.Notifications.NotificationCheckInterval;

        public RemindersService(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            this.scopeFactory = scopeFactory;
            this.logger = loggerFactory.CreateLogger<RemindersService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug("Clearing expired reminders on first run...");
            await WithVaultContext(ClearExpiredReminders);

            logger.LogDebug("Starting reminders service loop");
            while (!stoppingToken.IsCancellationRequested)
            {
                await WithVaultContext(async (context) =>
                {
                    var now = CurrentServerTime;
                    var upcomingReminders = await (
                            from notification in context.NotificationRequest
                            join user in context.User.Include(u => u.NotificationPhoneNumber) on notification.Uid equals user.Uid
                            join userSettings in context.NotificationUserSettings on user.Uid equals userSettings.Uid

                            where notification.EventOccursAt - userSettings.NotificationHeadroom < now
                            select new { Notification = notification, PhoneNumbers = user.NotificationPhoneNumber }
                        ).ToListAsync();

                    if (!upcomingReminders.Any())
                        return;

                    var remindersByUser = upcomingReminders.Select(r => r.Notification.Uid).Distinct().ToDictionary(
                            uid => uid,
                            uid => upcomingReminders.Where(r => r.Notification.Uid == uid).ToList()
                        );

                    foreach (var userReminder in remindersByUser)
                    {
                        var reminders = userReminder.Value;
                        var notificationText = BuildReminderText(reminders.Select(r => r.Notification).ToList());
                        foreach (var phoneNumber in userReminder.Value[0].PhoneNumbers.Select(pn => pn.PhoneNumber))
                        {
                            try
                            {
                                SMS.Send(phoneNumber, notificationText);
                            }
                            catch (Exception e)
                            {
                                logger.LogWarning(
                                    "Unable to send notification for reminder IDs {0} due to exception:\n" + e,
                                    String.Join(',', reminders.Select(r => r.Notification.Id))
                                );
                            }
                        }
                    }

                    context.NotificationRequest.RemoveRange(upcomingReminders.Select(r => r.Notification));
                    await context.SaveChangesAsync();
                });

                await Task.Delay(refreshDelay, stoppingToken);
            }
        }

        //  TODO - Change this on a per-world basis
        private DateTime CurrentServerTime => DateTime.UtcNow + TimeSpan.FromHours(1);

        private String BuildReminderText(List<Scaffold.NotificationRequest> requests)
        {
            StringBuilder reminderMessage = new StringBuilder();
            if (requests.Count == 1)
            {
                reminderMessage.Append(requests[0].Message);
            }
            else
            {
                var maxNotifications = Configuration.Behavior.Notifications.MaxNotificationsPerMessage;
                for (int i = 0; i < requests.Count && i < maxNotifications; i++)
                {
                    reminderMessage.Append($"{i+1}. ");
                    reminderMessage.Append(requests[i].Message);

                    if (i < requests.Count - 1)
                    {
                        reminderMessage.AppendLine();
                        reminderMessage.AppendLine();
                    }
                }

                if (requests.Count > maxNotifications)
                {
                    reminderMessage.Append($"({requests.Count - maxNotifications} more not shown)");
                }
            }

            return reminderMessage.ToString();
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

        private async Task ClearExpiredReminders(Scaffold.VaultContext context)
        {
            var now = CurrentServerTime;
            var expiredReminders = await (
                    from reminder in context.NotificationRequest
                    where reminder.EventOccursAt < CurrentServerTime
                    select reminder
                ).ToListAsync();

            context.RemoveRange(expiredReminders);
            await context.SaveChangesAsync();
        }
    }
}
