using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EBill.Models;
using EBill.Repository;

namespace EBill.Controllers
{
    public class EBillController : Controller
    {
        // GET: EBill
        public ActionResult Index()
        {
            Data data = new Data();
            var list = data.GetAllDetails();
            return View(list);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(BillDetail details)
        {
            Data data = new Data();
            data.SaveBillDetails(details);
            ModelState.Clear();
            return View();
        }

        [HttpPost]
        public ActionResult CreateItem(Items item)
        {
            return PartialView("CreateItem", item);
        }

        public ActionResult ViewBill(int id)
        {
            Data data = new Data();
            var details = data.GetDetail(id);
            return View(details);
        }
    }
}