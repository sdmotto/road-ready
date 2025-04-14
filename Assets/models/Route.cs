using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

[Table("routes")]
public class Route : BaseModel
{
    [PrimaryKey("id", false)] public string Id { get; set; }
    [Column("user_id")] public string UserId { get; set; }
    [Column("route_name")] public string RouteName { get; set; }

    [Column("marker_positions")]
    public string MarkerPositionsJson
    {
        get => JsonConvert.SerializeObject(markerPositions.ConvertAll(v => new Vector3DTO(v)));
        set => markerPositions = JsonConvert.DeserializeObject<List<Vector3DTO>>(value)?.ConvertAll(dto => dto.ToVector3()) ?? new List<Vector3>();
    }

    [Column("line_points")]
    public string LinePointsJson
    {
        get => JsonConvert.SerializeObject(linePoints.ConvertAll(v => new Vector3DTO(v)));
        set => linePoints = JsonConvert.DeserializeObject<List<Vector3DTO>>(value)?.ConvertAll(dto => dto.ToVector3()) ?? new List<Vector3>();
    }

    [Column("created_at")]
    public System.DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public List<Vector3> markerPositions = new List<Vector3>();
    [JsonIgnore]
    public List<Vector3> linePoints = new List<Vector3>();
}
