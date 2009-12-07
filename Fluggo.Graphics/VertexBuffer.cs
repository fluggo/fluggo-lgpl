using System;
using System.Runtime.InteropServices;

namespace Fluggo.Graphics.Common {
	[Flags]
	public enum SetDataOptions {
		None = 0,
		NoOverwrite = 1,
		Discard = 2
	}
}