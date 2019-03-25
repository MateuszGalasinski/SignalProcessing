using SignalProcessing;
using System.Runtime.Serialization;

namespace Persistence.Models
{
    [DataContract(Name = "ComplexDto")]
    public class ComplexDto
    {
        public ComplexDto(Types.Complex complex)
        {
            R = complex.r;
            I = complex.i;
        }

        [DataMember] public double R { get; set; }
        [DataMember] public double I { get; set; }
    }
}
