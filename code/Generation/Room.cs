using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SCP.Generation;

public partial class RoomsConfig
{
	public List<RoomData> Rooms { get; set; } = new();
}

public class RoomData
{
	public string Name { get; set; }
	public string Model { get; set; }
	public bool StaticSpawn { get; set; } = false;
	public Vector3 StaticPosition { get; set; } = Vector3.Zero;
	public Angles StaticOrientation { get; set; } = Angles.Zero;
	public List<LightData> LightsData { get; set; } = new();
	public string Type { get; set; } = "Undefined";
	public int Limit { get; set; } = -1;
	public int NbSpawn { get; set; } = 0;
	public bool Force { get; set; } = false;
}

public class Room
{
	public const float Size = 628.1288f;
	public RoomData RoomData { get; set; }
	public List<SpotLightEntity> Lights { get; set; } = new();
	public Sector Sector { get; set; }
	public Prop Prop { get; set; }

	private Room(RoomData roomData)
	{
		RoomData = roomData;
	}

	public static Room Create(RoomData roomData)
	{
		if ( roomData == null )
			return null;

		return new Room(roomData);
	}

	public void GenerateLights()
	{
		foreach ( LightData light in RoomData.LightsData )
		{
			Lights.Add( new SpotLightEntity
				{
					Enabled = light.Enabled,
					DynamicShadows = light.DynamicShadows,
					Range = light.Range,
					Falloff = light.Falloff,
					LinearAttenuation = light.LinearAttenuation,
					QuadraticAttenuation = light.QuadraticAttenuation,
					Brightness = light.Brightness,
					Color = light.Color,
					FogStrength = light.FogStrength,
					InnerConeAngle = light.InnerConeAngle,
					OuterConeAngle = light.OuterConeAngle,
					Position = Prop.Transform.PointToWorld( light.Position ),
					Rotation = Prop.Transform.RotationToWorld( Rotation.From( light.Orientation ) ),
					Parent = Prop,
					Model = Model.Load( "models/light/light_tubular.vmdl" )
				}
			);
		}
	}

	public void Delete()
	{
		Log.Info( $"Je supprime un type {RoomData.Type}" );
		foreach( SpotLightEntity light in Lights )
		{
			light.Delete();
		}

		Prop.Delete();
	}
}
