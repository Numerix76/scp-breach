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

	private int xEZ { get; set; }
	private int yEZ { get; set; }
	private int oriEZ { get; set; }

	public RoomsConfig Config
	{
		get
		{
			if ( loadedConfig == null )
				loadedConfig = Generator.LoadConfig<RoomsConfig>( ConfigFile );
			return loadedConfig;
		}
	}

	public async Task Generate()
	{
		Log.Info( $"Génération de {ShortName}" );

		if ( ShortName.Equals( "hcz" ) )
			DetermineEZ();

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

				Log.Info( $"{room?.RoomData.Type} {y}:{x}" );
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
					var pos = CalculatePosition(x, y);
					position = StartPosition + pos;
					rotation = Rotation.From( CalculateAngles(orientation) );
				}

				GenerateRoom( room, position, rotation );

				await Task.Delay( 100 );
			}
		}

		VerifyAndReplace();
	}

	private Vector3 CalculatePosition(int x, int y)
	{
		if ( !ShortName.Equals("hcz") )
			return new Vector3( -Room.Size * x, Room.Size * y, 0 );

		switch ( oriEZ )
		{
			case 1: return new Vector3( Room.Size * (Map.Pattern.Count - y - 1), Room.Size * (Map.Pattern[0].Count - x - xEZ), 0 );
			case 2: return new Vector3( Room.Size * x, Room.Size * (yEZ - y), 0 );
			case 3: return new Vector3( Room.Size * y, Room.Size * (x - xEZ), 0 );
			case 4: return new Vector3( Room.Size * (Map.Pattern[0].Count - x - 1), Room.Size * (y - yEZ), 0 );
		}

		return Vector3.Zero;
	}

	private Angles CalculateAngles(int orientation)
	{
		if ( !ShortName.Equals( "hcz" ) )
			return new Angles( 0, 180 - 90 * orientation, 0 );

		switch ( oriEZ )
		{
			case 1: return new Angles( 0, 180 - 90 * orientation + 90, 0 );
			case 2: return new Angles( 0, 180 - 90 * orientation - 180, 0 );
			case 3: return new Angles( 0, 180 - 90 * orientation - 90, 0 );
			case 4: return new Angles( 0, 180 - 90 * orientation, 0 );
		}

		return Angles.Zero;
	}

	private void DetermineEZ()
	{
		Log.Info( "Je commence à chercher" );
		Regex regexType = new Regex( "SS" );
		Regex regexData = new Regex( "-" );

		var type = "";
		do
		{
			int x = Random.Next( Map.Pattern[0].Count );
			int y = Random.Next( Map.Pattern.Count );

			string[] substrings = regexData.Split( Map.Pattern[y][x] );
			type = substrings[0];

			Log.Info( substrings );
			Log.Info( Map.Pattern[y][x] );
			if ( type.Equals( "" ) )
				continue;

			int orientation = int.Parse( substrings[1] );

			if ( regexType.IsMatch( type ) )
			{
				Map.Pattern[y][x] = "EZ-1";

				xEZ = x;
				yEZ = y;
				oriEZ = orientation;

				Log.Info( Map.Pattern[y][x] );
			}
		} while ( !regexType.IsMatch( type ) );

		Log.Info( "J'ai trouvé et remplacé" );

		/*Map.Pattern[0][2] = "EZ-3";

		xEZ = 2;
		yEZ = 0;
		oriEZ = 3;*/
	}

	private void GenerateRoom( Room room, Vector3 position, Rotation rotation )
	{
		Log.Info( $"Génération de {room.RoomData.Name}" );
		room.Prop = new Prop
		{
			Position = position,
			Rotation = rotation,
			Model = Model.Load(room.RoomData.Model),
			Name = room.RoomData.Name
		};

		if ( room.RoomData.Type.Equals( "EZ" ) )
			Log.Info( position );

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

		Log.Info( $"Remplacement d'une pièce {roomData.Type}" );
		for ( int y = 0; y < Map.Rooms.Count; y++ )
		{
			for ( int x = 0; x < Map.Rooms[y].Count; x++ )
			{
				if ( Map.Rooms[y][x] == null || Map.Rooms[y][x].RoomData.Force || !Map.Rooms[y][x].RoomData.Type.Equals(room.RoomData.Type) )
					continue;
				
				Vector3 position = Map.Rooms[y][x].Prop.Position;
				Rotation rotation = Map.Rooms[y][x].Prop.Rotation;

				GenerateRoom( room, position, rotation );

				Map.Rooms[y][x].Delete();
				Map.Rooms[y][x] = room;

				Log.Info( $"{y}:{x}" );

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
