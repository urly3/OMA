using System.Net;
using Imported = OMA.Core.Models.Imported;
using Internal = OMA.Core.Models.Internal;

namespace OMA.Core.Services;

public static class OMAImportService
{
    internal static bool DoesLobbyExist(long lobbyId)
    {
        string multi_link = @"https://osu.ppy.sh/community/matches/";
        string base_uri = multi_link + lobbyId.ToString();

        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Get, base_uri);
            request.Headers.Add("Accept", @"application/json, text/javascript, */*; q=0.01");
            var response = client.Send(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            return true;
        }
    }

    internal static Internal::Match? GetMatch(long id, int bestOf = 0, int warmups = 0)
    {
        // TODO:
        // use nullables instead of exceptions, or unions / options.
        // pass in the string and handle both cases with a flag?
        // have the function ready to use urls.
        var lobby = GetLobbyFromId(id);
        if (lobby == null)
        {
            return null;
        }
        var match = GetMatchFromLobby(lobby, bestOf, warmups);
        return match;
    }

    private static long LobbyIdFromUrl(string url)
    {
        if (!Uri.TryCreate(url, new UriCreationOptions(), out var uri))
        {
            throw new Exception("invalid url: not a url");
        };

        string strId = uri.Segments.Last();


        if (!long.TryParse(strId, out var id))
        {
            throw new Exception("invalid url: didn't end in an id");
        }

        return id;
    }

    private static Imported::Lobby? GetLobbyFromId(long id)
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
                //throw new Exception("not ok response: lobby likely not found");
                return null;
            }

            string json = response.Content.ReadAsStringAsync().Result;
            var lobby = System.Text.Json.JsonSerializer.Deserialize<Imported::Lobby>(json) ?? throw new Exception("failed to deserialise");

            if (lobby.events == null || lobby.users == null)
            {
                return null;
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
                    //throw new Exception("not ok response: lobby likely not found");
                    return null;
                }

                var newJson = newReponse.Content.ReadAsStringAsync().Result;
                var newLobby = System.Text.Json.JsonSerializer.Deserialize<Imported::Lobby>(newJson) ?? throw new Exception("failed to deserialise");
                if (newLobby.users == null || newLobby.events == null)
                {
                    return null;
                }

                lobby.events.InsertRange(0, newLobby.events);

                foreach (var user in newLobby.users)
                {
                    if (lobby.users.Find(users => users.id == user.id) == null)
                    {
                        lobby.users.Add(user);
                    }
                }

                lobbyFirstEventId = (long)newLobby.events[0].id!;
            }

            foreach (var gameEvent in lobby.events.Where(e => e.game != null))
            {
                // TODO:
                // handle other team types.
                if (gameEvent?.game?.team_type != "team-vs")
                {
                    throw new Exception("not team vs");
                }
            }

            return lobby;
        }
    }

    private static string GenerateAdditionalQueryString(long event_id)
    {
        return @"?before=" + event_id.ToString() + @"&limit=100";
    }

    // converting the imported lobby type into the match type that we can perform operations on.
    private static Internal::Match GetMatchFromLobby(Imported::Lobby lobby, int bestOf = 0, int warmups = 0)
    {
        // we know the lobby has events.
        // we know the lobby has users.
        // we know the lobby is team vs.

        // this is going to be a long function.
        // cry about it.
        // maybe this should be async if perfomance is needed for some reason?

        Internal::Match match = new()
        {
            BestOf = bestOf,
            WarmupCount = warmups,
            LobbyId = (long)lobby.match?.id!,
            Name = lobby.match?.name!,
            StartTime = (DateTime)lobby.match?.start_time!,
            EndTime = (DateTime)lobby.match?.end_time!
        };

        Dictionary<long, Internal::Team> playerTeams = new();

        // maps.
        foreach (var gameEvent in lobby.events?.Where(e => e.detail?.type == "other").ToList() ?? new(0))
        {
            Internal::Map map = new()
            {
                BeatmapId = gameEvent.game?.beatmap_id ?? 0,
                BeatmapSetId = gameEvent.game?.beatmap?.beatmapset_id ?? 0,
                CoverUrl = gameEvent.game?.beatmap?.beatmapset?.covers?.cover ?? "unavailable",
                Mapper = gameEvent.game?.beatmap?.beatmapset?.creator ?? "unavailable",
                Artist = gameEvent.game?.beatmap?.beatmapset?.artist ?? "unavailable",
                Title = gameEvent.game?.beatmap?.beatmapset?.title ?? "unavailable",
                StarRating = gameEvent.game?.beatmap?.difficulty_rating ?? 0.0f
            };

            // scores.
            foreach (var score in gameEvent.game?.scores ?? new())
            {
                map.Scores.Add(new()
                {
                    UserId = score.user_id,
                    TotalScore = score.score,
                    Accuracy = score.accuracy,
                    MaxCombo = score.max_combo,
                    PerfectCombo = score.perfect >= 1,
                    PP = score.pp ?? 0.0f,
                });

                // save teams of each player.
                // thus, we only add users to the match that have set a score.
                if (!playerTeams.ContainsKey(score.user_id))
                {
                    playerTeams.Add(score.user_id,
                        score.match?.team?.ToLower() == "blue" ? Internal::Team.Blue
                        : score.match?.team?.ToLower() == "red" ? Internal::Team.Red
                        : Internal::Team.None);
                }
            }

            if (map.Scores.All(s => s.TotalScore == 0))
            {
                match.AbandonedMaps.Add(map);
            }
            else
            {
                match.CompletedMaps.Add(map);
            }
        }

        // users.
        foreach (var user in lobby.users ?? new())
        {
            if (!playerTeams.ContainsKey(user.id))
            {
                continue;
            }

            match.Users.Add(new()
            {
                UserId = user.id,
                Username = user.username!,
                AvatarUrl = user.avatar_url!,
                CountryName = user.country?.name!,
                CountryCode = user.country_code!,
                Team = playerTeams[user.id],
            });
        }

        GetMatchStats(match);
        GetPlayerStats(match);
        return match;
    }

    private static void GetMatchStats(Internal::Match match)
    {
        int redWins = 0;
        int blueWins = 0;
        bool skipMap = false;

        for (int i = 0; i < match.CompletedMaps.Count; ++i)
        {
            var map = match.CompletedMaps[i];

            if (match.WarmupCount > 0 && i < match.WarmupCount)
            {
                match.WarmupMaps.Add(map);
                skipMap = true;
            }

            if (match.BestOf > 0 && (redWins >= (match.BestOf + 1) / 2 || blueWins >= (match.BestOf + 1) / 2))
            {
                match.ExtraMaps.Add(map);
                skipMap = true;
            }

            long redTotalScore = 0;
            int redScoreCount = 0;

            long blueTotalScore = 0;
            int blueScoreCount = 0;

            foreach (var score in match.CompletedMaps[i].Scores)
            {
                Internal::User setBy = match.Users
                    .Where(u => u.UserId == score.UserId)
                    .First();

                if (setBy.Team == Internal::Team.Red)
                {
                    redTotalScore += score.TotalScore;
                    ++redScoreCount;
                }
                else
                {
                    blueTotalScore += score.TotalScore;
                    ++blueScoreCount;
                }
            }

            map.AverageScore = (redTotalScore + blueTotalScore) / map.Scores.Count;
            map.RedAverageScore = redTotalScore / redScoreCount;
            map.BlueAverageScore = blueTotalScore / blueScoreCount;

            if (skipMap)
            {
                skipMap = false;
                continue;
            }

            if (redTotalScore > blueTotalScore)
            {
                ++redWins;
            }
            else
            {
                ++blueWins;
            }
        }

        match.BlueWins = blueWins;
        match.RedWins = redWins;

        if (match.RedWins > match.BlueWins)
        {
            match.WinningTeam = Internal.Team.Red;
            if (match.BestOf == 0)
            {
                match.BestOf = (match.RedWins * 2) - 1;
            }
        }
        else if (match.BlueWins > match.RedWins)
        {
            match.WinningTeam = Internal.Team.Blue;
            if (match.BestOf == 0)
            {
                match.BestOf = (match.BlueWins * 2) - 1;
            }
        }
        else
        {
            match.WinningTeam = Internal.Team.None;
            match.BestOf = match.RedWins * 2;
        }

        // remove non-tourney maps.
        foreach (var map in match.AbandonedMaps)
        {
            match.CompletedMaps.Remove(map);
        }
        foreach (var map in match.ExtraMaps)
        {
            match.CompletedMaps.Remove(map);
        }
    }

    private static void GetPlayerStats(Internal::Match match)
    {
        foreach (var user in match.Users)
        {
            var mapsUserPlayed = match.CompletedMaps.Where(m => m.Scores.Exists(s => s.UserId == user.UserId));
            user.MapsPlayed = mapsUserPlayed.Count();

            if (user.MapsPlayed == 0)
            {
                continue;
            }

            float matchAvg = 0.0f;
            float teamAvg = 0.0f;

            foreach (var map in mapsUserPlayed)
            {
                var score = map.Scores.FirstOrDefault(s => s.UserId == user.UserId)!;
                // highs.
                if (user.HighestScore == 0 || score.TotalScore > user.HighestScore)
                {
                    user.HighestScore = score.TotalScore;
                }
                if (user.HighestAccuracy == 0 || score.Accuracy > user.HighestAccuracy)
                {
                    user.HighestAccuracy = score.Accuracy;
                }

                // lows.
                if (user.LowestScore == 0 || score.TotalScore < user.LowestScore)
                {
                    user.LowestScore = score.TotalScore;
                }
                if (user.LowestAccuracy == 0 || score.Accuracy < user.LowestAccuracy)
                {
                    user.LowestAccuracy = score.Accuracy;
                }

                user.AverageScore += score.TotalScore;
                user.AverageAccuracy += score.Accuracy;

                matchAvg += score.TotalScore / map.AverageScore;

                teamAvg += user.Team == Internal::Team.Red
                    ? score.TotalScore / map.RedAverageScore
                    : score.TotalScore / map.BlueAverageScore;
            }

            user.AverageScore = user.AverageScore / user.MapsPlayed;
            user.AverageAccuracy = user.AverageAccuracy / user.MapsPlayed * 100;
            user.HighestAccuracy *= 100;
            user.LowestAccuracy *= 100;

            float cost = 2.0f / (user.MapsPlayed + 2);
            user.MatchCostTotal = cost * matchAvg;
            user.MatchCostTeam = cost * teamAvg;
        }
    }
}
