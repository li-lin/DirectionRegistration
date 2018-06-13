using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirectionRegistration.Repository.Entities;

namespace DirectionRegistration.Web.Helper
{
    public static class CustomHelper
    {
        //暂时不用
        public static MvcHtmlString DirectionsDropdownListHelper(this HtmlHelper html,string attrName,List<Direction> directions)
        {
            TagBuilder tag = new TagBuilder("select");
            tag.Attributes.Add("id",attrName);
            tag.Attributes.Add("name",attrName);
            if (directions == null || directions.Count == 0)
            {
                throw new Exception("系统错误:数据加载时出错，请稍后重试。");
            }
            foreach (var d in directions)
            {
                TagBuilder opTag = new TagBuilder("option");

            }
            return new MvcHtmlString(tag.ToString());
        }
    }
}