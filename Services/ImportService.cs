using System.Net;
using Imported = OMA.Models.Imported;
using Internal = OMA.Models.Internal;

namespace OMA.Services;

static class ImportService
{
    internal static long LobbyIdFromUrl(string url)
    {
        if (!Uri.TryCreate(url, new UriCreationOptions(), out var uri))
        {
            throw new Exception("invalid url: not a url");
        };

        string strId = uri.Segments.Last();


        if (!long.TryParse(strId, out var id)
)
        {
            throw new Exception("invalid url: didn't end in an id");
        }

        return id;
    }

    internal static Internal::Match GetMatch(long id, int bestOf = 0, int warmups = 0)
    {
        // FIXME:
        // pass in the string and handle both cases with a flag?
        // have the function ready to use urls.

        var impLobby = GetLobbyFromOsu(id);
        var match = GetMatchFromLobby(impLobby, bestOf, warmups);

        return match;
    }

    internal static Imported::Lobby GetLobbyFromOsu(long id)
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
            var lobby = System.Text.Json.JsonSerializer.Deserialize<Imported::Lobby>(json) ?? throw new Exception("failed to deserialise");

            _ = lobby.events ?? throw new Exception("no events");
            _ = lobby.users ?? throw new Exception("no users");

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
                var newLobby = System.Text.Json.JsonSerializer.Deserialize<Imported::Lobby>(newJson) ?? throw new Exception("failed to deserialise");
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

                lobbyFirstEventId = (long)newLobby.events[0].id!;
            }

            foreach (var gameEvent in lobby.events.Where(e => e.game != null))
            {
                if (gameEvent?.game?.team_type != "team-vs")
                {
                    throw new Exception("not team vs");
                }

                lobby.CompletedGames.Add(gameEvent ?? Enumerable.Empty<Imported::Event>().GetEnumerator().Current);
            }

            return lobby;
        }
    }

    internal static string GenerateAdditionalQueryString(long event_id)
    {
        return @"?before=" + event_id.ToString() + @"&limit=100";
    }

    internal static Internal::Match GetMatchFromLobby(Imported::Lobby lobby, int bestOf = 0, int warmups = 0)
    {
        // TODO:
        // get completed, abandoned, warmup, extra games.
        // calculate all the stats.
        // return the match in the new format.

        // we know the lobby has events.
        // we know the lobby has users.
        // we know the lobby is team vs.

        // this is going to be a long function.
        // cry about it.
        // maybe this should be async if perfomance is needed for some reason?

        Internal::Match match = new();

        match.BestOf = bestOf;
        match.WarmupCount = warmups;
        match.MultiId = (long)lobby.match?.id!;
        match.StartTime = (DateTime)lobby.match?.start_time!;
        match.StartTime = (DateTime)lobby.match?.end_time!;

        Dictionary<long, Internal::Team> playerTeams = new();

        // maps.
        foreach (var gameEvent in lobby.events ?? new())
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
                });

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

        GetMatchStats(match, bestOf, warmups);
        GetPlayerStats(match);
        return match;
    }

    internal static void GetMatchStats(Internal::Match match, int bestOf, int warmups)
    {
        int redWins = 0;
        int blueWins = 0;

        for (int i = 0; i < match.CompletedMaps.Count; ++i)
        {
            // TODO:
            // account for the warmups and best of / extra map(s).

            var map = match.CompletedMaps[i];

            if (warmups > 0 && i < warmups)
            {
                match.CompletedMaps.Remove(map);
                match.WarmupMaps.Add(map);
            }

            if (bestOf > 0 && (redWins >= (bestOf + 1) / 2 || blueWins >= (bestOf + 1) / 2))
            {
                match.CompletedMaps.Remove(map);
                match.ExtraMaps.Add(map);
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

                if (setBy.Team == Internal::Team.Blue)
                {
                    blueTotalScore += score.TotalScore;
                    ++blueScoreCount;
                }
                else
                {
                    redTotalScore += score.TotalScore;
                    ++redScoreCount;
                }
            }

            map.AverageScore = (redTotalScore + blueTotalScore) / map.Scores.Count;
            map.BlueAverageScore = blueTotalScore / blueScoreCount;
            map.RedAverageScore = redTotalScore / redScoreCount;

            if (blueTotalScore > redTotalScore)
            {
                ++blueWins;
            }
            else
            {
                ++redWins;
            }
        }

        match.BlueWins = blueWins;
        match.RedWins = redWins;
    }

    internal static void GetPlayerStats(Internal::Match match)
    {

    }
}
