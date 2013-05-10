using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace System.Web.Mvc
{
    /// <summary>
    /// Credits : This class is from the NerdDinner sample application
    /// </summary>
    /// <see cref="http://www.nerddinner.com/"/>
    public static class UrlHelperExtensions
    {
        internal static Uri ActionFull(this UrlHelper urlHelper, string actionName)
        {
            return new Uri(HttpContext.Current.Request.Url, urlHelper.Action(actionName));
        }

        internal static Uri ActionFull(this UrlHelper urlHelper, string actionName, string controllerName)
        {
            return new Uri(HttpContext.Current.Request.Url, urlHelper.Action(actionName, controllerName));
        }

        /// <summary>
        /// Returns an absolute url for an action
        /// </summary>
        /// <param name="url">UrlHelper</param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static string AbsoluteAction(this UrlHelper url, string action, object routeValues)
        {
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;

            string absoluteAction = string.Format("{0}://{1}{2}",
                                                  requestUrl.Scheme,
                                                  requestUrl.Authority,
                                                  url.Action(action, routeValues));

            return absoluteAction;
        }

        ///// <summary>
        ///// Returns an absolute url for an action
        ///// </summary>
        ///// <param name="url">UrlHelper</param>
        ///// <param name="action"></param>
        ///// <param name="controller"></param>
        ///// <returns></returns>
        //public static string AbsoluteAction(this UrlHelper url, string scheme, string controllerName, string action, object routeValues)
        //{
        //    Uri requestUrl = url.RequestContext.HttpContext.Request.Url;

        //    string absoluteAction = string.Format("{0}://{1}{2}",
        //                                          requestUrl.Scheme,
        //                                          requestUrl.Authority,
        //                                          url.Action(action, controllerName, routeValues));

        //    return absoluteAction;
        //}

        /// <summary>
        /// Returns an absolute url for an action
        /// </summary>
        /// <param name="url">UrlHelper</param>
        /// <param name="scheme"></param>
        /// <param name="action"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string AbsoluteAction(this UrlHelper url, string scheme, string action, object routeValues)
        {
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;

            string absoluteAction = string.Format("{0}://{1}{2}",
                                                  scheme,
                                                  requestUrl.Authority,
                                                  url.Action(action, routeValues));

            return absoluteAction;
        }
    }
}
