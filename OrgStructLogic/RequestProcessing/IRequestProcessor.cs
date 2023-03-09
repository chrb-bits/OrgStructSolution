using OrgStructModels.Protocol;

namespace OrgStructLogic.RequestProcessing
{
    public interface IRequestProcessor<in I, out O>
        where I : IRequest
        where O : IResult
    {
        O Process(I request);
    }
}
