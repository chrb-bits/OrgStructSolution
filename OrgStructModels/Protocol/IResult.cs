namespace OrgStructModels.Protocol
{
    public interface IResult
    {
        bool Success { set; get; }

        string Message { set; get; }
    }
}
