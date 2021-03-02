using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

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
            using IDbConnection cnn = new SQLiteConnection(@"Data Source=./Players/Players.db; Version=3;");
            var output = await cnn.QueryAsync($"SELECT * FROM Players WHERE ID={ID}", new DynamicParameters()).ConfigureAwait(false);

            if (!output.Any()) {
                await cnn.ExecuteAsync($"INSERT INTO Players (ID, Muted) VALUES ({ID}, 0)").ConfigureAwait(false);
                return new Player();
            }

            Player player = new Player();
            foreach (var item in output) {
                long IDc = item.ID;
                string LastMessages = item.LastMessages;
                string Warns = item.Warns;
                long Muted = item.Muted;

                player = new Player {
                    ID = (ulong)IDc,
                    LastMessages = LastMessages.Split(",").ToList(),
                    Warns = string.IsNullOrWhiteSpace(Warns) ? new List<string>() : Warns.Split(",").ToList(),
                    Muted = Convert.ToBoolean(Muted)
                };
            }

            return player;
        }

        public static async Task Save(Player player) {
            using IDbConnection cnn = new SQLiteConnection(@"Data Source=./Players/Players.db;Version=3;");
            var args = new Dictionary<string, object>{
                {"@id", player.ID},
                {"@lastmessages", player.LastMessages},
                {"@warns", player.Warns},
                {"@muted", player.Muted}
            };
            await cnn.ExecuteAsync($"UPDATE Players SET LastMessages=@lastmessages, Warns=@warns, Muted=@muted WHERE ID=@id", args);
        }
    }
}