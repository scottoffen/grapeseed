namespace Grapevine.Exceptions;

public class DuplicateRouteConstraintKey : GrapevineException
{
    public DuplicateRouteConstraintKey(string key) : base($"Duplicate key {key}: A given route can only use a given key once") { }
}
