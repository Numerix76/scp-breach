using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace SCP.Generation;

internal class Generator
{
	public string MapConfigFile { get; set; }
	private SectorsConfig sectorsConfig { get; set; }

	public SectorsConfig SectorsConfig
	{
		get
		{
			if ( sectorsConfig == null )
				sectorsConfig = LoadConfig<SectorsConfig>("config/room_config.json").Result;
			return sectorsConfig;
		}
	}

	public async Task<T> LoadConfig<T>(string file)
	{
		if ( !FileSystem.Mounted.FileExists( file ) ) return default(T);

		Log.Info( $"Le fichier {file} existe" );
		var json = FileSystem.Mounted.ReadAllText( file );
		T config = JsonSerializer.Deserialize<T>( json );

		return config;
	}

	public Generator() : this( "config/ez_map_01.json" ) {}

	public Generator(string map)
	{
		this.MapConfigFile = map;

		generatePatterns();
		generateSectors();
	}

	private void generatePatterns()
	{
		foreach( Sector sector in SectorsConfig.Sectors )
		{
			sector.Map = LoadConfig<Map>( $"config/{sector.ShortName}_map_01.json" ).Result;

			Log.Info( sector.Map );
		}
	}

	private void generateSectors()
	{
		foreach ( Sector sector in SectorsConfig.Sectors )
		{
			sector.Generate();
		}
	}
}

public class Map
{
	public List<List<string>> Pattern { get; set; } = new();

	public List<Room> Rooms { get; set; }

	public string toString()
	{
		return Pattern.ToString();
	}
}

