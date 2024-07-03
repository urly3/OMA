using System.Net;

namespace OMA.Models.Imported;

public record Lobby
{
    public Match? match { get; set; }
    public List<Event>? events { get; set; }
    public List<User>? users { get; set; }
    public long first_event_id { get; set; }
    public long latest_event_id { get; set; }
    public object? current_game_id { get; set; }

    public List<Event> AbandonedGames { get; set; } = new();
    public List<Event> CompletedGames { get; set; } = new();
    public List<Event> ExtraGames { get; set; } = new();
    public List<Event> WarmupGames { get; set; } = new();
    public string WinningTeam { get; set; } = "";
    public int RedWins { get; set; } = 0;
    public int BlueWins { get; set; } = 0;
    int BestOf { get; set; } = 0;
    int Warmups { get; set; } = 0;

    public (long, int) HighestAverageScore { get; set; } = (0, 0);
    public (long, float) HighestAverageAccuracy { get; set; } = (0, 0);

    public static Lobby Parse(long id, int bestOf = 0, int warmups = 0)
    {
        string multi_link = @"https://osu.ppy.sh/community/matches/";
        string base_uri = multi_link + id.ToString();

        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Get, base_uri);
            request.Headers.Add("Accept", @"application/json, text/javascript, */*; q=0.01");
            var response = client.Send(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("not ok response: lobby likely not found");
            }

            string json = response.Content.ReadAsStringAsync().Result;
            var lobby = System.Text.Json.JsonSerializer.Deserialize<Lobby>(json) ?? throw new Exception("failed to deserialise");

            _ = lobby.events ?? throw new Exception("no events");
            _ = lobby.users ?? throw new Exception("no users");

            if (bestOf != 0)
            {
                lobby.BestOf = bestOf;
            }

            if (warmups != 0)
            {
                lobby.Warmups = warmups;
            }

            var lobbyFirstEventIdSaved = lobby.events[0].id ?? throw new NullReferenceException();
            var lobbyFirstEventId = lobby.events[0].id ?? throw new NullReferenceException();
            while (lobby.events[0].id != lobby.first_event_id)
            {
                var newRequest = new HttpRequestMessage(HttpMethod.Get, base_uri + GenerateAdditionalQueryString(lobbyFirstEventId).ToString());
                newRequest.Headers.Add("Accept", @"application/json, text/javascript, */*; q=0.01");

                var newReponse = client.Send(newRequest);
                if (newReponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("not ok response: lobby likely not found");
                }

                var newJson = newReponse.Content.ReadAsStringAsync().Result;
                var newLobby = System.Text.Json.JsonSerializer.Deserialize<Lobby>(newJson) ?? throw new Exception("failed to deserialise");
                _ = newLobby.events ?? throw new Exception("no events");
                _ = newLobby.users ?? throw new Exception("no users");
                lobby.events.InsertRange(0, newLobby.events);

                foreach (var user in newLobby.users)
                {
                    if (lobby.users.Find(users => users.id == user.id) == null)
                    {
                        lobby.users.Add(user);
                    }
                }

                lobbyFirstEventId = newLobby.events[0].id ?? throw new Exception("null event list or something");
            }

            foreach (var gameEvent in lobby.events.Where(e => e.game != null))
            {
                if (gameEvent?.game?.team_type != "team-vs")
                    throw new Exception("not team vs.");
                if (gameEvent?.game?.scores?.Count == 0)
                    lobby.AbandonedGames.Add(gameEvent);
                else
                    lobby.CompletedGames.Add(gameEvent ?? Enumerable.Empty<Event>().GetEnumerator().Current);
            }

            return lobby;
        }
    }

    public static string GenerateAdditionalQueryString(long event_id)
    {
        return @"?before=" + event_id.ToString() + @"&limit=100";
    }

    public void Go()
    {
        CalculateStats();
    }

    public void CalculateStats()
    {
        GetWinningTeam();
        GetPlayerTeams();
        GetAverageScoreForEachGame();
        GetPlayerStats();
        GetHighestAverageScore();
        GetHighestAverageAccuracy();
    }

    public void GetWinningTeam()
    {
        foreach (var gameEvent in this.CompletedGames)
        {
            if (this.Warmups > 0)
            {
                this.WarmupGames.Add(gameEvent);
                --this.Warmups;
                continue;
            }

            if (this.BestOf != 0 && (this.RedWins >= (this.BestOf + 1) / 2 || this.BlueWins >= (this.BestOf + 1) / 2))
            {
                this.ExtraGames.Add(gameEvent);
                continue;
            }

            // sum up the scores of each map and increment winner.
            long redTotalScore = 0;
            long blueTotalScore = 0;

            foreach (var score in gameEvent.game?.scores?.Where(s => s.match?.team == "red") ?? Enumerable.Empty<Score>())
            {
                redTotalScore += score.score;
            }
            foreach (var score in gameEvent.game?.scores?.Where(s => s.match?.team == "blue") ?? Enumerable.Empty<Score>())
            {
                blueTotalScore += score.score;
            }

            _ = redTotalScore > blueTotalScore ? ++this.RedWins : ++this.BlueWins;
        }

        _ = this.RedWins > this.BlueWins ? this.WinningTeam = "red" : this.WinningTeam = "blue";

        foreach (var extraGame in this.ExtraGames)
        {
            this.CompletedGames.Remove(extraGame);
        }
        foreach (var warmupGame in this.WarmupGames)
        {
            this.CompletedGames.Remove(warmupGame);
        }
    }

    public void GetPlayerTeams()
    {
        foreach (var user in this.users ?? Enumerable.Empty<User>())
        {
            user.GetTeam(this.CompletedGames);
        }
    }
    public void GetPlayerStats()
    {
        foreach (var user in this.users ?? Enumerable.Empty<User>())
        {
            user.CalculateAverageScore(this.CompletedGames);
            user.CalculateAverageAccuracy(this.CompletedGames);
            user.GetHighestScore(this.CompletedGames);
            user.GetHighestAccuracy(this.CompletedGames);
            user.GetLowestScore(this.CompletedGames);
            user.GetLowestAccuracy(this.CompletedGames);
            user.GetMatchCosts(this.CompletedGames);
        }
    }

    public void GetAverageScoreForEachGame()
    {

        foreach (var gameEvent in this.CompletedGames)
        {
            float averageScore = 0.0f;
            float blueAverageScore = 0.0f;
            int blueCount = 0;
            float redAverageScore = 0.0f;
            int redCount = 0;

            foreach (var score in gameEvent.game?.scores!)
            {
                if (this.users?.Find(u => u.id == score.user_id)?.Team == "blue")
                {
                    blueAverageScore += score.score;
                    ++blueCount;
                }
                else
                {
                    redAverageScore += score.score;
                    ++redCount;
                }

                averageScore += score.score;
            }

            blueAverageScore /= blueCount;
            redAverageScore /= redCount;
            averageScore /= blueCount + redCount;

            gameEvent.game.BlueAverageScore = blueAverageScore;
            gameEvent.game.RedAverageScore = redAverageScore;
            gameEvent.game.AverageScore = averageScore;
        }
    }

    public void GetHighestAverageScore()
    {
        var haUser = this.users?.MaxBy(u => u.AverageScore);
        var haId = haUser?.id ?? 0;
        var haScore = haUser?.AverageScore ?? 0;
        HighestAverageScore = (haId, haScore);
    }

    public void GetHighestAverageAccuracy()
    {
        var haUser = this.users?.MaxBy(u => u.AverageAccuracy);
        var haId = haUser?.id ?? 0;
        var haAccuracy = haUser?.AverageAccuracy ?? 0;
        HighestAverageAccuracy = (haId, haAccuracy);
    }
}
