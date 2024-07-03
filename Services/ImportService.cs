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

    internal static Internal::Match GetMatchFromLobby(Imported::Lobby lobby, int bestOf, int warmups)
    {
        // TODO:
        // get completed, abandoned, warmup, extra games.
        // calculate all the stats.
        // return the match in the new format.

        // we know the lobby has events.
        // we know the lobby has users.
        // we know the lobby is team vs.

        // this is going to be a long function.
        // maybe this should be async if perfomance is needed for some reason.

        Internal::Match match = new();

        match.BestOf = bestOf;
        match.WarmupCount = warmups;
        match.MultiId = (long)lobby.match?.id!;
        match.StartTime = (DateTime)lobby.match?.start_time!;
        match.StartTime = (DateTime)lobby.match?.end_time!;

        // get each map event played and make a Map from it.
        // converting each thing into our new types.
        bool abandoned = false;
        Internal::Map map = default!;
        foreach (var gameEvent in lobby.events!)
        {
            map = new()
            {
                BeatmapId = (long)gameEvent?.game?.beatmap_id!,
                BeatmapSetId = (long)gameEvent?.game?.beatmap?.beatmapset_id!,
                CoverUrl = gameEvent.game?.beatmap?.beatmapset?.covers?.cover ?? "n/a",
                Mapper = gameEvent.game?.beatmap?.beatmapset?.creator!,
                Artist = gameEvent.game?.beatmap?.beatmapset?.artist!,
                Title = gameEvent.game?.beatmap?.beatmapset?.title!,
                StarRating = (float)gameEvent.game?.beatmap?.difficulty_rating!
            };

            foreach (var score in gameEvent.game?.scores!)
            {
                map.Scores.Add(new()
                {
                    UserId = score.user_id,
                    TotalScore = score.score,
                    Accuracy = score.accuracy,
                    MaxCombo = score.max_combo,
                    PerfectCombo = score.perfect >= 1,
                });
            }

            abandoned = map.Scores.All(s => s.TotalScore == 0);

            if (!abandoned)
            {
                match.CompletedMaps.Add(map);
            }
            else
            {
                match.AbandonedMaps.Add(map);
            }
        }

        return match;
    }
}
