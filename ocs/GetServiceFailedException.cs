namespace ocs;

public class GetServiceFailedException : Exception
{
    public GetServiceFailedException(Type service) : base($"failed to get service {service}") { }
}
