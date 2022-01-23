using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace IronBeard.Core.Features.Razor;

/// <summary>
/// AspNetCore MVC Razor to String renderer. This leverages the IRazorViewEngine
/// to find and render view files to a string
/// </summary>
public class RazorViewToStringRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public RazorViewToStringRenderer(IRazorViewEngine viewEngine,ITempDataProvider tempDataProvider,IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Renders the view at the given view path using the given model as the view's
    /// @model declaration
    /// </summary>
    /// <param name="viewPath">Relative path of view</param>
    /// <param name="model">Model to render in view</param>
    /// <typeparam name="T">Type of model</typeparam>
    /// <returns>Rendered string</returns>
    public async Task<string> RenderViewToStringAsync<T>(string viewPath, T model)
    {
        var actionContext = GetActionContext();
        var view = FindView(viewPath);

        using var output = new StringWriter();
        var viewContext = GetViewContext(model, actionContext, output, view);
        await view.RenderAsync(viewContext);
        return output.ToString();
    }

    /// <summary>
    /// Gets the current ActionContext. In our case, most of the context is empty,
    /// but the view engine requires it.
    /// </summary>
    /// <returns>Action Context</returns>
    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider
        };
        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }

    /// <summary>
    /// Handles finding the view with the given action context and expected view path.
    /// In our case, we only look for the view at the path given, we don't search for it
    /// anywhere else.
    /// </summary>
    /// <param name="viewPath"></param>
    /// <returns></returns>
    private IView FindView(string viewPath)
    {
        // Get the view at the given path
        var getViewResult = _viewEngine.GetView(null, viewPath, true);
        if (getViewResult.Success)
            return getViewResult.View;

        // throw if we couldn't find it. This is a critical issue
        throw new InvalidOperationException("Unable to find View");
    }

    /// <summary>
    /// Get's the correct view context for writing out to string.
    /// </summary>
    /// <param name="model">Model of the view's data</param>
    /// <param name="action">Current ActionContext</param>
    /// <param name="output">StringWriter for writing the view to string</param>
    /// <param name="view">View to render</param>
    /// <typeparam name="T">Type of model</typeparam>
    /// <returns>View Context</returns>
    private ViewContext GetViewContext<T>(T model, ActionContext action, StringWriter output, IView view){
        var viewDataDictionary = new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
        var tempDataDictionary = new TempDataDictionary(action.HttpContext, _tempDataProvider);
        return new ViewContext(action, view, viewDataDictionary, tempDataDictionary, output, new HtmlHelperOptions());
    }
}