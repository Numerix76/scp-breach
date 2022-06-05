using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP;

public partial class Light : SpotLightEntity, ICloneable
{
	public Angles Orientation { get; set; } = Angles.Zero;

	public object Clone()
	{
		return this.MemberwiseClone();
	}

}
