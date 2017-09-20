using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using XField.Models;

namespace XField
{
    class Program
    {
        //EF test
        static void Main(string[] args)
        {
            DataContext db = new DataContext();
            //Initial
            var model = db.Blog;
            db.Blog.RemoveRange(model);
            var comment = db.Comment;
            db.Comment.RemoveRange(comment);
            var tag = db.Tag;
            db.Tag.RemoveRange(tag);
            db.SaveChanges();
            Console.WriteLine("Clean Done");
            Console.ReadLine();
            Blog blog = new Blog()
            {
                Id = Guid.NewGuid().ToString(),
                Content = "Blog",
                Comments = new List<Comment>
                {
                    new Comment() { Id = Guid.NewGuid().ToString(), Content = "Comment" },
                     new Comment() { Id = Guid.NewGuid().ToString(), Content = "Comment" }
                },
                Tags = new List<Tag>
                {
                    new Tag() { Id = Guid.NewGuid().ToString(), Name = "Tags" },
                    new Tag() { Id = Guid.NewGuid().ToString(), Name = "Tags" }
                }

            };

            db.Blog.Add(blog);
            db.SaveChanges();
            Console.WriteLine("Initial datas");
            Console.ReadLine();
            //Do sth

            //只删除Blog 从表Comment和Tags响应的外键都设定为Null
            //var b = db.Blog.FirstOrDefault(f => f.Content == "Blog");


            //删除掉Comment Tags保留为Null
            var b = db.Blog.Include(i => i.Comments).Include(i=>i.Tags).FirstOrDefault(f => f.Content == "Blog");
            db.Comment.RemoveRange(b.Comments);
            db.Blog.Remove(b);
            db.SaveChanges();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
