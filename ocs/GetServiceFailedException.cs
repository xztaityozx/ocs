using System.Runtime.Serialization;

namespace ocs;
[Serializable]
public class GetServiceFailedException : Exception
{
    public GetServiceFailedException(Type service) : base($"failed to get service {service}") { }
    protected GetServiceFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
