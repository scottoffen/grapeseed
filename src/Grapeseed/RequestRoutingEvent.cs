namespace Grapevine;

public class RequestRoutingEvent : List<RoutingAsyncEventHandler>
{
    public async Task<int> Invoke(IHttpContext context)
    {
        var counter = 0;
        foreach (var handler in this)
        {
            await handler(context);
            counter++;
            if (context.WasRespondedTo) break;
        }
        return counter;
    }

    public static RequestRoutingEvent operator +(RequestRoutingEvent source, RoutingAsyncEventHandler obj)
    {
        source.Add(obj);
        return source;
    }

    public static RequestRoutingEvent operator -(RequestRoutingEvent source, RoutingAsyncEventHandler obj)
    {
        source.Remove(obj);
        return source;
    }
}
