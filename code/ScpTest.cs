using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using SCP.Config;

namespace SCP;

partial class ScpTest
{
	private static RoomsConfig loadedConfig;

	private const string filePath = "config/room_config.json";

	public static RoomsConfig Config
	{
		get
		{
			if ( loadedConfig == null )
				loadedConfig = LoadConfig().Result;
			return loadedConfig;
		}
	}

	public static async Task<RoomsConfig> LoadConfig()
	{
		if ( !FileSystem.Mounted.FileExists( ScpTest.filePath ) ) return new();

		var json = FileSystem.Mounted.ReadAllText( filePath );
		loadedConfig = JsonSerializer.Deserialize<RoomsConfig>( json );

		return loadedConfig;
	}



	[ConCmd.Server("scptest")]
	public static void spawn()
	{
		loadedConfig = null;
		var config = ScpTest.Config;

		Log.Info( config );

		ConsoleSystem.Caller.Pawn.Transform = new Transform().WithPosition( new Vector3( 0, 0, 0 ) );


		foreach ( Room room in config.rooms )
		{
			Log.Info( room.name );

			var model = Model.Load( room.model );

			if ( model == null || model.IsError )
				return;

			if ( room.staticSpawn )
			{
				var ent = new Prop
				{
					Position = room.position,
					Rotation = Rotation.From( room.orientation ),
					Model = model,
					Name = room.name
				};

				ent.SetupPhysicsFromModel( PhysicsMotionType.Static );

				foreach ( Light light in room.lights )
				{
					light.Parent = ent; 

					light.Position = light.Parent.Transform.PointToWorld( light.Position );
					light.Rotation = light.Parent.Transform.RotationToWorld( Rotation.From(light.Orientation) );
					light.SetModel( "models/light/light_tubular.vmdl" );

					Log.Info( light.Position.ToString() );
				}
			}
		}


		var sun = new EnvironmentLightEntity
		{
			Position = new Vector3( 32, 0, 192 ),
			Rotation = Rotation.From( new Angles( 69.3447f, 103.642f, -103.642f ) ),
			//SunAngle = 2.125f
		};

	}

	[ConCmd.Server("clear_world")]
	public static void clear_world()
	{
		foreach ( var model in Entity.All.OfType<Prop>().ToArray() )
		{
			model.Delete();
		}

		foreach ( var model in Entity.All.OfType<SpotLightEntity>().ToArray() )
		{
			model.Delete();
		}

		Log.Info( "cleared world" );
	}
}
