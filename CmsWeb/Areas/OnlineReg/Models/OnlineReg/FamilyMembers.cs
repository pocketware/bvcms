using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsData;

namespace CmsWeb.Areas.OnlineReg.Models
{
    public partial class OnlineRegModel
    {
        public class FamilyMember
        {
            public int PeopleId { get; set; }
            public string Name { get; set; }
            public int? Age { get; set; }
        }
        public IEnumerable<FamilyMember> FamilyMembers()
        {
            var family = from p in user.Family.People
                         where p.DeceasedDate == null
                         select new { p.PeopleId, p.Name2, p.Age, p.Name };
            var q = from m in family
                    join r in _list on m.PeopleId equals r.PeopleId into j
                    from r in j.DefaultIfEmpty()
                    where r == null || r.IsValidForContinue == false
                    orderby m.PeopleId == user.Family.HeadOfHouseholdId ? 1 :
                            m.PeopleId == user.Family.HeadOfHouseholdSpouseId ? 2 :
                            3, m.Age descending, m.Name2
                    select new FamilyMember
                    {
                        PeopleId = m.PeopleId,
                        Name = m.Name,
                        Age = m.Age
                    };
            return q;
        }

        public void StartRegistrationForFamilyMember(int id, ModelStateDictionary modelState)
        {
            modelState.Clear(); // ensure we pull form fields from our model, not MVC's
            HistoryAdd("Register");
            int index = List.Count - 1;
            if (List[index].classid.HasValue)
                classid = List[index].classid;
            var p = LoadExistingPerson(id, index);
            p.ValidateModelForFind(modelState, id, selectFromFamily: true);
            if (!modelState.IsValid)
                return;
            List[index] = p;
            if (p.ManageSubscriptions() && p.Found == true)
                return;

            if (p.org != null && p.Found == true)
            {
                if (!SupportMissionTrip)
                    p.IsFilled = p.org.RegLimitCount(DbUtil.Db) >= p.org.Limit;
                if (p.IsFilled)
                    modelState.AddModelError(this.GetNameFor(mm => mm.List[List.IndexOf(p)].Found), 
                        "Sorry, but registration is filled.");
                if (p.Found == true)
                    p.FillPriorInfo();
                return;
            }
            if (p.org == null && p.ComputesOrganizationByAge())
                modelState.AddModelError(this.GetNameFor(mm => mm.List[id].Found), p.NoAppropriateOrgError);
            if (p.FinishedFindingOrAddingRegistrant && p.org != null && p.ComputesOrganizationByAge())
                p.classid = p.org.OrganizationId;
        }
    }
}