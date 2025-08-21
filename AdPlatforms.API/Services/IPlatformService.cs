namespace AdPlatforms.API.Services;

public interface IPlatformService
{
    Task LoadPlatformsAsync(Stream fileStream);
    
    IEnumerable<string>  FindPlatforms(string location);
}
