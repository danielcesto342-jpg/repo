using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using ChildrenOfTheGraveEnumNetwork.Enums;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.AttackableUnits.AI;
using ChildrenOfTheGrave.ChildrenOfTheGraveServer.GameObjects.StatsNS;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using Dropbox.Api;
using System.IO;
using System.Net.Http.Headers;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Content
{

    public class EndGameInfo
    {
        [JsonProperty]
        public long GameId;
        [JsonProperty]
        public float Time;
        [JsonProperty]
        public int WinningTeam;
        [JsonProperty]
        public int MapId;
        [JsonProperty]
        public Dictionary<TeamId, EndGameTeamStats> EndGameTeams = new()
        {
            {TeamId.TEAM_ORDER, new EndGameTeamStats()},
            {TeamId.TEAM_CHAOS, new EndGameTeamStats()}
        };

        internal EndGameInfo(int winningTeam)
        {
            GameId = Game.Config.GameId;
            Time = Game.Time.GameTime;
            WinningTeam = winningTeam;
            MapId = Game.Map.Id;
            foreach (var player in Game.PlayerManager.GetPlayers())
            {
                EndGameTeams[player.Team].Players.Add(new EndGameChampionStats(player.Champion));
            }
        }

        internal async Task Post(string address)
        {
            Console.WriteLine("Attempting authentication...");
            var jwttoken = await AuthenticateAndGetTokenAsync(address, Game.Config.nicknameforreplay, Game.Config.pswdforreplay);
            Console.WriteLine("Authentication successful, token obtained.");

            Console.WriteLine("Serializing end game data...");


            string serializedEndGameInfo = JsonConvert.SerializeObject(this, Formatting.Indented);

            // Wait for the upload to complete and get the sharing link
            string replaylink = await uploadreplay();

            // Call UpdateGameHistoryAsync with the sharing link
            await UpdateGameHistoryAsync(address, (int)Game.Config.GameId, replaylink, serializedEndGameInfo, jwttoken);
            Game.isUploadingfinished = true;
        }

        static async Task<string> uploadreplay()
        {
            string accessToken = Game.Config.apikeydropbox; // Replace with your access token
            var dbx = new DropboxClient(accessToken);

            // Example of uploading a file
            string filePath = Game.nameofreplay;
            Console.WriteLine($"Attempting to upload file: {filePath}");
            await UploadFile(dbx, filePath);
            Console.WriteLine("Upload completed, generating sharing link...");

            // Generate a sharing link and return it
            string fileName = Path.GetFileName(filePath);
            string sharedLink = await GenerateSharedLink(dbx, "/" + fileName);
            return sharedLink; // Return the sharing link
        }

        static async Task UploadFile(DropboxClient dbx, string filePath)
        {
            try
            {
                using (var fileStream = File.Open(filePath, FileMode.Open))
                {
                    // Upload the file
                    var response = await dbx.Files.UploadAsync(
                        "/" + Path.GetFileName(filePath), // Path in Dropbox
                        Dropbox.Api.Files.WriteMode.Overwrite.Instance, // Replace if file already exists
                        body: fileStream);

                    Console.WriteLine("File uploaded: " + response.PathLower);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during upload: " + ex.Message);
            }
        }

        static async Task<string> GenerateSharedLink(DropboxClient dbx, string filePath)
        {
            try
            {
                // Create a sharing link and return it
                var sharedLink = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(filePath);
                Console.WriteLine("Sharing link: " + sharedLink.Url);
                return sharedLink.Url; // Return the sharing link
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating sharing link: " + ex.Message);
                return null; // Return null in case of error
            }
        }
        internal async Task UpdateGameHistoryAsync(string address, int gameHistoryId, string replayLink, object historyStats, string jwtToken)
        {
            using HttpClient client = new();

            // Create an object with GameHistory details to send
            var gameHistoryDetails = new
            {
                linkOfReplay = replayLink,
                historyStat = historyStats
            };

            // Serialize the object to JSON
            string serializedGameHistory = JsonConvert.SerializeObject(gameHistoryDetails, Formatting.Indented);

            Console.WriteLine($"Serialized GameHistory: {serializedGameHistory}");
            // Prepare the content for the HTTP request
            Console.WriteLine(serializedGameHistory);
            StringContent payload = new(serializedGameHistory, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                // Send the PUT request to update the GameHistory
                HttpResponseMessage response = await client.PutAsync($"http://{address}/profile/History/{gameHistoryId}", payload);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Game history updated successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to update game history. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the game history: {ex.Message}");
            }
        }
        public async Task<string> AuthenticateAndGetTokenAsync(string authAddress, string username, string passworde)
        {
            using HttpClient client = new();

            var authPayload = new
            {
                userName = username,
                password = passworde
            };

            string serializedAuthPayload = JsonConvert.SerializeObject(authPayload);
            StringContent payload = new(serializedAuthPayload, Encoding.UTF8, "application/json");

            try
            {
                Console.WriteLine("Sending authentication request...");
                HttpResponseMessage response = await client.PostAsync($"http://{authAddress}/users/login", payload);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Authentication successful.");
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic tokenResponse = JsonConvert.DeserializeObject(jsonResponse);
                    return tokenResponse.token;  // Assumes the token is returned in this format
                }
                else
                {
                    Console.WriteLine($"Authentication failed. Status code: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authentication: {ex.Message}");
                return null;
            }
        }

    }
}



public class EndGameTeamStats
{
    [JsonProperty]
    public int TeamKillScore => Players.Sum(x => x.ChampionStatistics.Kills);
    [JsonProperty]
    public float TeamPointScore => Players.Sum(x => x.Score);
    [JsonProperty]
    public List<EndGameChampionStats> Players = new();
}

public class EndGameChampionStats
{
    [JsonProperty]
    public string Name;
    [JsonProperty]
    public string Champion;
    [JsonProperty]
    public int SkinId;
    [JsonProperty]
    public int Level;
    [JsonProperty]
    public float Score;
    [JsonProperty]
    public float Gold;
    [JsonProperty]
    public ChampionStatistics ChampionStatistics;
    [JsonProperty]
    public List<int> Items = new();

    public EndGameChampionStats(Champion ch)
    {
        Name = ch.Name;
        Champion = ch.Model;
        SkinId = ch.SkinID;
        Level = ch.Experience.Level;
        Score = ch.ChampionStats.Score;
        ChampionStatistics = ch.ChampionStatistics;
        Gold = ch.GoldOwner.TotalGoldEarned;

        foreach (var item in ch.ItemInventory.GetItems())
        {
            Items.Add(item.ItemData.Id);
        }
    }
}