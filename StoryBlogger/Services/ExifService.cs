using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Xmp;
using System.Globalization;
using Directory = System.IO.Directory;

public class ExifService
{
    public async Task<List<PhotoMetadata>> ExtractAlbumMetadata(string folderPath, LocationService locationService)
    {
        var metadataList = new List<PhotoMetadata>();
        var imageFiles = Directory.GetFiles(folderPath, "*.jpg");

        foreach (var imagePath in imageFiles)
        {
            var metadata = await ExtractMetadata(imagePath, locationService);
            if (metadata != null)
                metadataList.Add(metadata);
        }

        return metadataList;
    }

    public async Task<PhotoMetadata> ExtractMetadata(string imagePath, LocationService locationService)
    {
        var directories = ImageMetadataReader.ReadMetadata(imagePath);
        var metadata = new PhotoMetadata();

        // Date Taken
        var exifDir = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        metadata.DateTaken = exifDir?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal) ?? "";

        // GPS Location
        var gpsDir = directories.OfType<GpsDirectory>().FirstOrDefault();
        var location = gpsDir?.GetGeoLocation();
        if (location != null)
        {
            var coordinates = $"{location.Latitude.ToString(CultureInfo.InvariantCulture)},{location.Longitude.ToString(CultureInfo.InvariantCulture)}";
            var streetName = await locationService.GetStreetNameAsync(coordinates);
            metadata.Location = !string.IsNullOrWhiteSpace(streetName) ? streetName : coordinates;
        }
        else
        {
            metadata.Location = "";
        }

        // IPTC Tags (Keywords)
        var iptcDir = directories.OfType<IptcDirectory>().FirstOrDefault();
        if (iptcDir != null && iptcDir.ContainsTag(IptcDirectory.TagKeywords))
        {
            var tags = iptcDir.GetStringArray(IptcDirectory.TagKeywords);
            if (tags != null)
                metadata.Tags.AddRange(tags.Where(t => !string.IsNullOrWhiteSpace(t)));
        }

        // XMP Tags (for People, etc.)
        var xmpDir = directories.OfType<XmpDirectory>().FirstOrDefault();
        if (xmpDir != null)
        {
            var xmpMeta = xmpDir.XmpMeta;
            if (xmpMeta != null)
            {
                // People (if stored in XMP, e.g., MicrosoftPhoto:LastKeywordXMP)
                var people = xmpMeta.Properties
                    .Where(p => p.Path != null && p.Path.ToLower().Contains("person"))
                    .Select(p => p.Value)
                    .Where(v => !string.IsNullOrWhiteSpace(v));
                metadata.People.AddRange(people);
            }
        }

        // Fallback: If people are stored as tags with "Person:" prefix
        metadata.People.AddRange(metadata.Tags.Where(t => t.StartsWith("Person:", StringComparison.OrdinalIgnoreCase))
                                              .Select(t => t.Replace("Person:", "", StringComparison.OrdinalIgnoreCase).Trim()));

        return metadata;
    }
}
