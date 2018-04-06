using UnityEngine;
using System.Reflection;

namespace Klak.Wiring
{
	[AddComponentMenu("Klak/Wiring/Output/Generic/Console Out")]
	public class ConsoleOut : NodeBase
	{
		#region Node I/O

		[Inlet]
		public void Bang() {
			Debug.Log(name + " Bang!");
		}

		[Inlet]
		public float number {
			set {
				Debug.Log(name + " " + value);
			}
		}

		[Inlet]
		public Vector3 vector {
			set {
				Debug.Log(name + " Vector3" + value);
			}
		}

		[Inlet]
		public Quaternion rotation {
			set {
				Debug.Log(name + " Quaternion" + value);
			}
		}

		[Inlet]
		public Color color {
			set {
				Debug.Log(name + " " + value);
			}
		}

		#endregion
	}
}
