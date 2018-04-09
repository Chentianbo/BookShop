 


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
        public DbSet<ShopOrder> ShopOrder { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class ShopOrder
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
				[DisplayName("UID")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  UID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BookID")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  BookID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BookName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  BookName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("OrderNumber")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  OrderNumber { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecProvince")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecProvince { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecCity")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecCity { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecTown")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecTown { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecAddress")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecAddress { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecLinkMan")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecLinkMan { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecPhone")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecPhone { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecZip")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RecZip { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PayTime")]
				public DateTime?  PayTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("FinishTime")]
				public DateTime?  FinishTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("DelivertTime")]
				public DateTime?  DelivertTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TotalCount")]
				public int  TotalCount { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("OrderPrice")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  OrderPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BuyMsg")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  BuyMsg { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Remark")]
				public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Status")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Logistics")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  Logistics { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ShipFreight")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ShipFreight { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Spec")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  Spec { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SenderMan")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  SenderMan { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SenderPhone")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  SenderPhone { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SenderAddress")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  SenderAddress { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SenderProvince")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  SenderProvince { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SenderCity")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  SenderCity { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SenderTown")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  SenderTown { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RecTime")]
				public DateTime?  RecTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Sign")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  Sign { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TotalPrice")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  TotalPrice { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PUID")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  PUID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PUserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  PUserName { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public ShopOrder()
        {
            ID = Guid.NewGuid().ToString();
        }

        public void CreateSign()
        {
            Sign = (UID + BookID + OrderNumber + RecLinkMan + RecPhone + Convert.ToInt32(TotalPrice).ToString() + Logistics + Convert.ToInt32(ShipFreight) + Status.ToString() + SenderMan + SenderPhone).ToLower().ToMD5();
        }
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface IShopOrderService :IServiceBase<ShopOrder> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(ShopOrder entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class ShopOrderService :  ServiceBase<ShopOrder>,IShopOrderService
    {


        public ShopOrderService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(ShopOrder entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
