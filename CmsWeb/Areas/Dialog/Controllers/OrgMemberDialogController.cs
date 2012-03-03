using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsData;
using UtilityExtensions;
using CMSPresenter;
using CmsWeb.Models;
using CmsWeb.Models.OrganizationPage;
using CmsData.Codes;

namespace CmsWeb.Areas.Dialog.Controllers
{
    public class OrgMemberDialogController : CmsStaffController
    {
        public ActionResult Index(int id, int pid, string from)
        {
            var m = DbUtil.Db.OrganizationMembers.SingleOrDefault(om => om.OrganizationId == id && om.PeopleId == pid);
            ViewData["from"] = from;
            if (m == null)
                return Content("cannot find membership: id={0} pid={1}".Fmt(id, pid));
            return View(m);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CheckBoxChanged(string id, bool ck)
        {
            var a = id.Split('-');
            var om = DbUtil.Db.OrganizationMembers.SingleOrDefault(m => m.PeopleId == a[2].ToInt() && m.OrganizationId == a[1].ToInt());
            if (om == null)
                return Content("error");
            if (ck)
                om.OrgMemMemTags.Add(new OrgMemMemTag { MemberTagId = a[3].ToInt() });
            else
            {
                var mt = om.OrgMemMemTags.Single(t => t.MemberTagId == a[3].ToInt());
                DbUtil.Db.OrgMemMemTags.DeleteOnSubmit(mt);
            }
            DbUtil.Db.SubmitChanges();
            return Content("ok");
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(int id, int pid)
        {
            ViewData["MemberTypes"] = QueryModel.ConvertToSelect(CodeValueController.MemberTypeCodes(), "Id");
            var om = DbUtil.Db.OrganizationMembers.Single(m => m.PeopleId == pid && m.OrganizationId == id);
            return View(om);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Display(int id, int pid)
        {
            var om = DbUtil.Db.OrganizationMembers.Single(m => m.PeopleId == pid && m.OrganizationId == id);
            return View(om);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(int id, int pid)
        {
            var om = DbUtil.Db.OrganizationMembers.Single(m => m.PeopleId == pid && m.OrganizationId == id);
            try
            {
                UpdateModel(om);
                om.ShirtSize = om.ShirtSize.MaxString(20);
                DbUtil.Db.SubmitChanges();
            }
            catch (Exception)
            {
                ViewData["MemberTypes"] = QueryModel.ConvertToSelect(CodeValueController.MemberTypeCodes(), "Id");
                return View("Edit", om);
            }
            return View("Display", om);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Drop(string id)
        {
            var a = id.Split('-');
            var om = DbUtil.Db.OrganizationMembers.SingleOrDefault(m => m.PeopleId == a[2].ToInt() && m.OrganizationId == a[1].ToInt());
            if (om != null)
            {
                om.Drop(DbUtil.Db, addToHistory:true);
                DbUtil.Db.SubmitChanges();
            }
            return Content("dropped");
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Move(int id, int pid)
        {
            var om = DbUtil.Db.OrganizationMembers.Single(m => m.PeopleId == pid && m.OrganizationId == id);
            ViewData["name"] = om.Person.Name;
            ViewData["oid"] = id;
            ViewData["pid"] = pid;
            if (om.Organization.DivisionId == null)
                return View((IEnumerable<OrgMove>)null);
            var q = from o in DbUtil.Db.Organizations
                    where o.DivOrgs.Any(dd => dd.DivId == o.DivisionId)
                    where o.OrganizationId != id
                    where o.OrganizationStatusId == OrgStatusCode.Active
                    orderby o.OrganizationName
                    select new OrgMove
                    {
                         OrgName = o.OrganizationName,
                         OrgId = o.OrganizationId,
                         id = "m-{0}-{1}-{2}".Fmt(id, pid, o.OrganizationId),
                         Program = o.Division.Program.Name,
                         Division = o.Division.Name,
                         orgSchedule = o.OrgSchedules.First()
                    };
            return View(q.ToList());
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult MoveSelect(string id)
        {
            var a = id.Split('-');
            var om1 = DbUtil.Db.OrganizationMembers.Single(m => m.PeopleId == a[2].ToInt() && m.OrganizationId == a[1].ToInt());
            var om2 = CmsData.OrganizationMember.InsertOrgMembers(DbUtil.Db,
                a[3].ToInt(), om1.PeopleId, om1.MemberTypeId, DateTime.Now, om1.InactiveDate, om1.Pending ?? false);
			DbUtil.Db.UpdateMainFellowship(om2.OrganizationId);
            om2.Request = om1.Request;
            om2.Amount = om1.Amount;
            om2.UserData = om1.UserData;
            om1.Drop(DbUtil.Db, addToHistory:true);
            DbUtil.Db.SubmitChanges();
            return Content("moved");
        }
        public class OrgMove
        {
            public string OrgName { get; set; }
            public string id { get; set; }
            public int OrgId { get; set; }
            public string Program { get; set; }
            public string Division { get; set; }
            public OrgSchedule orgSchedule { get; set; }
            public string Tip
            {
                get
                {
                    var si = new ScheduleInfo(orgSchedule);
                    return "{0} ({1})|Program:{2}|Division: {3}|Schedule: {4}".Fmt(OrgName, OrgId, Program, Division, si.Display);
                }
            }
        }
        public string HelpLink()
        {
            return "";
        }

    }
}
