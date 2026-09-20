namespace Calendary.Application;

/// Marker type for MediatR's assembly scanning (`RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly)`)
/// — avoids pinning the registration to an arbitrary handler type.
public sealed class AssemblyReference;
