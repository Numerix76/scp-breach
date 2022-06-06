using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP;

public partial class LightData
{
	public bool Enabled { get; set; } = true;
	public bool DynamicShadows { get; set; } = false;
    public float Range { get; set; } = 512f;
    public float Falloff { get; set; } = 1f;
    public float LinearAttenuation { get; set; } = 0f;
    public float QuadraticAttenuation { get; set; } = 1f;
    public float Brightness { get; set; } = 10f;
    public Color Color { get; set; } = Color.White;
    public float FogStrength { get; set; } = 1f;
    public float InnerConeAngle { get; set; } = 45f;
    public float OuterConeAngle { get; set; } = 60f;
	public Vector3 Position { get; set; } = Vector3.Zero;
	public Angles Orientation { get; set; } = Angles.Zero;
}
