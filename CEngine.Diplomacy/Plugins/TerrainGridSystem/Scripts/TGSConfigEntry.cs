using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TGS
{

	[Serializable]
	public struct TGSConfigEntry
	{
		public bool visible;
		public int territoryIndex;
		public Color color;
		public int textureIndex;
		public int tag;
		public bool canCross;
		public float crossCost;
	}

}