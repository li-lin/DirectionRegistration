using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;
using System.Web.Script.Serialization;

namespace DirectionRegistration.Web.Controllers
{
    [SuperCheck]
    public class DirectionController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();
        //
        // GET: /Direction/
        // 暂时取消方向负责人信息

        public ActionResult Index()
        {
            DirectionViewModel model = new DirectionViewModel();
            return View(model);
        }

        public PartialViewResult GetDirections()
        {
            return PartialView("PartialDirectionList", BindDirectionsViewModel());
        }

        [HttpPost]
        public ActionResult Add(DirectionViewModel model)
        {
            if (!isExistedDirection(model.Title))
            {
                Direction d = new Direction
                {
                    Title = model.Title,
                    Max = model.Max
                };
                db.Directions.Add(d);
                int i = db.SaveChanges();

                if (i > 0)
                {
                    return PartialView("PartialDirectionList", BindDirectionsViewModel());
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
                db.DirectionStudents.RemoveRange(dirs);

                //删除课程与方向的对应关系
                var dcs = db.DirectionCourses.Where(dc => dc.Direction.Id == Id).ToList();
                db.DirectionCourses.RemoveRange(dcs);

                db.Directions.Remove(d);

                int i = db.SaveChanges();
                if (i == 0)
                {
                    return Json(new { code = 1, data = "删除失败" });
                }
            }
            return PartialView("PartialDirectionList", BindDirectionsViewModel());
        }

        [HttpGet]
        public ActionResult Modify(int Id)
        {
            Direction _direction = db.Directions.SingleOrDefault(d => d.Id == Id);
            DirectionViewModel model = BindDirectionViewModel(_direction);

            ViewBag.Courses = GetCourseSelectListItems();

            return PartialView("PartialModifyDirection", model);
        }

        [HttpPost]
        public ActionResult Modify(DirectionViewModel direction)
        {
            Direction _direction = db.Directions.SingleOrDefault(d => d.Id == direction.Id);
            if (_direction != null)
            {
                _direction.Title = direction.Title;
                _direction.Max = direction.Max;
                
                int i = db.SaveChanges();
                return PartialView("PartialDirectionList", BindDirectionsViewModel());
            }
            return Json(new { code = 1, data = "修改失败" });
        }

        /// <summary>
        /// 删除指定方向的指定考核课程
        /// </summary>
        [HttpPost]
        public ActionResult DeleteCourse(int Cid, int Did)
        {
            var dc = db.DirectionCourses.SingleOrDefault(d => d.Direction.Id == Did && d.Course.Id == Cid);
            if (dc != null)
            {
                db.DirectionCourses.Remove(dc);
                db.SaveChanges();
                var direction = db.Directions.SingleOrDefault(d => d.Id == Did);
                if (direction != null)
                {
                    return PartialView("PartialDirectionCourseList", BindDirectionViewModel(direction));
                }
            }
            return Json(new { code = 1, data = "删除失败" });
        }

        [HttpPost]
        public ActionResult AddCourseToDirection(string dcData)
        {
            var serializer = new JavaScriptSerializer();
            var queryData = serializer.Deserialize<DirectionCourseAdderModel>(dcData);
            var direction = db.Directions.SingleOrDefault(d => d.Id == queryData.Did);
            var course = db.Courses.SingleOrDefault(c => c.Id == queryData.Cid);
            if (direction != null && course != null)
            {
                if (direction.DirectionCourses.Exists(dc => dc.Course.Id == course.Id)==false)
                {
                    db.DirectionCourses.Add(new DirectionCourse
                    {
                        Direction = direction,
                        Course = course,
                        Proportion = queryData.Proportion
                    });
                    db.SaveChanges();
                    return PartialView("PartialDirectionCourseList", BindDirectionViewModel(direction));
                }
                else
                {
                    return Json(new { code = 1, data = "该课程已存在" });
                }

            }
            return Json(new { code = 1, data = "添加失败" });
        }

        public PartialViewResult GetDirectionCourses(int did)
        {
            Direction direction = db.Directions.SingleOrDefault(d => d.Id == did);
            return PartialView("PartialDirectionCourseList", BindDirectionViewModel(direction));
        }
        /// <summary>
        /// 装载方向列表信息到视图模型列表
        /// </summary>
        private List<DirectionViewModel> BindDirectionsViewModel()
        {
            List<Direction> ds = db.Directions.ToList();
            List<DirectionViewModel> m = new List<DirectionViewModel>();           

            ds.ForEach(dd =>
            {
                var item = BindDirectionViewModel(dd);
                m.Add(item);
            });
            return m;
        }

        /// <summary>
        /// 装载方向信息到视图模型
        /// </summary>
        private DirectionViewModel BindDirectionViewModel(Direction dd)
        {
            var item = new DirectionViewModel
            {
                Id = dd.Id,
                Title = dd.Title,
                Max = dd.Max
            };
            //装载该方向对应的考核课程信息
            item.Courses = new List<DirectionCoursesViewModel>();
            foreach (var dc in dd.DirectionCourses)
            {
                item.Courses.Add(new DirectionCoursesViewModel()
                {
                    CourseId = dc.Course.Id,
                    CourseName = dc.Course.CourseName,
                    Proportion = dc.Proportion
                });
            }
            return item;
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

        /// <summary>
        /// 获取所有课程信息，用以填充方向考核课程选择下拉列表。
        /// </summary>
        private List<SelectListItem> GetCourseSelectListItems()
        {
            var courses = db.Courses;
            List<SelectListItem> coursesListItems = new List<SelectListItem>();

            foreach (var course in courses)
            {
                coursesListItems.Add(new SelectListItem()
                {
                    Value = course.Id.ToString(),
                    Text = course.CourseName
                });
            }
            if (courses.Count() == 0)
            {
                coursesListItems.Add(new SelectListItem()
                {
                    Value ="暂无课程",
                    Text = "暂无课程"
                });
            }
            return coursesListItems;
        }
        //private List<SelectListItem> getTeacherListItems(int? tid = null)
        //{
        //    var teacherListItems = new List<SelectListItem>();
        //    if (tid == null)
        //    {
        //        teacherListItems.Add(
        //            new SelectListItem()
        //            {
        //                Selected = true,
        //                Text = "方向负责人",
        //                Value = "0"
        //            }
        //        );
        //    }

        //    var tlist = db.Teachers.ToList();
        //    foreach (var d in tlist)
        //    {
        //        var item = new SelectListItem()
        //        {
        //            Selected = false,
        //            Text = d.Name,
        //            Value = d.Id.ToString()
        //        };
        //        if (d.Id == tid)
        //        {
        //            item.Selected = true;
        //        }
        //        teacherListItems.Add(item);
        //    }
        //    return teacherListItems;
        //}
    }
}

class DirectionCourseAdderModel
{
    public int Cid { get; set; }
    public int Did { get; set; }
    public double Proportion { get; set; }
}
