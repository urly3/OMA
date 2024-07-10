namespace OMA.Models.Internal;

class Score
{
    public long UserId { get; set; }
    public long TotalScore { get; set; }
    public float Accuracy { get; set; }
    public long MaxCombo { get; set; }
    public bool PerfectCombo { get; set; }
    public int PP { get; set; }
}
