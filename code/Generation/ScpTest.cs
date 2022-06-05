using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using SCP.Generation;

namespace SCP;

partial class ScpTest
{
	private static SectorsConfig SectorsConfig;

	private const string filePath = "config/room_config.json";

	public static SectorsConfig Config
	{
		get
		{
			if ( SectorsConfig == null )
				SectorsConfig = LoadConfig().Result;
			return SectorsConfig;
		}
	}

	public static async Task<SectorsConfig> LoadConfig()
	{
		if ( !FileSystem.Mounted.FileExists( ScpTest.filePath ) ) return new();

		var json = FileSystem.Mounted.ReadAllText( filePath );
		SectorsConfig = JsonSerializer.Deserialize<SectorsConfig>( json );

		return SectorsConfig;
	}



	[ConCmd.Server("generate_map")]
	public static void spawn()
	{
		ScpTest.clear_world();
		
		new Generator();
		ConsoleSystem.Caller.Pawn.Transform = new Transform().WithPosition( new Vector3( 0, 0, 0 ) );

		/*SectorsConfig = null;
		var config = ScpTest.Config;

		Log.Info( config );


		foreach ( Sector sector in config.Sectors )
		{
			sector.Generate();
		}

		var sun = new EnvironmentLightEntity
		{
			Position = new Vector3( 32, 0, 192 ),
			Rotation = Rotation.From( new Angles( 69.3447f, 103.642f, -103.642f ) ),
			//SunAngle = 2.125f
		};*/

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

		foreach ( var model in Entity.All.OfType<EnvironmentLightEntity>().ToArray() )
		{
			model.Delete();
		}

		Log.Info( "cleared world" );
	}

	[ConCmd.Server( "teleport" )]
	public static void teleport(float x, float y, float z)
	{
		ConsoleSystem.Caller.Pawn.Transform = new Transform().WithPosition( new Vector3( x, y, z ) );
	}
}
