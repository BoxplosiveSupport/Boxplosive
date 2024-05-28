using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.Home;
using System.Collections.Generic;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class HomeFactory
    {
        public static CreateModule CreateCreateModule(UrlHelper url)
        {
            var model = new CreateModule();

            // Add the defaults
            var createPromotion = new ProcessModule
            {
                Title = "Create new promotion",
                ActionUrl = url.Action(MVC.CreatePromotion.ActionNames.Index, MVC.CreatePromotion.Name),
                ActionMetadata = new ActionMetadata()
                {
                    MethodName = MVC.CreatePromotion.ActionNames.Index,
                    Namespace = MVC.CreatePromotion.GetType().Namespace,
                    ControllerName = MVC.CreatePromotion.Name,
                    IsClass = true,
                }
            };
            model.CreateActions.Add(createPromotion);
            var createLoyalty = new ProcessModule
            {
                Title = "Create new loyalty",
                ActionUrl = url.Action(MVC.CreateLoyalty.ActionNames.Index, MVC.CreateLoyalty.Name),
                ActionMetadata = new ActionMetadata()
                {
                    MethodName = MVC.CreateLoyalty.ActionNames.Index,
                    Namespace = MVC.CreateLoyalty.GetType().Namespace,
                    ControllerName = MVC.CreateLoyalty.Name,
                    IsClass = true,
                }
            };
            model.CreateActions.Add(createLoyalty);

            return model;
        }

        public static List<ProcessModule> CreateHomePageModules(UrlHelper url)
        {
            List<ProcessModule> homePageModules = new List<ProcessModule>();

            homePageModules.CheckFeatureAndAdd(new ProcessModule
            {
                Title = "News articles",
                CounterDescription = "Manage existing and add new news articles",
                ActionUrl = url.Action(MVC.ManageNewsArticle.Index()),
                Roles = MVC.ManageNewsArticle.AuthorizedRoles
            });

            homePageModules.CheckFeatureAndAdd(new ProcessModule
            {
                Title = "Retailer stores",
                CounterDescription = "Manage existing and add new retailer stores",
                ActionUrl = url.Action(MVC.ManageRetailerStores.Index()),
                Roles = MVC.ManageRetailerStores.AuthorizedRoles
            });

            homePageModules.CheckFeatureAndAdd(new ProcessModule
            {
                Title = "Products",
                CounterDescription = "Manage product item overrides",
                ActionUrl = url.Action(MVC.ManageProducts.Index()),
                Roles = MVC.ManageProducts.AuthorizedRoles
            });

            homePageModules.CheckFeatureAndAdd(new ProcessModule
            {
                FeatureName = "StartupMessage",
                Title = "Startup Messages",
                CounterDescription = "Manage existing and add new Startup Messages",
                ActionUrl = url.Action(MVC.ManageVersionMessage.Index()),
                Roles = MVC.ManageVersionMessage.AuthorizedRoles
            });

            homePageModules.CheckFeatureAndAdd(new ProcessModule
            {
                FeatureName = "LoyaltyEvent",
                Title = "Loyalty events",
                CounterDescription = "Manage loyalty events",
                ActionUrl = url.Action(MVC.LoyaltyEventDefinition.Index()),
                Roles = MVC.LoyaltyEventDefinition.AuthorizedRoles
            });

            homePageModules.CheckFeatureAndAdd(new ProcessModule
            {
                FeatureName = NotificationDefinitionFactory.FeatureName,
                Title = NotificationDefinitionFactory.ListPageTitle,
                CounterDescription = NotificationDefinitionFactory.ManageText,
                ActionUrl = url.Action(MVC.NotificationDefinition.Index()),
                Roles = MVC.NotificationDefinition.AuthorizedRoles
            });

            homePageModules.CheckFeatureAndAdd(new ProcessModule
            {
                FeatureName = "ManageApplicationSettings",
                Title = "Settings",
                CounterDescription = "Manage application settings",
                ActionUrl = url.Action(MVC.AppConfigItemGroup.ActionNames.Index, MVC.AppConfigItemGroup.Name),
                Roles = MVC.AppConfigItemGroup.AuthorizedRoles
            });

            homePageModules.CheckFeatureAndAdd(new ProcessModule
            {
                Title = AppCardStackDefinitionFactory.ListPageTitle,
                CounterDescription = AppCardStackDefinitionFactory.ManageText,
                ActionUrl = url.Action(MVC.AppCardStackDefinition.Index()),
                Roles = MVC.AppCardStackDefinition.AuthorizedRoles
            });

            return homePageModules;
        }
    }
}