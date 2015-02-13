using ModInstallation.Interfaces;

namespace ModInstallation.Implementations.Extractors
{
    public class DefaultExtractionProgress : IExtractionProgress
    {
        public string FileName { get; set; }

        public double OverallProgress { get; set; }
    }
}