using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Model.Convert;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using Twilio.TwiML.Messaging;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Sms")]
    public class SmsController : TwilioController
    {
        Scaffold.VaultContext context;
        ILogger logger;


        public SmsController(Scaffold.VaultContext context, ILoggerFactory loggerFactory)
        {
            this.context = context;
            this.logger = loggerFactory.CreateLogger<SmsController>();
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveMessage(String from, String body)
        {
            body = body.ToLower();
            var formattedNumber = NotificationConvert.ReFormatPhoneNumber(from);

            logger.LogDebug("Got sms: " + body);

            var recognizedCancelCodes = new List<String>
            {
                "stop",
                "cancel",
                "unsubscribe"
            };

            if (recognizedCancelCodes.Contains(body))
            {
                var registeredNumbers = await (
                        from number in context.NotificationPhoneNumber
                        where number.PhoneNumber == formattedNumber
                        select number
                    ).ToListAsync();

                foreach (var number in registeredNumbers)
                    number.Enabled = false;

                await context.SaveChangesAsync();
            }

            var recognizedRegisterCodes = new List<String>
            {
                "start"
            };

            if (recognizedRegisterCodes.Contains(body))
            {
                var registeredNumbers = await (
                        from number in context.NotificationPhoneNumber
                        where number.PhoneNumber == formattedNumber
                        select number
                    ).ToListAsync();

                foreach (var number in registeredNumbers)
                    number.Enabled = true;

                await context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}