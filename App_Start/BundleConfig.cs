using System.Web.Optimization;

namespace nl.boxplosive.BackOffice.Mvc
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region Scripts

            bundles.Add(new ScriptBundle("~/Script/jQuery")
                .Include("~/Scripts/jquery-{version}.js")
                .Include("~/Scripts/jquery-ui-{version}.js")
                .Include("~/Scripts/boxplosive/boxSpinner.js")
                .Include("~/Scripts/boxplosive/boxAutocomplete.js")
                .Include("~/Scripts/boxplosive/equalHeights.js")
                .Include("~/Scripts/boxplosive/initCharts.js")
                .Include("~/Scripts/boxplosive/custom-jquery-validation.js")
                .Include("~/Scripts/plugins/load-image.all.js")
                .Include("~/Scripts/jQuery.FileUpload/jQuery.iframe-transport.js")
                .Include("~/Scripts/jQuery.FileUpload/jQuery.fileupload.js")
                .Include("~/Scripts/jQuery.FileUpload/jQuery.fileupload-process.js")
                .Include("~/Scripts/jQuery.FileUpload/jQuery.fileupload-image.js")
                .Include("~/Scripts/jQuery.FileUpload/jQuery.fileupload-validate.js")
                .Include("~/Scripts/jquery.validate.js")
                .Include("~/Scripts/additional-methods.js")
                .Include("~/Scripts/mustache.js")
                .Include("~/Scripts/MvcFoolproofJQueryValidation.js")
                .Include("~/Scripts/jquery.timepicker.js")
                .Include("~/Scripts/timepicker.js"));

            bundles.Add(new ScriptBundle("~/Script/Bootstrap")
                .Include("~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/Script/Modernizr")
                .Include("~/Scripts/modernizr-{version}.js"));

            bundles.Add(new ScriptBundle("~/Script/IE8Polyfills")
                .Include("~/Scripts/respond.js")
                .Include("~/Scripts/polyfills/es5-shim.js")
                .Include("~/Scripts/polyfills/es5-sham.js")
                .Include("~/Scripts/polyfills/excanvas.js"));

            bundles.Add(new ScriptBundle("~/Script/Boxplosive")
                .Include("~/Scripts/boxplosive.js")
                .Include("~/Scripts/boxplosive/tab.js")
                .Include("~/Scripts/boxplosive/modal.js")
                .Include("~/Scripts/boxplosive/init.js")
                .Include("~/Scripts/boxplosive/boxplosive-product.js"));

            #endregion Scripts

            #region Styling

            bundles.Add(new StyleBundle("~/Styles/Boxplosive")
                .Include("~/stylesheets/boxplosive.css"));

            #endregion Styling
        }
    }
}
