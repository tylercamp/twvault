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

        //  Notifications generally not available when an account is being sat, to preserve privacy

        [HttpGet("requests")]
        public async Task<IActionResult> GetNotificationRequests()
        {
            if (IsSitter)
                return Ok(Enumerable.Empty<JSON.Notification>());

            var requests = await (
                    from request in context.NotificationRequest
                    where request.Uid == CurrentUserId && request.Enabled
                    orderby request.EventOccursAt
                    select request
                ).ToListAsync();

            var jsonRequests = requests.Select(NotificationConvert.ModelToJson).ToList();
            return Ok(jsonRequests);
        }

        [HttpPost("requests")]
        public async Task<IActionResult> AddOrUpdateNotificationRequest([FromBody]JSON.Notification jsonNotification)
        {
            if (IsSitter)
                return Ok();

            var scaffoldRequest = NotificationConvert.JsonToModel(jsonNotification, null);
            scaffoldRequest.Enabled = true;
            scaffoldRequest.Uid = CurrentUserId;

            var tx = BuildTransaction();
            scaffoldRequest.Tx = tx;
            context.Add(tx);
            context.Add(scaffoldRequest);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("requests/{requestId}")]
        public async Task<IActionResult> DeleteNotificationRequest(long requestId)
        {
            if (IsSitter)
                return Ok();

            var request = await context.NotificationRequest.FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null)
                return NotFound();

            if (request.Uid != CurrentUserId)
            {
                context.Add(MakeFailedAuthRecord("User did not own the notification request"));
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            request.Enabled = false;
            request.Tx = BuildTransaction(request.TxId);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("phone-numbers")]
        public async Task<IActionResult> GetPhoneNumbers()
        {
            if (IsSitter)
                return Ok(Enumerable.Empty<JSON.PhoneNumber>());

            var phoneNumbers = await (
                    from sms in context.NotificationPhoneNumber
                    where sms.Uid == CurrentUserId && sms.Enabled
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

            var scaffoldPhoneNumber = await (
                    from phoneNumber in context.NotificationPhoneNumber
                    where phoneNumber.Uid == CurrentUserId
                    where phoneNumber.PhoneNumber == formattedNumber
                    select phoneNumber
                ).FirstOrDefaultAsync();

            var isNewNumber = scaffoldPhoneNumber == null;

            if (scaffoldPhoneNumber != null)
            {
                scaffoldPhoneNumber.Label = phoneNumberRequest.Label;
            }
            else
            {
                var newNumber = new Scaffold.NotificationPhoneNumber();
                newNumber.Uid = CurrentUserId;
                newNumber.PhoneNumber = formattedNumber;
                newNumber.Label = phoneNumberRequest.Label;
                newNumber.Enabled = true;
                context.Add(newNumber);

                scaffoldPhoneNumber = newNumber;

                var hasSettings = await context.NotificationUserSettings.Where(s => s.Uid == CurrentUserId).AnyAsync();
                if (!hasSettings)
                {
                    var newSettings = new Scaffold.NotificationUserSettings();
                    newSettings.Uid = CurrentUserId;
                    newSettings.Tx = BuildTransaction();

                    context.Add(newSettings);
                }
            }

            isNewNumber = isNewNumber || !scaffoldPhoneNumber.Enabled;

            scaffoldPhoneNumber.Enabled = true;
            scaffoldPhoneNumber.Tx = BuildTransaction(scaffoldPhoneNumber.TxId);
            context.Add(scaffoldPhoneNumber.Tx);

            await context.SaveChangesAsync();

            if (isNewNumber)
            {
                SMS.Send(formattedNumber, "This phone number has been registered with the Vault. Text UNSUBSCRIBE to stop receiving messages.");
            }

            if (isNewNumber)
                return NoContent();
            else
                return Ok();
        }

        [HttpDelete("phone-numbers/{id}")]
        public async Task<IActionResult> DeletePhoneNumber(int id)
        {
            if (IsSitter)
                return Ok();

            var phoneNumber = await context.NotificationPhoneNumber.FirstOrDefaultAsync(pn => pn.Id == id);
            if (phoneNumber == null)
                return NotFound();

            if (phoneNumber.Uid != CurrentUserId)
            {
                context.Add(MakeFailedAuthRecord("User did not own the phone number"));
                await context.SaveChangesAsync();
                return Unauthorized();
            }

            var wasEnabled = phoneNumber.Enabled;

            context.Remove(phoneNumber);
            await context.SaveChangesAsync();

            if (wasEnabled)
            {
                try
                {
                    SMS.Send(phoneNumber.PhoneNumber, "Your number has been removed from the Vault.");
                }
                catch { }
            }

            logger.LogInformation("Phone number for user {0} was deleted by IP {1}", CurrentUserId, UserIP.ToString());

            return Ok();
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await context.NotificationUserSettings.FirstOrDefaultAsync(s => s.Uid == CurrentUserId);
            if (settings == null)
            {
                settings = new Scaffold.NotificationUserSettings();
                settings.Uid = CurrentUserId;
                settings.Tx = BuildTransaction();
                context.Add(settings);
                await context.SaveChangesAsync();
            }

            return Ok(NotificationConvert.ModelToJson(settings));
        }

        [HttpPost("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody]JSON.NotificationSettings newSettings)
        {
            if (IsSitter)
                return Ok();

            var scaffoldSettings = await context.NotificationUserSettings.FirstOrDefaultAsync(s => s.Uid == CurrentUserId);
            NotificationConvert.JsonToModel(newSettings, scaffoldSettings, context);

            scaffoldSettings.Tx = BuildTransaction(scaffoldSettings.TxId);
            context.Add(scaffoldSettings.Tx);

            await context.SaveChangesAsync();

            return Ok();
        }
    }
}