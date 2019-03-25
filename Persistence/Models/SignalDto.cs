using SignalProcessing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Persistence.Models
{
    [DataContract(Name = "SignalDto", IsReference = true)]
    public class SignalDto
    {
        public SignalDto(Types.Signal signal)
        {
            Points = signal.points.Select(p => new PointDto(p)).ToList();
            Metadata = new SignalMetadataDto(signal.metadata);
        }

        [DataMember] public List<PointDto> Points { get; set; }
        [DataMember] public SignalMetadataDto Metadata { get; set; }
    }
}
