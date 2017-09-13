using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MarkDbModel;

namespace MarkWeb.Controllers
{
    public class DbSession
    {
        public static ApplicationDbContext GetDbContext()
        {
            ApplicationDbContext DbContext = HttpContext.Current.Items["DbContext"] as ApplicationDbContext;
            if(DbContext==null)
            {
                DbContext = new ApplicationDbContext();
                HttpContext.Current.Items["DbContext"] = DbContext;
            }
            return DbContext;
        }
    }
}