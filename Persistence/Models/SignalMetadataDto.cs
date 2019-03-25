using SignalProcessing;
using System.Runtime.Serialization;

namespace Persistence.Models
{
    [DataContract(Name = "SignalMetadataDto")]
    public class SignalMetadataDto
    {
        public SignalMetadataDto(Types.SignalMetadata metadata)
        {
            Amplitude = metadata.amplitude;
            StartTime = metadata.startTime;
            Duration = metadata.duration;
            DutyCycle = metadata.dutyCycle;
            SignalFrequency = metadata.signalFrequency;
            SamplingFrequency = metadata.samplingFrequency;
            IsContinous = metadata.isContinous;
            SignalType = metadata.signalType;
        }

        [DataMember] public double Amplitude { get; set; }
        [DataMember] public double StartTime { get; set; }
        [DataMember] public double Duration { get; set; }
        [DataMember] public double DutyCycle { get; set; }
        [DataMember] public double SignalFrequency { get; set; }
        [DataMember] public double SamplingFrequency { get; set; }
        [DataMember] public bool IsContinous { get; set; }
        [DataMember] public Types.SignalType SignalType { get; set; }
    }
}
