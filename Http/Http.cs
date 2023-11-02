using Stenguage.Errors;
using Stenguage.Json;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;
using System.Net;

namespace Http
{
    public class Http
    {
        public static RuntimeResult get(Context ctx,
                                        StringValue url)
        {
            RuntimeResult res = new RuntimeResult();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync(url.Value).Result;
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return res.Success(new ObjectValue(new Dictionary<string, RuntimeValue>
                    {
                        ["StatusCode"] = new NumberValue((int)response.StatusCode),
                        ["Content"] = new StringValue(response.Content.ReadAsStringAsync().Result),
                        ["IsSuccess"] = new BooleanValue(response.IsSuccessStatusCode)
                    }));
                }
                catch (Exception ex)
                {
                    return res.Failure(new Error(ex.Message, ctx.Env.SourceCode, ctx.Start, ctx.End));
                }
            }
        }

        public static RuntimeResult post(Context ctx,
                                        StringValue url, ObjectValue body)
        {
            RuntimeResult res = new RuntimeResult();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Console.WriteLine(body.Properties.ToJson());
                    HttpResponseMessage response = client.PostAsync(url.Value, new StringContent(body.Properties.ToJson())).Result;
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return res.Success(new ObjectValue(new Dictionary<string, RuntimeValue>
                    {
                        ["StatusCode"] = new NumberValue((int)response.StatusCode),
                        ["Content"] = new StringValue(response.Content.ReadAsStringAsync().Result),
                        ["IsSuccess"] = new BooleanValue(response.IsSuccessStatusCode)
                    }));
                }
                catch (Exception ex)
                {
                    return res.Failure(new Error(ex.Message, ctx.Env.SourceCode, ctx.Start, ctx.End));
                }
            }
        }

    }
}