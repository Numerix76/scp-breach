using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SCP.Generation;
internal class SectorsConfig
{
	public List<Sector> Sectors { get; set; } = new();
}

public class Sector
{
	public string Name { get; set; }
	public string ShortName { get; set; }
	public string ConfigFile { get; set; } = "";
	public Vector3 StartPosition { get; set; } = new();
	public Map Map { get; set; }

	private RoomsConfig loadedConfig;

	public RoomsConfig Config
	{
		get
		{
			if ( loadedConfig == null )
				loadedConfig = LoadConfig().Result;
			return loadedConfig;
		}
	}

	public async Task<RoomsConfig> LoadConfig()
	{
		if ( !FileSystem.Mounted.FileExists( ConfigFile ) ) return new();

		var json = FileSystem.Mounted.ReadAllText( ConfigFile );
		loadedConfig = JsonSerializer.Deserialize<RoomsConfig>( json );

		return loadedConfig;
	}

	public void Generate()
	{
		var config = Config;

		for(int y = 0; y < Map.Pattern.Count ; y++)
		{
			for( int x = 0; x < Map.Pattern[y].Count ; x++ )
			{
				var roomInfo = Map.Pattern[y][x];
				Log.Info( $"{x}:{y} = {roomInfo}" );

				if ( roomInfo.Equals( "" ) ) continue;

				Regex regex = new Regex( "-" );
				string[] substrings = regex.Split( roomInfo );
				var type = substrings[0];
				int orientation =  int.Parse(substrings[1]);

				Log.Info( $"{type} {orientation}" );

				var room = FindRoomByType( type );

				if ( room == null )
				{
					Log.Info( $"{type} introuvable" );
					continue;
				}


				var model = Model.Load( room.Model );

				if ( model == null || model.IsError )
				{
					Log.Info( $"{type} modèle introuvable" );
					continue;
				}

				room.Sector = this;
				if ( room.StaticSpawn )
				{
					room.Prop = new Prop
					{
						Position = StartPosition + room.Position,
						Rotation = Rotation.From( room.Orientation ),
						Model = model,
						Name = room.Name
					};
				}
				else
				{
					float sizeX = 628.1288f;//model.Bounds.Size.x;
					float sizeY = 628.1288f;//model.Bounds.Size.x;

					Log.Info( sizeY.ToString() );

					var pos = new Vector3( -sizeX * x, sizeY * y, 0 );
					room.Prop = new Prop
					{
						Position = StartPosition + pos,
						Rotation = Rotation.From( new Angles(0, 180-90*orientation, 0 ) ),
						Model = model,
						Name = room.Name
					};
				}

				room.Prop.SetupPhysicsFromModel( PhysicsMotionType.Static );

				room.GenerateLights();
			}
		}
	}

	private Room FindRoomByType(string type)
	{
		foreach(Room room in Config.Rooms)
		{
			if ( room.Type == type )
				return (Room) room.Clone();
		}

		return null;
	}
}
