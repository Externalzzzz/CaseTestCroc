namespace WebDbMonitoring.Models
{
    public class ChartModel
    {
        public List<string> Labels { get; set; } 
        public List<int> Values { get; set; }
        public ChartModel(List<string> labels, List<int> values)
        {
            this.Labels = labels;
            this.Values = values;
        }
    }
}
