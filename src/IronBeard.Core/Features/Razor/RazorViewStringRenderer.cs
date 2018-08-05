using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace IronBeard.Core.Features.Razor
{
    public class RazorViewToStringRenderer
    {
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(IRazorViewEngine viewEngine,ITempDataProvider tempDataProvider,IServiceProvider serviceProvider)
        {
            this._viewEngine = viewEngine;
            this._tempDataProvider = tempDataProvider;
            this._serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<T>(string viewPath, T model)
        {
            var actionContext = GetActionContext();
            var view = FindView(actionContext, viewPath);

            using (var output = new StringWriter())
            {
                var viewContext = this.GetViewContext(model, actionContext, output, view);
                await view.RenderAsync(viewContext);
                return output.ToString();
            }
        }

        private IView FindView(ActionContext actionContext, string viewPath)
        {
            //var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: "~/Articles/RazorTest.cshtml", isMainPage: true);
            var getViewResult = _viewEngine.GetView(null, viewPath, true);
            if (getViewResult.Success)
                return getViewResult.View;

            throw new InvalidOperationException("Unable to find View");
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        private ViewContext GetViewContext<T>(T model, ActionContext action, StringWriter output, IView view){
            var viewDataDictionary = new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
            var tempDataDictionary = new TempDataDictionary(action.HttpContext, _tempDataProvider);
            return new ViewContext(action, view, viewDataDictionary, tempDataDictionary, output, new HtmlHelperOptions());
        }
    }
}