﻿using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.Bloom.Core.Common.Base;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Tsutskiridze.TradeBuddy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ApiControllerBase
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramController> _logger;
        
        public TelegramController(ITelegramBotClient botClient, ILogger<TelegramController> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] Update update)
        {
            
            _logger.LogInformation("request:");
            _logger.LogInformation(JsonConvert.SerializeObject(update));
            
            
            if (update == null){
                _logger.LogError("bad_request");
                return BadRequest();
            }

            try{
                if (update.Type == UpdateType.Message)
                {
                    var message = update.Message;
                    _logger.LogInformation("updated");
                    await _botClient.SendMessage(message.Chat.Id, $"You said: {message.Text}");
                }

                return Ok();
            }catch (Exception ex){
                throw;
            }
        }
    }
}
