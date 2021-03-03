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
                await SQLiteController.Save(this);
            }
        }

        public static async Task<Player> GetPlayer(ulong ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            var output = await cnn.QuerySingleOrDefaultAsync($"SELECT * FROM Players WHERE ID={ID}").ConfigureAwait(false);

            if (output == null) {
                await cnn.ExecuteAsync($"INSERT INTO Players (ID) VALUES ({ID})").ConfigureAwait(false);
                return new Player();
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

        public static async Task Save(Player player) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            var args = new Dictionary<string, object>{
                {"@id", player.ID},
                {"@lastmessages", string.Join(",", player.LastMessages)},
                {"@warns", string.Join(",", player.Warns)},
                {"@muted", player.Muted}
            };
            await cnn.ExecuteAsync($"UPDATE Players SET LastMessages=@lastmessages, Warns=@warns, Muted=@muted WHERE ID=@id", args);
        }
    }
}