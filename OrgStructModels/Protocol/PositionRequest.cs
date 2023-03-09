using OrgStructModels.Persistables;

namespace OrgStructModels.Protocol
{
    public class PositionRequest :  ARequestBase
    {
        public PositionModel Parent { set; get; }
        public PositionModel Position { set; get; }
    }
}
