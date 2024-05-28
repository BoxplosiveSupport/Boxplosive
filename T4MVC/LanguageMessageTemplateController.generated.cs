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
    public partial class LanguageMessageTemplateController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public LanguageMessageTemplateController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected LanguageMessageTemplateController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult Index()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Edit()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Edit);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Delete()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Delete);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetMessageGroupPartialView()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetMessageGroupPartialView);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public LanguageMessageTemplateController Actions { get { return MVC.LanguageMessageTemplate; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "LanguageMessageTemplate";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "LanguageMessageTemplate";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string Create = "Create";
            public readonly string Edit = "Edit";
            public readonly string Delete = "Delete";
            public readonly string GetMessageGroupPartialView = "GetMessageGroupPartialView";
            public readonly string SignOutUser = "SignOutUser";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string Create = "Create";
            public const string Edit = "Edit";
            public const string Delete = "Delete";
            public const string GetMessageGroupPartialView = "GetMessageGroupPartialView";
            public const string SignOutUser = "SignOutUser";
        }


        static readonly ActionParamsClass_Index s_params_Index = new ActionParamsClass_Index();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Index IndexParams { get { return s_params_Index; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Index
        {
            public readonly string sortColumn = "sortColumn";
            public readonly string sortOrder = "sortOrder";
        }
        static readonly ActionParamsClass_Create s_params_Create = new ActionParamsClass_Create();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Create CreateParams { get { return s_params_Create; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Create
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_Edit s_params_Edit = new ActionParamsClass_Edit();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Edit EditParams { get { return s_params_Edit; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Edit
        {
            public readonly string id = "id";
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_Delete s_params_Delete = new ActionParamsClass_Delete();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Delete DeleteParams { get { return s_params_Delete; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Delete
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_GetMessageGroupPartialView s_params_GetMessageGroupPartialView = new ActionParamsClass_GetMessageGroupPartialView();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetMessageGroupPartialView GetMessageGroupPartialViewParams { get { return s_params_GetMessageGroupPartialView; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetMessageGroupPartialView
        {
            public readonly string selected = "selected";
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
                public readonly string Create = "Create";
                public readonly string Edit = "Edit";
                public readonly string Empty = "Empty";
                public readonly string Index = "Index";
                public readonly string MessageGroupNameEmail = "MessageGroupNameEmail";
            }
            public readonly string Create = "~/Views/LanguageMessageTemplate/Create.cshtml";
            public readonly string Edit = "~/Views/LanguageMessageTemplate/Edit.cshtml";
            public readonly string Empty = "~/Views/LanguageMessageTemplate/Empty.cshtml";
            public readonly string Index = "~/Views/LanguageMessageTemplate/Index.cshtml";
            public readonly string MessageGroupNameEmail = "~/Views/LanguageMessageTemplate/MessageGroupNameEmail.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_LanguageMessageTemplateController : nl.boxplosive.BackOffice.Mvc.Controllers.LanguageMessageTemplateController
    {
        public T4MVC_LanguageMessageTemplateController() : base(Dummy.Instance) { }

        [NonAction]
        partial void IndexOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string sortColumn, string sortOrder);

        [NonAction]
        public override System.Web.Mvc.ActionResult Index(string sortColumn, string sortOrder)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "sortColumn", sortColumn);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "sortOrder", sortOrder);
            IndexOverride(callInfo, sortColumn, sortOrder);
            return callInfo;
        }

        [NonAction]
        partial void CreateOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult Create()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Create);
            CreateOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void CreateOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate.LanguageMessageTemplateCreateModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Create(nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate.LanguageMessageTemplateCreateModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Create);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            CreateOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void EditOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id);

        [NonAction]
        public override System.Web.Mvc.ActionResult Edit(int id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Edit);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            EditOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void EditOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate.LanguageMessageTemplateEditModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Edit(nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate.LanguageMessageTemplateEditModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Edit);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            EditOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void DeleteOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate.LanguageMessageTemplateModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Delete(nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate.LanguageMessageTemplateModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Delete);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            DeleteOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void GetMessageGroupPartialViewOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string selected);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetMessageGroupPartialView(string selected)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetMessageGroupPartialView);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "selected", selected);
            GetMessageGroupPartialViewOverride(callInfo, selected);
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
