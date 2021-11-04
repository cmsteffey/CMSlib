using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace CMSlib.HttpUtilities
{
    public class Utf8StringResponse
    {
        public string Content { get; internal init; }
        public HttpResponseMessage Base { get; internal init; }

        internal Utf8StringResponse()
        {
        }

        public static implicit operator Utf8StringResponse(HttpResponseMessage @base)
        {
            Stream stream = @base.Content.ReadAsStream();
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return new Utf8StringResponse()
            {
                Base = @base,
                Content = Encoding.UTF8.GetString(bytes)
            };
        }

        public static implicit operator string(Utf8StringResponse resp) => resp.Content;
        public override string ToString() => this;
    }

    public static class Utf8StringResponseExtensions
    {
        public static Utf8StringResponse AsStringResponse(this HttpResponseMessage resp) => resp;

        public static T? ParseAsJson<T>(this string obj)
        {
            return JsonSerializer.Deserialize<T>(obj);
        }

        public static T? ParseAsJson<T>(this HttpResponseMessage resp)
        {
            return resp.AsStringResponse().Content.ParseAsJson<T>();
        }
    }
}