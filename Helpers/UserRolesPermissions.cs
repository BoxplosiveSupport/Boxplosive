using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Models.Home;
using nl.boxplosive.Business.Sdk;
using System;
using System.Reflection;
using System.Security.Principal;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
    public static class UserRolesPermissions
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Determine if the user has the role to use a method or class.
        /// </summary>
        /// <param name="actionMetadata">Metadata containing the relevant properties of the method or class</param>
        /// <param name="user">The user we need to validate</param>
        /// <remarks>
        /// This is used for disabling buttons and other functionalities in views.
        /// </remarks>
        /// <returns>True if the user has the right role(s) for this method or class or metadata is empty, else false</returns>
        public static bool EnableDisable(ActionMetadata actionMetadata, IPrincipal user)
        {
            if (actionMetadata == null)
                return true;

            return EnableDisable(actionMetadata.MethodName, actionMetadata.Namespace, actionMetadata.ControllerName, user, actionMetadata.IsClass, actionMetadata.TypeArgs);
        }

        /// <summary>
        /// Determine if the user has the role to use a method or class.
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <param name="Namespace">The namespace of the controller</param>
        /// <param name="controllerName">The controller name</param>
        /// <param name="user">The user we need to validate</param>
        /// <param name="isClass">If true, then get the Authorize attribute from the class</param>
        /// <param name="typeArgs">The parameter type to distinct methods with the same name</param>
        /// <remarks>
        /// This is used for disabling buttons and other functionalities in views.
        /// </remarks>
        /// <returns>True if the user has the right role(s) for this method or class, else false</returns>
        public static bool EnableDisable(string methodName, string Namespace, string controllerName, IPrincipal user, bool isClass = false, Type[] typeArgs = null)
        {
            // Get MethodInfo.
            Type type = Type.GetType(Namespace + "." + controllerName + "Controller");
            AuthorizeAttribute attribute;

            // Get the authorize attribute from the method or the class.
            // prevent ambiguous exception for e.g. index() and index(Model model)
            if (isClass)
            {
                attribute = Attribute.GetCustomAttribute(type, typeof(AuthorizeAttribute)) as AuthorizeAttribute;
            }
            else if (typeArgs != null)
            {
                MethodInfo newType = type.GetMethod(methodName, typeArgs);
                attribute = newType.GetCustomAttribute(typeof(AuthorizeAttribute)) as AuthorizeAttribute;
            }
            else
            {
                var methodInfo = type.GetMethod(methodName);
                attribute = methodInfo.GetCustomAttribute(typeof(AuthorizeAttribute)) as AuthorizeAttribute;
            }

            // if there is no authorize attribute, then the user should have access
            if (attribute == null)
            {
                return true;
            }

            string[] roles = attribute.Roles.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var role in roles)
            {
                if (user.IsInRole(role))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Return true iff the user is allowed access to the given member,
        /// or its declaring class if the member does not declare access.
        ///
        /// Access is granted through the AuthorizeAttribute on the member
        /// or its declaring class.
        ///
        /// If no attribute can be found, log a warning and return true.
        /// </summary>
        public static bool IsAllowed(this IPrincipal user, MemberInfo member)
        {
            var attribute = member.GetCustomAttribute<AuthorizeAttribute>();

            if (attribute == null)
            {
                var type = member.DeclaringType;

                attribute = type?.GetCustomAttribute<AuthorizeAttribute>();

                if (attribute == null)
                {
                    if (_Logger.IsWarnEnabled)
                    {
                        _Logger.Warn($"No authorization roles or attribute on member={member} of type={type}");
                    }

                    return true;
                }
            }

            return user.IsInOneRole(attribute.AuthorizedRoles);
        }
    }
}
