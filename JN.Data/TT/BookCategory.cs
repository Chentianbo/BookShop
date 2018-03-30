 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MvcCore;
using System.Data.Entity;
using System.ComponentModel;
using MvcCore.Infrastructure;
using System.Data.Entity.Validation;
namespace JN.Data
{
    public partial class SysDbContext : FrameworkContext
    {
        /// <summary>
        /// 把实体添加到EF上下文
        /// </summary>
        public DbSet<BookCategory> BookCategory { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class BookCategory
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		[Key]
		public string  ID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Name")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Name { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CateImg")]
				public string  CateImg { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ParentId")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ParentId { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("parentName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  parentName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Ppacth")]
				public string  Ppacth { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Color")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  Color { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Sort")]
				public int  Sort { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsNavTop")]
				public bool?  IsNavTop { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsShow")]
				public bool?  IsShow { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public BookCategory()
        {
            ID = Guid.NewGuid().ToString();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface IBookCategoryService :IServiceBase<BookCategory> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(BookCategory entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class BookCategoryService :  ServiceBase<BookCategory>,IBookCategoryService
    {


        public BookCategoryService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(BookCategory entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
