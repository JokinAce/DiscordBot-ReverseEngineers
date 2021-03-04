using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using static DiscordBotEthan.Program;

namespace DiscordBotEthan.Players {

    public class SQLiteController {

        public struct Player {
            public ulong ID { get; set; }
            public List<string> LastMessages { get; set; }
            public List<string> Warns { get; set; }
            public bool Muted { get; set; }

            internal async Task Save() {
                await new SQLiteController().Save(this);
            }
        }

        public async Task<Player> GetPlayer(ulong ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            var output = await cnn.QuerySingleOrDefaultAsync($"SELECT * FROM Players WHERE ID=@id", new { id = ID }).ConfigureAwait(false);

            if (output == null) {
                await cnn.ExecuteAsync($"INSERT INTO Players (ID) VALUES (@id)", new { id = ID }).ConfigureAwait(false);
                output = await cnn.QuerySingleOrDefaultAsync($"SELECT * FROM Players WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
            }

            long IDc = output.ID;
            string LastMessages = output.LastMessages;
            string Warns = output.Warns;
            long Muted = output.Muted;

            Player player = new Player {
                ID = (ulong)IDc,
                LastMessages = string.IsNullOrEmpty(LastMessages) ? new List<string>() : LastMessages.Split(",").ToList(),
                Warns = string.IsNullOrEmpty(Warns) ? new List<string>() : Warns.Split(",").ToList(),
                Muted = Convert.ToBoolean(Muted)
            };

            return player;

        }

        public async Task Save(Player player) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            var args = new Dictionary<string, object>{
                {"@id", player.ID},
                {"@lastmessages", string.Join(",", player.LastMessages)},
                {"@warns", string.Join(",", player.Warns)},
                {"@muted", player.Muted}
            };
            await cnn.ExecuteAsync($"UPDATE Players SET LastMessages=@lastmessages, Warns=@warns, Muted=@muted WHERE ID=@id", args).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns all Reminders from DB
        /// </summary>
        /// <returns>Dynamic List</returns>

        public async Task<dynamic> GetReminders() {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.QueryAsync("SELECT * FROM Reminders").ConfigureAwait(false);
        }

        public async Task<dynamic> GetReminderWithID(long ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.QueryFirstOrDefaultAsync("SELECT * FROM Tempmutes WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
        }

        public async Task<int> AddReminder(long ID, long ChannelID, long Date, string Reminder) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("INSERT INTO Reminders (ID, ChannelID, Date, Reminder) VALUES (@id, @channelid, @date, @reminder)", new { id = ID, channelid = ChannelID, date = Date, reminder = Reminder }).ConfigureAwait(false);
        }

        public async Task<int> DeleteRemindersWithDate(long Date) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("DELETE FROM Reminders WHERE Date=@date", new { date = Date }).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns all Tempmutes from DB
        /// </summary>
        /// <returns>Dynamic List</returns>

        public async Task<dynamic> GetTempmutes() {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.QueryAsync("SELECT * FROM Tempmutes").ConfigureAwait(false);
        }

        public async Task<dynamic> GetTempmuteWithID(long ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.QueryFirstOrDefaultAsync("SELECT * FROM Tempmutes WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
        }

        public async Task<int> AddTempmute(long ID, long Date) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("INSERT INTO Tempmutes (ID, Date) VALUES (@id, @date)", new { id = ID, date = Date }).ConfigureAwait(false);
        }

        public async Task<int> DeleteTempmutesWithID(long ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("DELETE FROM Tempmutes WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
        }
    }
}