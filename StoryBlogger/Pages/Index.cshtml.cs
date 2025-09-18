using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoryBlogger.Pages
{

    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;

        public string Story { get; set; }

        public IndexModel(IConfiguration config)
        {
            _config = config;
        }

        public async Task OnGetAsync()
        {
            var exifService = new ExifService();
            var locationService = new LocationService();
            var albumMetadata = await exifService.ExtractAlbumMetadata("wwwroot/images/UploadedAlbum", locationService);

            var promptBuilder = new PromptBuilder();
            var prompt = promptBuilder.BuildPrompt(albumMetadata);

            var openAIService = new OpenAIService(_config);
            Story = await openAIService.GenerateStoryAsync(prompt);
        }
    }

}
