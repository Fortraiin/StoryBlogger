public class PhotoMetadata
{
    public string DateTaken { get; set; }
    public string Location { get; set; }

    public List<string> Tags { get; set; } = new();
    public List<string> People { get; set; } = new();
}
