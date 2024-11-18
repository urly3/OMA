namespace OMA.Core.Models.Imported;

public record Lobby
{
    public Match? match { get; set; }
    public List<Event>? events { get; set; }
    public List<User>? users { get; set; }
    public long first_event_id { get; set; }
    public long latest_event_id { get; set; }
    public object? current_game_id { get; set; }
}
