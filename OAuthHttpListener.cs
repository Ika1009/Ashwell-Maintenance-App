using System;
using System.Net;
using System.Threading.Tasks;

public class OAuthHttpListener
{
    private HttpListener _listener;
    private string _authorizationCode;

    public OAuthHttpListener(string uri)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(uri);
    }

    public async Task<string> StartListeningAsync()
    {
        _listener.Start();
        Console.WriteLine("Listening for OAuth redirect...");

        var context = await _listener.GetContextAsync();
        var request = context.Request;

        // Extract the code from the query string
        var queryParams = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
        _authorizationCode = queryParams["code"];

        // You can now respond to the request to close the browser window
        var response = context.Response;
        string responseString = "<html><body>You can close this window now.</body></html>";
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        responseOutput.Write(buffer, 0, buffer.Length);
        responseOutput.Close();

        _listener.Stop();

        return _authorizationCode;
    }
}
