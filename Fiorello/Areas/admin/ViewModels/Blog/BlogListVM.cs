namespace Fiorello.Areas.admin.ViewModels.Blog
{
	public class BlogListVM
	{
        public BlogListVM()
        {
            Blogs = new List<Models.Blog>();
        }
        public List<Models.Blog> Blogs { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int Take { get; set; } = 3;
        public int TotalPage { get; set; }
    }
}
