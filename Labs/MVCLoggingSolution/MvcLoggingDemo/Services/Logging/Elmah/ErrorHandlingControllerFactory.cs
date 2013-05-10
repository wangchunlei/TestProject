using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using MvcLoggingDemo.Helpers;

namespace MvcLoggingDemo.Services
{
    /// <summary>
    /// This custom controller factory injects a custom attribute 
    /// on every action that is invoked by the controller
    /// </summary>
    public class ErrorHandlingControllerFactory : DefaultControllerFactory
    {
        /// <summary>
        /// Injects a custom attribute 
        /// on every action that is invoked by the controller
        /// </summary>
        /// <param name="requestContext">The request context</param>
        /// <param name="controllerName">The name of the controller</param>
        /// <returns>An instance of a controller</returns>
        public override IController CreateController(
            RequestContext requestContext,
            string controllerName)
        {
            var controller =
                base.CreateController(requestContext,
                controllerName);

            var c = controller as Controller;

            if (c != null)
            {
                c.ActionInvoker =
                    new ErrorHandlingActionInvoker(
                        new HandleErrorWithELMAHAttribute());
            }

            return controller;
        }
    }
}