namespace IronBeard.Core.Features.Razor;

public interface IRazorViewRenderer
{
    Task<string> RenderAsync<T>(string viewPath, T model);
}
