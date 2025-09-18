public class PromptBuilder
{
    public string BuildPrompt(List<PhotoMetadata> albumMetadata)
    {
        var locations = string.Join(", ", albumMetadata.Select(m => m.Location).Distinct());
        var dates = string.Join(", ", albumMetadata.Select(m => m.DateTaken).Distinct());
        var tags = string.Join(", ", albumMetadata.SelectMany(m => m.Tags).Distinct());
        var people = string.Join(", ", albumMetadata.SelectMany(m => m.People).Distinct());

        return $"Write a creative narrative under 300 words about an event or trip based on these photos. " +
               $"Locations: {locations}. Dates: {dates}. Tags: {tags}. People: {people}. " +
               $"Focus on the experience, people involved, and the story these photos tell.";
    }
}