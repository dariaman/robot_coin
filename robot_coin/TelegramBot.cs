
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace robot_coin
{
    internal class TelegramBot
    {
        public string TELEGRAM_TOKEN_BOT = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN_BOT", EnvironmentVariableTarget.User);
        public string TELEGRAM_CHATID_ERROR = Environment.GetEnvironmentVariable("TELEGRAM_CHATID_ERROR", EnvironmentVariableTarget.User);
        public string TELEGRAM_CHATID_STATUS = Environment.GetEnvironmentVariable("TELEGRAM_CHATID_STATUS", EnvironmentVariableTarget.User);
        public string TELEGRAM_CHATID_INFO = Environment.GetEnvironmentVariable("TELEGRAM_CHATID_INFO", EnvironmentVariableTarget.User);

        public async Task SendErrorAsync(string Msg)
        {
            var bot = new TelegramBotClient(TELEGRAM_TOKEN_BOT);
            try
            {
                await bot.SendTextMessageAsync(TELEGRAM_CHATID_ERROR, Msg[..Math.Min(Msg.Length, 4095)], ParseMode.Html);
            }
            catch (Exception ex)
            {
                throw new Exception("Undefined Error (Send Exception) " + ex.Message);
            }
        }

        public async Task SendMessageAsync(string Msg)
        {
            var bot = new TelegramBotClient(TELEGRAM_TOKEN_BOT);
            try
            {
                await bot.SendTextMessageAsync(TELEGRAM_CHATID_INFO, Msg[..Math.Min(Msg.Length, 4095)], ParseMode.Html);
            }
            catch (Exception ex)
            {
                throw new Exception("Undefined Error (Send Exception) " + ex.Message);
            }
        }

        public async Task SendStatusAsync(string Msg)
        {
            var bot = new TelegramBotClient(TELEGRAM_TOKEN_BOT);
            try
            {
                await bot.SendTextMessageAsync(TELEGRAM_CHATID_STATUS, Msg[..Math.Min(Msg.Length, 4095)], ParseMode.Html);
            }
            catch (Exception ex)
            {
                throw new Exception("Undefined Error (Send Exception) " + ex.Message);
            }
        }
    }
}
