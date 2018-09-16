using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Features.Notifications;
using TW.Vault.Model.Convert;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Notification")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class NotificationController : BaseController
    {
        public NotificationController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetNotificationRequests()
        {
            var requests = await (
                    from request in context.NotificationRequest
                    where request.Uid == CurrentUser.Uid
                    select request
                ).ToListAsync();

            var jsonRequests = requests.Select(NotificationConvert.ModelToJson).ToList();
            return Ok(jsonRequests);
        }

        [HttpPost("requests")]
        public async Task<IActionResult> AddOrUpdateNotificationRequest([FromBody]JSON.Notification jsonNotification)
        {
            var scaffoldRequest = NotificationConvert.JsonToModel(jsonNotification, null);
            context.Add(scaffoldRequest);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("phone-numbers")]
        public async Task<IActionResult> GetPhoneNumbers()
        {
            var phoneNumbers = await (
                    from sms in context.NotificationPhoneNumber
                    where sms.Uid == CurrentUser.Uid && sms.Enabled
                    select sms
                ).ToListAsync();

            var jsonNumbers = phoneNumbers.Select(NotificationConvert.ModelToJson).ToList();
            return Ok(jsonNumbers);
        }

        [HttpPost("phone-numbers")]
        public async Task<IActionResult> AddPhoneNumber([FromBody]JSON.NewPhoneNumberRequest phoneNumberRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            String formattedNumber = NotificationConvert.ReFormatPhoneNumber(phoneNumberRequest.PhoneNumber);
            if (formattedNumber == null)
            {
                return BadRequest("Invalid phone number");
            }

            var existingPhoneNumber = await (
                    from phoneNumber in context.NotificationPhoneNumber
                    where phoneNumber.Uid == CurrentUser.Uid
                    where phoneNumber.PhoneNumber == formattedNumber
                    select phoneNumber
                ).FirstOrDefaultAsync();

            if (existingPhoneNumber != null)
            {
                existingPhoneNumber.Label = phoneNumberRequest.Label;
            }
            else
            {
                var newNumber = new Scaffold.NotificationPhoneNumber();
                newNumber.Uid = CurrentUser.Uid;
                newNumber.PhoneNumber = formattedNumber;
                newNumber.Label = phoneNumberRequest.Label;
                context.Add(newNumber);

                var newSettings = new Scaffold.NotificationUserSettings();
                newSettings.Uid = CurrentUser.Uid;

                context.Add(newSettings);
            }

            existingPhoneNumber.Enabled = true;

            await context.SaveChangesAsync();

            if (existingPhoneNumber == null)
            {
                SMS.Send(formattedNumber, "This phone number has been registered with the Vault. Text UNSUBSCRIBE to stop receiving messages.");
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await context.NotificationUserSettings.FirstOrDefaultAsync(s => s.Uid == CurrentUser.Uid);
            if (settings == null)
            {
                settings = new Scaffold.NotificationUserSettings();
                settings.Uid = CurrentUser.Uid;
                context.Add(settings);
                await context.SaveChangesAsync();
            }

            return Ok(NotificationConvert.ModelToJson(settings));
        }

        [HttpPost("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody]JSON.NotificationSettings newSettings)
        {
            var existingSettings = await context.NotificationUserSettings.FirstOrDefaultAsync(s => s.Uid == CurrentUser.Uid);
            NotificationConvert.JsonToModel(newSettings, existingSettings, context);

            await context.SaveChangesAsync();

            return Ok();
        }
    }
}