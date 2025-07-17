using PartialClassExtGen.Abstractions.Common;
using System;

namespace PartialClassExtGen.GenalyzerBase
{
	/// <summary>
	/// Represents an abstract base class for partial class extenders that work with a specific attribute type.
	/// </summary>
	/// <typeparam name="TAttribute">The type of attribute associated with the partial class extender. Must derive from <see cref="Attribute"/>.</typeparam>
	public abstract partial class VanillaPCEG<TAttribute>
		: VanillaPCEG<TAttribute, IPartialClassExtender, IPCEGDiagnostics>
		where TAttribute : Attribute
	{
		// Just a simple alias for the base class
	}
}
