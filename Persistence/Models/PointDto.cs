using SignalProcessing;
using System.Runtime.Serialization;

namespace Persistence.Models
{
    [DataContract(Name = "PointDto", IsReference = true)]
    public class PointDto
    {
        public PointDto(Types.Point point)
        {
            X = point.x;
            Y = new ComplexDto(point.y);
        }

        [DataMember] public double X { get; set; }
        [DataMember] public ComplexDto Y { get; set; }
    }
}
