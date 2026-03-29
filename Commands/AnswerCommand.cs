using CommandSystem;
using Exiled.API.Features;
using System;
using System.Windows.Input;

namespace PollPlugin.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class AnswerCommand : CommandSystem.ICommand
    {
        public string Command => "answer";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Проголосовать в активном голосовании. Синтаксис: .answer <номер_варианта>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "Команда доступна только для игроков.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Использование: .answer <номер_варианта>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int choice))
            {
                response = "Укажите номер варианта числом.";
                return false;
            }

            response = PollManager.CastVote(player.UserId, choice - 1);
            return !response.Contains("нет") && !response.Contains("Некорректный") && !response.Contains("уже");
        }
    }
}