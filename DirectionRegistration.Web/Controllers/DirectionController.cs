using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;

namespace DirectionRegistration.Web.Controllers
{
    [SuperCheck]
    public class DirectionController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();
        //
        // GET: /Direction/

        public ActionResult Index()
        {
            ViewBag.Teachers = getTeacherListItems();

            DirectionViewModel model = new DirectionViewModel();
            return View(model);
        }

        public PartialViewResult GetDirections()
        {
            return PartialView("PartialDirectionList", bindDirectionViewModel());
        }

        [HttpPost]
        public ActionResult Add(DirectionViewModel model)
        {
            if (!isExistedDirection(model.Title))
            {
                Teacher teacher = db.Teachers.SingleOrDefault(t => t.Id == model.TeacherId);
                if (teacher == null)
                {
                    return Json(new { code = 1, data = "未指定方向负责人" });
                }
                if (teacher.DirectionId != null)
                {
                    return Json(new { code = 1, data = "该负责人已指定负责方向" });
                }

                Direction d = new Direction
                {
                    Title = model.Title,
                    Max = 35,
                    TeacherId = teacher.Id
                };
                this.db.Directions.Add(d);
                int i = this.db.SaveChanges();

                if (i > 0)
                {
                    teacher.DirectionId = db.Directions.Single(dd => dd.Title == d.Title).Id;
                    db.SaveChanges();
                    return PartialView("PartialDirectionList", bindDirectionViewModel());
                }
            }
            return Json(new { code = 1, data = "添加失败" });
        }

        [HttpPost]
        public ActionResult Delete(int Id)
        {
            var d = this.db.Directions.SingleOrDefault(dd => dd.Id == Id);
            if (d != null)
            {
                //删除学生填选记录
                var dirs = db.DirectionStudents.Where(ds => ds.Direction.Id == Id).ToList();
                foreach(var dir in dirs)
                {
                    db.DirectionStudents.Remove(dir);
                }
                this.db.Directions.Remove(d);
                //删除教师的方向负责人关系
                var teacher = db.Teachers.SingleOrDefault(t => t.DirectionId == Id);
                if (teacher != null)
                {
                    teacher.DirectionId = null;
                }

                int i = this.db.SaveChanges();
                if (i == 0)
                {
                    return Json(new { code = 1, data = "删除失败" });
                }
            }
            return PartialView("PartialDirectionList", bindDirectionViewModel());
        }

        [HttpGet]
        public ActionResult Modify(int Id)
        {
            Direction _direction = db.Directions.SingleOrDefault(d => d.Id == Id);
            DirectionViewModel model = new DirectionViewModel();
            if (_direction != null)
            {
                var teacher = db.Teachers.SingleOrDefault(t => t.Id == _direction.TeacherId);
                model.Id = _direction.Id;
                model.Title = _direction.Title;
                model.Max = _direction.Max;
                if (teacher != null)
                {
                    model.TeacherId = _direction.TeacherId.Value;
                    model.TeacherName = teacher.Name;
                }
                else
                {
                    model.TeacherId = 0;
                }
            }

            ViewBag.Teachers = getTeacherListItems(model.TeacherId);

            return PartialView("PartialModifyDirection", model);
        }

        [HttpPost]
        public ActionResult Modify(DirectionViewModel direction)
        {
            Direction _direction = db.Directions.SingleOrDefault(d => d.Id == direction.Id);
            if (_direction != null)
            {
                Teacher newTeacher = db.Teachers.SingleOrDefault(t => t.Id == direction.TeacherId);
                if (newTeacher == null)
                {
                    return Json(new { code = 1, data = "未指定方向负责人" });
                }
                if (newTeacher.DirectionId != null && newTeacher.DirectionId != _direction.Id)
                {
                    return Json(new { code = 1, data = "该负责人已指定负责方向" });
                }

                var oldTeacher = db.Teachers.SingleOrDefault(t => t.DirectionId == _direction.Id);
                if (oldTeacher != null) oldTeacher.DirectionId = null;
                newTeacher.DirectionId = _direction.Id;

                _direction.Title = direction.Title;
                _direction.TeacherId = newTeacher.Id;
                _direction.Max = direction.Max;
                
                int i = db.SaveChanges();
                return PartialView("PartialDirectionList", bindDirectionViewModel());
            }
            return Json(new { code = 1, data = "修改失败" });
        }

        private List<DirectionViewModel> bindDirectionViewModel()
        {
            List<Direction> ds = this.db.Directions.ToList();
            List<DirectionViewModel> m = new List<DirectionViewModel>();
            ds.ForEach(dd =>
            {
                var teacher = db.Teachers.SingleOrDefault(t => t.Id == dd.TeacherId);
                var item = new DirectionViewModel
                {
                    Id = dd.Id,
                    Title = dd.Title,
                    TeacherId = dd.TeacherId ?? 0,
                    Max = dd.Max
                };
                if (teacher != null)
                {
                    item.TeacherName = teacher.Name;
                }
                else
                {
                    item.TeacherName = "未指定";
                }
                m.Add(item);
            });
            return m;
        }
        
        private bool isExistedDirection(string directionName)
        {
            bool b = false;
            int count = db.Directions.Count(d => d.Title == directionName);
            if (count != 0)
            {
                b = true;
            }
            return b;
        }

        private List<SelectListItem> getTeacherListItems(int? tid = null)
        {
            var teacherListItems = new List<SelectListItem>();
            if (tid == null)
            {
                teacherListItems.Add(
                    new SelectListItem()
                    {
                        Selected = true,
                        Text = "方向负责人",
                        Value = "0"
                    }
                );
            }

            var tlist = db.Teachers.ToList();
            foreach (var d in tlist)
            {
                var item = new SelectListItem()
                {
                    Selected = false,
                    Text = d.Name,
                    Value = d.Id.ToString()
                };
                if (d.Id == tid)
                {
                    item.Selected = true;
                }
                teacherListItems.Add(item);
            }
            return teacherListItems;
        }
    }
}
