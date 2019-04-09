namespace PlotsVisualizer.Models
{
    public class ErrorResults
    {
        public double SamplingFrequency { get; set; }
        public string MSE { get; set; }
        public string SNR { get; set; }
        public string PSNR { get; set; }
        public string MD { get; set; }
        public int NeighoursCount { get; set; }
    }
}
