using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SCP.Generation;
internal class SectorConfig
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

	private readonly Random Random = new( (int) new DateTimeOffset( DateTime.UtcNow ).ToUnixTimeSeconds() );

	public RoomsConfig Config
	{
		get
		{
			if ( loadedConfig == null )
				loadedConfig = Generator.LoadConfig<RoomsConfig>( ConfigFile );
			return loadedConfig;
		}
	}

	public void Generate()
	{
		var config = Config;

		for ( int y = 0; y < Map.Pattern.Count; y++ )
		{
			var ensRooms = new List<Room>();
			this.Map.Rooms.Add( ensRooms );

			for ( int x = 0; x < Map.Pattern[y].Count; x++ )
			{
				var roomInfo = Map.Pattern[y][x];

				if ( roomInfo.Equals( "" ) ) continue;

				Regex regex = new Regex( "-" );
				string[] substrings = regex.Split( roomInfo );
				var type = substrings[0];
				int orientation = int.Parse( substrings[1] );

				Room room = Room.Create( FindRoomByType( type ) );

				ensRooms.Add( room );

				if ( room == null )
				{
					Log.Info( $"{type} introuvable" );
					continue;
				}			

				room.Sector = this;

				Vector3 position;
				Rotation rotation;
				if ( room.RoomData.StaticSpawn )
				{
					position = StartPosition + room.RoomData.StaticPosition;
					rotation = Rotation.From( room.RoomData.StaticOrientation );
				}
				else
				{ 
					var pos = new Vector3( -Room.Size * x, Room.Size * y, 0 );
					position = StartPosition + pos;
					rotation = Rotation.From( new Angles( 0, 180 - 90 * orientation, 0 ) );
				}

				GenerateRoom( room, position, rotation );
			}
		}

		VerifyAndReplace();
	}

	private void GenerateRoom( Room room, Vector3 position, Rotation rotation )
	{
		room.Prop = new Prop
		{
			Position = position,
			Rotation = rotation,
			Model = Model.Load(room.RoomData.Model),
			Name = room.RoomData.Name
		};

		room.Prop.SetupPhysicsFromModel( PhysicsMotionType.Static );

		room.GenerateLights();
	}

	private void VerifyAndReplace()
	{
		foreach(RoomData roomData in Config.Rooms)
		{
			if ( roomData.Force && roomData.NbSpawn < roomData.Limit )
			{
				ReplaceRoom( roomData );
				roomData.NbSpawn++;
			}
		}		
	}

	private void ReplaceRoom(RoomData roomData)
	{
		Room room = Room.Create(roomData);

		Log.Info( "Remplacement d'une pièce" );
		for ( int y = 0; y < Map.Rooms.Count; y++ )
		{
			for ( int x = 0; x < Map.Rooms[y].Count; x++ )
			{
				if ( Map.Rooms[y][x] == null || Map.Rooms[y][x].RoomData.Force || !Map.Rooms[y][x].RoomData.Type.Equals(room.RoomData.Type ) )
					continue;
				
				Vector3 position = Map.Rooms[y][x].Prop.Position;
				Rotation rotation = Map.Rooms[y][x].Prop.Rotation;

				GenerateRoom( room, position, rotation );

				Map.Rooms[y][x].Delete();
				Map.Rooms[y][x] = room;

				return; // On à réussi à remplacer
			}
		}
	}

	private RoomData FindRoomByType(string type)
	{
		List<RoomData> roomsData = Config.Rooms.FindAll( r => r.Type == type && (r.Limit == -1 || r.NbSpawn < r.Limit) );

		if ( roomsData.Count == 0 )
			return null;

		int indRoom = Random.Next( 0, roomsData.Count );

		RoomData roomData = roomsData[indRoom];

		roomData.NbSpawn++;

		return roomData;
	}
}
