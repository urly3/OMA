namespace OMA.Models.Imported;

public record Score
{
    public float accuracy { get; set; }
    public object? best_id { get; set; }
    public DateTime created_at { get; set; }
    public object? id { get; set; }
    public long max_combo { get; set; }
    public string? mode { get; set; }
    public int mode_int { get; set; }
    public List<string>? mods { get; set; }
    public bool passed { get; set; }
    public int perfect { get; set; }
    public float? pp { get; set; }
    public string? rank { get; set; }
    public bool replay { get; set; }
    public int score { get; set; }
    public Statistics? statistics { get; set; }
    public string? type { get; set; }
    public long user_id { get; set; }
    public CurrentUserAttributes? current_user_attributes { get; set; }
    public Match? match { get; set; }
}
