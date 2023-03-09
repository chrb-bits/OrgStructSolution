using Newtonsoft.Json;

namespace OrgStructModels.Protocol
{
    public abstract class AResultBase : IResult
    {
        public AResultBase() { }

        public AResultBase(bool success, string message = "")
        {
            Success = success;
            Message = message;
        }


        [JsonProperty(Order = -10)]
        public bool Success { set; get; }

        public string Message { set; get; }
    }
}
