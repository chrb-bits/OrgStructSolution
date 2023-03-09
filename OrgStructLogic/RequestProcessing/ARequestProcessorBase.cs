using OrgStructModels.Protocol;

namespace OrgStructLogic.RequestProcessing
{
    public abstract class ARequestProcessorBase<I, O> : IRequestProcessor<I, O>
        where I : IRequest
        where O : IResult
    {
        public abstract O Process(I request);
    }
}
