using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Models.Home
{
    /// <summary>
    /// ActionMetadata contains properties to allow to get method type or class type using reflection
    /// </summary>
    public class ActionMetadata
    {
        public string ControllerName { get; set; }
        public string MethodName { get; set; }
        public string Namespace { get; set; }

        public Type[] TypeArgs { get; set; }
        public bool IsClass { get; set; }
    }
}