// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments and CLS compliance
// 0108: suppress "Foo hides inherited member Foo. Use the new keyword if hiding was intended." when a controller and its abstract parent are both processed
// 0114: suppress "Foo.BarController.Baz()' hides inherited member 'Qux.BarController.Baz()'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword." when an action (with an argument) overrides an action in a parent controller
#pragma warning disable 1591, 3008, 3009, 0108, 0114
#region T4MVC

using System;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using T4MVC;
namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public partial class HomeController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HomeController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected HomeController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(Task<ActionResult> taskResult)
        {
            return RedirectToAction(taskResult.Result);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(Task<ActionResult> taskResult)
        {
            return RedirectToActionPermanent(taskResult.Result);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ShowAllLiveCampaigns()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ShowAllLiveCampaigns);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ShowAllFinishedCampaigns()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ShowAllFinishedCampaigns);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetCampaignDrafts()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetCampaignDrafts);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HomeController Actions { get { return MVC.Home; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Home";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Home";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string ShowAllLiveCampaigns = "ShowAllLiveCampaigns";
            public readonly string ShowAllFinishedCampaigns = "ShowAllFinishedCampaigns";
            public readonly string GetCampaignDrafts = "GetCampaignDrafts";
            public readonly string SignOutUser = "SignOutUser";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string ShowAllLiveCampaigns = "ShowAllLiveCampaigns";
            public const string ShowAllFinishedCampaigns = "ShowAllFinishedCampaigns";
            public const string GetCampaignDrafts = "GetCampaignDrafts";
            public const string SignOutUser = "SignOutUser";
        }


        static readonly ActionParamsClass_ShowAllLiveCampaigns s_params_ShowAllLiveCampaigns = new ActionParamsClass_ShowAllLiveCampaigns();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ShowAllLiveCampaigns ShowAllLiveCampaignsParams { get { return s_params_ShowAllLiveCampaigns; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ShowAllLiveCampaigns
        {
            public readonly string page = "page";
        }
        static readonly ActionParamsClass_ShowAllFinishedCampaigns s_params_ShowAllFinishedCampaigns = new ActionParamsClass_ShowAllFinishedCampaigns();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ShowAllFinishedCampaigns ShowAllFinishedCampaignsParams { get { return s_params_ShowAllFinishedCampaigns; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ShowAllFinishedCampaigns
        {
            public readonly string page = "page";
        }
        static readonly ActionParamsClass_GetCampaignDrafts s_params_GetCampaignDrafts = new ActionParamsClass_GetCampaignDrafts();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetCampaignDrafts GetCampaignDraftsParams { get { return s_params_GetCampaignDrafts; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetCampaignDrafts
        {
            public readonly string campaignType = "campaignType";
        }
        static readonly ActionParamsClass_SignOutUser s_params_SignOutUser = new ActionParamsClass_SignOutUser();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_SignOutUser SignOutUserParams { get { return s_params_SignOutUser; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_SignOutUser
        {
            public readonly string actionResultOverride = "actionResultOverride";
        }
        static readonly ViewsClass s_views = new ViewsClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewsClass Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewsClass
        {
            static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
            public _ViewNamesClass ViewNames { get { return s_ViewNames; } }
            public class _ViewNamesClass
            {
                public readonly string AnalyseModel = "AnalyseModel";
                public readonly string Index = "Index";
                public readonly string PublishModel = "PublishModel";
                public readonly string ShowAllCampaigns = "ShowAllCampaigns";
                public readonly string ShowAllFinishedPublications = "ShowAllFinishedPublications";
                public readonly string ShowAllLivePublications = "ShowAllLivePublications";
            }
            public readonly string AnalyseModel = "~/Views/Home/AnalyseModel.cshtml";
            public readonly string Index = "~/Views/Home/Index.cshtml";
            public readonly string PublishModel = "~/Views/Home/PublishModel.cshtml";
            public readonly string ShowAllCampaigns = "~/Views/Home/ShowAllCampaigns.cshtml";
            public readonly string ShowAllFinishedPublications = "~/Views/Home/ShowAllFinishedPublications.cshtml";
            public readonly string ShowAllLivePublications = "~/Views/Home/ShowAllLivePublications.cshtml";
            static readonly _DisplayTemplatesClass s_DisplayTemplates = new _DisplayTemplatesClass();
            public _DisplayTemplatesClass DisplayTemplates { get { return s_DisplayTemplates; } }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public partial class _DisplayTemplatesClass
            {
                static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
                public _ViewNamesClass ViewNames { get { return s_ViewNames; } }
                public class _ViewNamesClass
                {
                    public readonly string CampaignModel = "CampaignModel";
                    public readonly string ClientModule = "ClientModule";
                    public readonly string ProcessModule = "ProcessModule";
                }
                public readonly string CampaignModel = "~/Views/Home/DisplayTemplates/CampaignModel.cshtml";
                public readonly string ClientModule = "~/Views/Home/DisplayTemplates/ClientModule.cshtml";
                public readonly string ProcessModule = "~/Views/Home/DisplayTemplates/ProcessModule.cshtml";
            }
            static readonly _EditorTemplatesClass s_EditorTemplates = new _EditorTemplatesClass();
            public _EditorTemplatesClass EditorTemplates { get { return s_EditorTemplates; } }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public partial class _EditorTemplatesClass
            {
                static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
                public _ViewNamesClass ViewNames { get { return s_ViewNames; } }
                public class _ViewNamesClass
                {
                    public readonly string CampaignType = "CampaignType";
                }
                public readonly string CampaignType = "~/Views/Home/EditorTemplates/CampaignType.cshtml";
            }
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_HomeController : nl.boxplosive.BackOffice.Mvc.Controllers.HomeController
    {
        public T4MVC_HomeController() : base(Dummy.Instance) { }

        [NonAction]
        partial void IndexOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult Index()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
            IndexOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void ShowAllLiveCampaignsOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int? page);

        [NonAction]
        public override System.Web.Mvc.ActionResult ShowAllLiveCampaigns(int? page)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ShowAllLiveCampaigns);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "page", page);
            ShowAllLiveCampaignsOverride(callInfo, page);
            return callInfo;
        }

        [NonAction]
        partial void ShowAllFinishedCampaignsOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int? page);

        [NonAction]
        public override System.Web.Mvc.ActionResult ShowAllFinishedCampaigns(int? page)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ShowAllFinishedCampaigns);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "page", page);
            ShowAllFinishedCampaignsOverride(callInfo, page);
            return callInfo;
        }

        [NonAction]
        partial void GetCampaignDraftsOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int campaignType);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetCampaignDrafts(int campaignType)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetCampaignDrafts);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "campaignType", campaignType);
            GetCampaignDraftsOverride(callInfo, campaignType);
            return callInfo;
        }

        [NonAction]
        partial void SignOutUserOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, System.Web.Mvc.ActionResult actionResultOverride);

        [NonAction]
        public override System.Web.Mvc.ActionResult SignOutUser(System.Web.Mvc.ActionResult actionResultOverride)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.SignOutUser);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "actionResultOverride", actionResultOverride);
            SignOutUserOverride(callInfo, actionResultOverride);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
