 


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
        public DbSet<BookInfo> BookInfo { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class BookInfo
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
				[DisplayName("BookName")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  BookName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Author")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Author { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ISBN")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  ISBN { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Spec")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Spec { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Publisher")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Publisher { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ImageUrl")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  ImageUrl { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BookCategoryId")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  BookCategoryId { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Version")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Version { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PrintDate")]
				public DateTime?  PrintDate { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PageCount")]
				public int?  PageCount { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("WordCount")]
				public int?  WordCount { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Introduction")]
				public string  Introduction { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsTranslate")]
				public bool?  IsTranslate { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Translator")]
				public bool?  Translator { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AuthorIntroduction")]
				public string  AuthorIntroduction { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Catalog")]
				public string  Catalog { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Description")]
				public string  Description { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Preface")]
				public string  Preface { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UId")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  UID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BookState")]
				public int  BookState { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Sign")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Sign { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("OlaPrice")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  OlaPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CurrentPrice")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  CurrentPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TranslatorIntroduction")]
				public string  TranslatorIntroduction { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BookCount")]
				public int  BookCount { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("FreightPrice")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  FreightPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserName")]
		        [MaxLength(20,ErrorMessage="最大长度为20")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ShowPlace")]
				public int?  ShowPlace { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsTop")]
				public bool?  IsTop { get; set; }




        /// <summary>
        /// 构造函数
        /// </summary>

        public BookInfo()
        {
            ID = Guid.NewGuid().ToString();
            CreateTime = DateTime.Now;
            IsTop = false;
        }
        //生成加密串
        public void CreateSign()
        {
            Sign = (BookName + Author + ISBN + BookCategoryId + (Convert.ToInt32(CurrentPrice)).ToString() + (Convert.ToInt32(OlaPrice)).ToString() + UID + BookState.ToString() + (Convert.ToInt32(FreightPrice)).ToString()+ ShowPlace.ToString()+IsTop.ToString()).ToLower().ToMD5();
        }

    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface IBookInfoService :IServiceBase<BookInfo> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(BookInfo entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class BookInfoService :  ServiceBase<BookInfo>,IBookInfoService
    {


        public BookInfoService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(BookInfo entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
