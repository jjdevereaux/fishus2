using SpacetimeDB.Types;
using UnityEngine;

namespace SpacetimeDB.Types
{
	public partial class DbVector3
	{
		public static implicit operator Vector3(DbVector3 vec)
		{
			return new Vector3(vec.X, vec.Y, vec.Z);
		}

		public static implicit operator DbVector3(Vector3 vec)
		{
			return new DbVector3(vec.x, vec.y, vec.z);
		}
	}
}