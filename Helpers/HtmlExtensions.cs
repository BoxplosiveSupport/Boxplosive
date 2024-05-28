using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString ActionLinkIcon(this HtmlHelper helper, string linkText, string actionName, object routeValues, object htmlAttributes,
            object iconHtmlAttributes)
        {
            // Generate the URL
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            RouteData routeData = helper.ViewContext.RouteData;
            string url = urlHelper.Action(actionName, routeData.GetRequiredString("controller"), routeValues);

            return _CreateActionLinkIcon(helper, linkText, url, htmlAttributes, iconHtmlAttributes);
        }

        public static MvcHtmlString ActionLinkIcon(this HtmlHelper helper, string linkText, ActionResult result, object htmlAttributes,
            object iconHtmlAttributes)
        {
            // Generate the URL
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            string url = urlHelper.Action(result);

            return _CreateActionLinkIcon(helper, linkText, url, htmlAttributes, iconHtmlAttributes);
        }

        private static MvcHtmlString _CreateActionLinkIcon(HtmlHelper helper, string linkText, string url, object htmlAttributes, object iconHtmlAttributes)
        {
            // Generate the inner span tag
            var spanTag = new TagBuilder("span");
            spanTag.InnerHtml = linkText;
            spanTag.MergeAttributes(new RouteValueDictionary(iconHtmlAttributes));
            var span = spanTag.ToString(TagRenderMode.Normal);

            // Generate the link tag
            var aTag = new TagBuilder("a");
            aTag.InnerHtml = span;
            aTag.Attributes["href"] = url;
            aTag.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            return MvcHtmlString.Create(aTag.ToString());
        }

        public static MvcHtmlString ModalSubmitButton(this HtmlHelper htmlHelper, string id, string buttonText, string name, string value, string formId,
            object routeValues = null, object htmlAttributes = null, bool disabled = false)
        {
            var attributes = new RouteValueDictionary(htmlAttributes);

            var tag = _CreateModalButtonTagBuilder(id, buttonText, disabled);
            tag.InnerHtml = buttonText;
            tag.MergeAttribute("name", name);
            tag.MergeAttribute("value", value);
            tag.MergeAttribute("data-formid", formId);
            tag.MergeAttributes(attributes, true);

            return MvcHtmlString.Create(tag.ToString());
        }

        public static MvcHtmlString ModalButton(this HtmlHelper htmlHelper, string id, string buttonText, string actionName, string controllerName = null,
            string subject = null, string type = null, object routeValues = null, object htmlAttributes = null, bool disabled = false)
        {
            // Generate the URL
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var routeData = htmlHelper.ViewContext.RouteData;
            var url = urlHelper.Action(actionName, controllerName ?? routeData.GetRequiredString("controller"), routeValues);

            var tag = _CreateModalButtonTagBuilder(id, buttonText, disabled);
            tag.MergeAttribute("data-url", url);

            if (htmlAttributes != null)
            {
                var attributes = new RouteValueDictionary(htmlAttributes);
                tag.MergeAttributes(attributes, true);
            }

            if (!string.IsNullOrEmpty(subject))
            {
                tag.MergeAttribute("data-subject", subject);
            }
            if (!string.IsNullOrEmpty(type))
            {
                tag.MergeAttribute("data-type", type);
            }

            return MvcHtmlString.Create(tag.ToString());
        }

        private static TagBuilder _CreateModalButtonTagBuilder(string id, string buttonText, bool disabled)
        {
            var tag = new TagBuilder("button");
            tag.InnerHtml = buttonText;
            tag.MergeAttribute("class", "btn btn-primary button-modal");
            tag.MergeAttribute("data-buttonmodal", "true");
            tag.MergeAttribute("data-toggle", "modal");
            tag.MergeAttribute("data-target", $"#{id}");

            if (disabled)
            {
                tag.MergeAttribute("disabled", "disabled");
            }

            return tag;
        }

        public static MvcHtmlString LabelForRequired<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression,
            object htmlAttributes = null, string labelText = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            if (String.IsNullOrEmpty(labelText))
            {
                labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            }

            if (String.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            var tag = new TagBuilder("label");
            if (htmlAttributes != null)
            {
                tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            }

            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            tag.SetInnerText(HttpUtility.HtmlDecode(labelText));
            if (metadata.IsRequired)
            {
                tag.AddCssClass("required");
            }

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString DisplayForWithError<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression,
            string displayTagName = "label", object htmlAttributes = null, string labelPrefixText = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            var labelText = metadata.SimpleDisplayText;
            if (String.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }
            labelText = String.Format("{0} {1}", labelPrefixText, labelText);

            var tag = new TagBuilder(displayTagName);
            tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));

            var name = ExpressionHelper.GetExpressionText(expression);
            var fullName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            ModelState modelState;
            if (html.ViewData.ModelState.TryGetValue(fullName, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    var firstOrDefault = modelState.Errors.FirstOrDefault();
                    labelText = firstOrDefault != null ? firstOrDefault.ErrorMessage : "Sadly something went wrong";

                    tag.AddCssClass("datapanel-color");
                }
            }

            tag.SetInnerText(labelText);

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        public static IHtmlString TextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression,
            string value, bool useNativeUnobtrusiveAttributes, int rows = 2, int columns = 20, object htmlAttributes = null)
        {
            string name = ExpressionHelper.GetExpressionText((LambdaExpression)expression);
            if (!useNativeUnobtrusiveAttributes)
            {
                return htmlHelper.TextArea(name, value, rows, columns, htmlAttributes);
            }

            ModelMetadata modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            RouteValueDictionary attributes = Mapper.GetUnobtrusiveValidationAttributes(htmlHelper, expression, htmlAttributes, modelMetadata);
            return Mapper.GenerateHtmlWithoutMvcUnobtrusiveAttributes(() => htmlHelper.TextArea(name, value, rows, columns, attributes));
        }

        public static RouteValueDictionary AddRouteValue(this object htmlAttributes, string key, object value, bool condition = true)
        {
            var routeValues = (htmlAttributes as RouteValueDictionary) ?? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if (condition)
                routeValues[key] = value;

            return routeValues;
        }
    }
}