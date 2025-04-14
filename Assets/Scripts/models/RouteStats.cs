using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

[Table("route_stats")]
public class RouteStats : BaseModel
{
    [PrimaryKey("id", false)] public int Id { get; set; }

    [Column("average_speed")] public float AverageSpeed { get; set; }
    [Column("max_speed")] public float MaxSpeed { get; set; }
    [Column("elapsed_time")] public float ElapsedTime { get; set; }
    [Column("score")] public float Score { get; set; }

    [Column("stop_sign_stops")] public int StopSignStops { get; set; }
    [Column("light_success_count")] public int LightSuccessCount { get; set; }
    [Column("total_collisions")] public int TotalCollisions { get; set; }

    [Column("num_left_turns")] public int NumLeftTurns { get; set; }
    [Column("num_right_turns")] public int NumRightTurns { get; set; }

    [Column("stop_sign_penalty")] public float StopSignPenalty { get; set; }
    [Column("red_penalty")] public float RedPenalty { get; set; }
    [Column("yellow_penalty")] public float YellowPenalty { get; set; }
    [Column("collision_penalty")] public float CollisionPenalty { get; set; }
    [Column("speeding_penalty")] public float SpeedingPenalty { get; set; }
    [Column("turn_sig_penalty")] public float TurnSigPenalty { get; set; }
}
