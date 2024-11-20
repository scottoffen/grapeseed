namespace Grapeseed;

public class RequestReceivedEvent : List<RequestReceivedAsyncEventHandler>
{
    public async Task<int> Invoke(IHttpContext context, IRestServer server)
    {
        var counter = 0;
        foreach (var handler in this)
        {
            await handler(context, server);
            counter++;
            if (context.WasRespondedTo) break;
        }
        return counter;
    }

    public static RequestReceivedEvent operator +(RequestReceivedEvent source, RequestReceivedAsyncEventHandler obj)
    {
        source.Add(obj);
        return source;
    }

    public static RequestReceivedEvent operator -(RequestReceivedEvent source, RequestReceivedAsyncEventHandler obj)
    {
        source.Remove(obj);
        return source;
    }
}
