using System.Web;
using System.Web.Mvc;
using DirectionRegistration.Web.Filters;

namespace DirectionRegistration
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustHandleErrorAttribute());
            //filters.Add(new LoginCheckAttribute());
            //filters.Add(new SuperCheckAttribute());
        }
    }
}