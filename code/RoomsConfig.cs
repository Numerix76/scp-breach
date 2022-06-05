using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Config;

public partial class RoomsConfig
{
	public List<Room> rooms { get; set; } = new();
}

public class Room
{
	public string name { get; set; }
	public string model { get; set; }
	public Boolean staticSpawn { get; set; } = false;
	public Vector3 position { get; set; } = Vector3.Zero;
	public Angles orientation { get; set; } = Angles.Zero;
	public List<Light> lights { get; set; } = new();

}
