using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using MEC;

namespace PollPlugin
{
    public static class PollManager
    {
        public static bool IsActive { get; private set; }
        public static string Question { get; private set; }
        public static List<string> Options { get; private set; } = new List<string>();
        public static Dictionary<string, int> Votes { get; private set; } = new Dictionary<string, int>();
        private static CoroutineHandle _timerHandle;

        public static void Reset()
        {
            IsActive = false;
            Question = null;
            Options.Clear();
            Votes.Clear();
            if (_timerHandle.IsRunning)
                Timing.KillCoroutines(_timerHandle);
        }

        public static bool StartPoll(string question, int duration, List<string> options)
        {
            if (IsActive)
                return false;

            IsActive = true;
            Question = question;
            Options = new List<string>(options);
            Votes.Clear();

            StringBuilder broadcast = new StringBuilder();
            broadcast.AppendLine($"<b><size=28>📊 ГОЛОСОВАНИЕ</size></b>");
            broadcast.AppendLine($"<size=24>{Question}</size>");
            broadcast.AppendLine();
            for (int i = 0; i < Options.Count; i++)
            {
                broadcast.AppendLine($"<size=22>{i + 1}. {Options[i]}</size>");
            }
            broadcast.AppendLine();
            broadcast.AppendLine($"<size=20>Напишите в консоль (~): <b>.answer <номер></b></size>");
            broadcast.AppendLine($"<size=18>Время: {duration} сек.</size>");

            string broadcastText = broadcast.ToString();

            foreach (Player player in Player.List)
            {
                player.Broadcast((ushort)(duration > 15 ? 15 : duration), broadcastText);
            }

            _timerHandle = Timing.RunCoroutine(PollTimer(duration));
            return true;
        }

        private static IEnumerator<float> PollTimer(int seconds)
        {
            yield return Timing.WaitForSeconds(seconds);
            EndPoll();
        }

        public static void EndPoll()
        {
            if (!IsActive)
                return;

            IsActive = false;

            StringBuilder result = new StringBuilder();
            result.AppendLine($"[POLL] Голосование завершено: \"{Question}\"");
            result.AppendLine($"[POLL] Всего проголосовало: {Votes.Count}");

            Dictionary<int, int> tally = new Dictionary<int, int>();
            for (int i = 0; i < Options.Count; i++)
                tally[i] = 0;

            foreach (var vote in Votes.Values)
                tally[vote]++;

            for (int i = 0; i < Options.Count; i++)
            {
                float percentage = Votes.Count > 0 ? (tally[i] / (float)Votes.Count) * 100f : 0f;
                result.AppendLine($"[POLL]   {i + 1}. {Options[i]} — {tally[i]} голос(ов) ({percentage:F1}%)");
            }

            int maxVotes = tally.Values.Max();
            var winners = tally.Where(x => x.Value == maxVotes).Select(x => Options[x.Key]).ToList();

            if (maxVotes > 0)
            {
                if (winners.Count == 1)
                    result.AppendLine($"[POLL] Победил вариант: \"{winners[0]}\" с {maxVotes} голос(ами)");
                else
                    result.AppendLine($"[POLL] Ничья между: {string.Join(", ", winners.Select(w => $"\"{w}\""))}");
            }
            else
            {
                result.AppendLine("[POLL] Никто не проголосовал.");
            }

            string resultText = result.ToString();

            foreach (Player player in Player.List)
            {
                if (player.RemoteAdminAccess)
                {
                    player.RemoteAdminMessage(resultText);
                }
            }

            Log.Info(resultText);

            foreach (Player player in Player.List)
            {
                player.Broadcast(8, $"<b><size=24>📊 Голосование завершено!</size></b>\n<size=20>Результаты отправлены администрации.</size>");
            }

            Question = null;
            Options.Clear();
            Votes.Clear();
        }

        public static string CastVote(string oderId, int optionIndex)
        {
            if (!IsActive)
                return "В данный момент нет активного голосования.";

            if (optionIndex < 0 || optionIndex >= Options.Count)
                return $"Некорректный номер варианта. Доступные варианты: 1-{Options.Count}.";

            if (Votes.ContainsKey(oderId))
                return "Вы уже проголосовали в этом голосовании.";

            Votes[oderId] = optionIndex;
            return $"Ваш голос за вариант №{optionIndex + 1} (\"{Options[optionIndex]}\") принят.";
        }
    }
}