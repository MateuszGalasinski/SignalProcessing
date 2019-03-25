using Microsoft.FSharp.Collections;
using Persistence.Models;
using SignalProcessing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Persistence
{
    public class XmlSerializer
    {
        private readonly DataContractSerializer _serializer = new DataContractSerializer(typeof(SignalDto));

        public void Serialize(Types.Signal signal, string filePath)
        {
            using (FileStream writer = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                _serializer.WriteObject(writer, new SignalDto(signal));
            }
        }

        public Types.Signal Deserialize(string filePath)
        {
            using (TextReader reader = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                string text = reader.ReadToEnd();
            }
            using (FileStream reader = new FileStream(filePath, FileMode.Open))
            {
                return MapBackDto((SignalDto)_serializer.ReadObject(reader));
            }
        }

        private Types.Signal MapBackDto(SignalDto dto)
        {
            var cSharpPointList =
                dto.Points.Select(p => new Types.Point(p.X, new Types.Complex(p.Y.R, p.Y.I)));
            FSharpList<Types.Point> points = ListModule.OfSeq(cSharpPointList);

            return new Types.Signal(
                points,
                new Types.SignalMetadata(
                    dto.Metadata.SignalType,
                    dto.Metadata.IsContinous,
                    dto.Metadata.Amplitude,
                    dto.Metadata.StartTime,
                    dto.Metadata.Duration,
                    dto.Metadata.DutyCycle,
                    dto.Metadata.SignalFrequency,
                    dto.Metadata.SamplingFrequency
                ));
        }
    }
}
