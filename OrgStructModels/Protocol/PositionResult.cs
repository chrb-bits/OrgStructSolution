using OrgStructModels.Persistables;

namespace OrgStructModels.Protocol
{
    public class PositionResult : AResultBase
    {
        public PositionResult() : base() { }

        public PositionResult(bool success, PositionModel position) : base(success)
        {
            Position = position;
        }

        public PositionModel Position { set; get; }
    }
}
