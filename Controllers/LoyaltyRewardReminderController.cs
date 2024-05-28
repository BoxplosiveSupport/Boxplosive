using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Models.LoyaltyRewardReminder;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public partial class LoyaltyRewardReminderController : ControllerBase
    {
        private ILoyaltyPointsProgramRepository _LoyaltyPointsProgramRepository = DataRepositoryFactory.GetInstance().DataRepository<ILoyaltyPointsProgramRepository>();
        private ILoyaltyPointsRewardRepository _LoyaltyPointsRewardRepository = DataRepositoryFactory.GetInstance().DataRepository<ILoyaltyPointsRewardRepository>();
        private ILoyaltyRewardReminderRepository _LoyaltyRewardReminderRepository = DataRepositoryFactory.GetInstance().DataRepository<ILoyaltyRewardReminderRepository>();

        public virtual ActionResult Index(int id)
        {
            IDtoLoyaltyPointsProgram program = _LoyaltyPointsProgramRepository.Get(id);
            if (program == null)
            {
                return View(new RewardReminderOverview());
            }

            return View(new RewardReminderOverview(id, program.Rewards.Where(r => r.Status != LoyaltyPointsRewardStatus.Expired)));
        }

        public virtual ActionResult Edit(int id, LoyaltyRewardReminderType type)
        {
            var reward = _LoyaltyPointsRewardRepository.Get(id);
            var loyaltyRewardReminder = _LoyaltyRewardReminderRepository.GetByRewardId(id).FirstOrDefault(r => r.Type == type);
            RewardReminderDetail model = loyaltyRewardReminder == null ? new RewardReminderDetail(reward, type) : new RewardReminderDetail(loyaltyRewardReminder);

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Edit(RewardReminderDetail model)
        {
            if (model.IsEnabled || model.ReminderId > 0)
            {
                _LoyaltyRewardReminderRepository.InsertOrUpdate(model.ToDto());
            }

            var reward = _LoyaltyPointsRewardRepository.Get(model.LoyaltyPointsRewardId);
            return RedirectToAction(MVC.LoyaltyRewardReminder.ActionNames.Index, new { id = reward.Campaign.LoyaltyPointsProgram.Id });
        }
    }
}