using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Generation;

public partial class RoomsConfig
{
	public List<Room> Rooms { get; set; } = new();
}

public class Room : ICloneable
{
	public string Name { get; set; }
	public string Model { get; set; }
	public Boolean StaticSpawn { get; set; } = false;
	public Vector3 Position { get; set; } = Vector3.Zero;
	public Angles Orientation { get; set; } = Angles.Zero;
	public List<Light> Lights { get; set; } = new();
	public Sector Sector { get; set; }
	public Prop Prop { get; set; }
	public string Type { get; set; }

	public void GenerateLights()
	{
		foreach ( Light light in Lights )
		{
			light.Parent = Prop;

			light.Position = Prop.Transform.PointToWorld( light.Position );
			light.Rotation = Prop.Transform.RotationToWorld( Rotation.From( light.Orientation ) );
			light.SetModel( "models/light/light_tubular.vmdl" );
		}
	}

	public object Clone()
	{
		Room clone = (Room) this.MemberwiseClone();

		List<Light> lights = new();

		foreach( Light light in Lights )
		{
			lights.Add( new Light
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
					Position = light.Position,
					Orientation = light.Orientation,
				} 
			);
		}

		clone.Lights = lights;

		Log.Info( clone.Lights[0] );

		return clone;
	}
}
