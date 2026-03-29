using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PollPlugin.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PollCreateCommand : CommandSystem.ICommand
    {
        public string Command => "pollcreate";
        public string[] Aliases => new[] { "pcreate" };
        public string Description => "Создать голосование. Синтаксис: pollcreate \"<вопрос>\" <время_сек> \"<вариант1>\" \"<вариант2>\" [...]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("poll.create"))
            {
                response = "У вас нет прав для создания голосований (poll.create).";
                return false;
            }

            string rawInput = string.Join(" ", arguments);

            List<string> parsed = new List<string>();
            Regex regex = new Regex("\"([^\"]*)\"|([^\\s]+)");
            foreach (Match match in regex.Matches(rawInput))
            {
                if (match.Groups[1].Success)
                    parsed.Add(match.Groups[1].Value);
                else
                    parsed.Add(match.Groups[2].Value);
            }

            if (parsed.Count < 4)
            {
                response = "Использование: pollcreate \"<вопрос>\" <время_сек> \"<вариант1>\" \"<вариант2>\" [...]";
                return false;
            }

            string question = parsed[0];

            if (!int.TryParse(parsed[1], out int duration) || duration <= 0)
            {
                response = "Время должно быть положительным целым числом (в секундах).";
                return false;
            }

            List<string> options = new List<string>();
            for (int i = 2; i < parsed.Count; i++)
                options.Add(parsed[i]);

            if (options.Count < 2)
            {
                response = "Необходимо указать минимум 2 варианта ответа.";
                return false;
            }

            if (PollManager.IsActive)
            {
                response = "В данный момент уже активно другое голосование. Дождитесь его завершения.";
                return false;
            }

            PollManager.StartPoll(question, duration, options);
            response = $"Голосование \"{question}\" создано на {duration} секунд с {options.Count} вариантами.";
            return true;
        }
    }
}