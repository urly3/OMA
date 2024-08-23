namespace OMA.Models.Imported;

public record User {
    public string? avatar_url { get; set; }
    public string? country_code { get; set; }
    public string? default_group { get; set; }
    public long id { get; set; }
    public bool is_active { get; set; }
    public bool is_bot { get; set; }
    public bool is_deleted { get; set; }
    public bool is_online { get; set; }
    public bool is_supporter { get; set; }
    public DateTime? last_visit { get; set; }
    public bool pm_friends_only { get; set; }
    public object? profile_colour { get; set; }
    public string? username { get; set; }
    public Country? country { get; set; }
}
