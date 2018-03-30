 


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
        public DbSet<User> User { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DisplayName("")]
    public partial class User
    {



        /// <summary>
        /// 
        /// </summary>  
        [DisplayName("ID")]
        [MaxLength(50, ErrorMessage = "最大长度为50")]
        [Key]
        public string ID { get; set; }



        /// <summary>
        /// 用户名
        /// </summary>  
        [DisplayName("用户名")]
        [MaxLength(20, ErrorMessage = "用户名最大长度为20")]
        public string UserName { get; set; }



        /// <summary>
        /// 学号
        /// </summary>  
        [DisplayName("学号")]
        [MaxLength(20, ErrorMessage = "学号最大长度为20")]
        public string StudentNumber { get; set; }



        /// <summary>
        /// 登录密码
        /// </summary>  
        [DisplayName("登录密码")]
        [MaxLength(50, ErrorMessage = "登录密码最大长度为50")]
        public string Password { get; set; }



        /// <summary>
        /// 二级密码
        /// </summary>  
        [DisplayName("二级密码")]
        [MaxLength(50, ErrorMessage = "二级密码最大长度为50")]
        public string Password2 { get; set; }



        /// <summary>
        /// 三级密码
        /// </summary>  
        [DisplayName("三级密码")]
        [MaxLength(50, ErrorMessage = "三级密码最大长度为50")]
        public string Password3 { get; set; }



        /// <summary>
        /// 昵称
        /// </summary>  
        [DisplayName("昵称")]
        [MaxLength(20, ErrorMessage = "昵称最大长度为20")]
        public string NickName { get; set; }



        /// <summary>
        /// 真实姓名
        /// </summary>  
        [DisplayName("真实姓名")]
        [MaxLength(50, ErrorMessage = "真实姓名最大长度为50")]
        public string RealName { get; set; }



        /// <summary>
        /// 性别
        /// </summary>  
        [DisplayName("性别")]
        [MaxLength(2, ErrorMessage = "性别最大长度为2")]
        public string Sex { get; set; }



        /// <summary>
        /// 生日
        /// </summary>  
        [DisplayName("生日")]
        public DateTime? Birthday { get; set; }



        /// <summary>
        /// 邮箱
        /// </summary>  
        [DisplayName("邮箱")]
        [MaxLength(30, ErrorMessage = "邮箱最大长度为30")]
        public string Email { get; set; }



        /// <summary>
        /// 
        /// </summary>  
        [DisplayName("QQ")]
        [MaxLength(20, ErrorMessage = "最大长度为20")]
        public string QQ { get; set; }



        /// <summary>
        /// 国家
        /// </summary>  
        [DisplayName("国家")]
        [MaxLength(50, ErrorMessage = "国家最大长度为50")]
        public string Country { get; set; }



        /// <summary>
        /// 省份
        /// </summary>  
        [DisplayName("省份")]
        [MaxLength(50, ErrorMessage = "省份最大长度为50")]
        public string Province { get; set; }



        /// <summary>
        /// 城市
        /// </summary>  
        [DisplayName("城市")]
        [MaxLength(50, ErrorMessage = "城市最大长度为50")]
        public string City { get; set; }



        /// <summary>
        /// 地址
        /// </summary>  
        [DisplayName("地址")]
        [MaxLength(50, ErrorMessage = "地址最大长度为50")]
        public string Address { get; set; }



        /// <summary>
        /// 手机
        /// </summary>  
        [DisplayName("手机")]
        [MaxLength(50, ErrorMessage = "手机最大长度为50")]
        public string Mobile { get; set; }



        /// <summary>
        /// 电话
        /// </summary>  
        [DisplayName("电话")]
        [MaxLength(50, ErrorMessage = "电话最大长度为50")]
        public string TelePhone { get; set; }



        /// <summary>
        /// 头像
        /// </summary>  
        [DisplayName("头像")]
        [MaxLength(250, ErrorMessage = "头像最大长度为250")]
        public string HeadFace { get; set; }



        /// <summary>
        /// 账户状态
        /// </summary>  
        [DisplayName("账户状态")]
        public int AccountState { get; set; }



        /// <summary>
        /// 状态改变时间
        /// </summary>  
        [DisplayName("状态改变时间")]
        public DateTime? UpdateAccountStateTime { get; set; }



        /// <summary>
        /// 改变原因
        /// </summary>  
        [DisplayName("改变原因")]
        [MaxLength(250, ErrorMessage = "改变原因最大长度为250")]
        public string UpdateStateReason { get; set; }



        /// <summary>
        /// 身份证号码
        /// </summary>  
        [DisplayName("身份证号码")]
        [MaxLength(50, ErrorMessage = "身份证号码最大长度为50")]
        public string IDCard { get; set; }



        /// <summary>
        /// 密保问题
        /// </summary>  
        [DisplayName("密保问题")]
        [MaxLength(50, ErrorMessage = "密保问题最大长度为50")]
        public string Question { get; set; }



        /// <summary>
        /// 密保答案
        /// </summary>  
        [DisplayName("密保答案")]
        [MaxLength(50, ErrorMessage = "密保答案最大长度为50")]
        public string Answer { get; set; }



        /// <summary>
        /// 注册时间
        /// </summary>  
        [DisplayName("注册时间")]
        public DateTime CreateTime { get; set; }



        /// <summary>
        /// 登录错误次数
        /// </summary>  
        [DisplayName("登录错误次数")]
        public int? InputWrongPwdTimes { get; set; }



        /// <summary>
        /// 最后登录时间
        /// </summary>  
        [DisplayName("最后登录时间")]
        public DateTime? LastLoginTime { get; set; }



        /// <summary>
        /// 最后修改时间
        /// </summary>  
        [DisplayName("最后修改时间")]
        public DateTime? LastUpdateTime { get; set; }



        /// <summary>
        /// 账户余额
        /// </summary>  
        [DisplayName("账户余额")]
        [Filters.DecimalPrecision(18, 2)]
        public decimal? UserWallet { get; set; }



        /// <summary>
        /// 累计消费
        /// </summary>  
        [DisplayName("累计消费")]
        [Filters.DecimalPrecision(18, 2)]
        public decimal? AddupTakeWallet { get; set; }



        /// <summary>
        /// 累计收入
        /// </summary>  
        [DisplayName("累计收入")]
        [Filters.DecimalPrecision(18, 2)]
        public decimal? AddupIncomeWallet { get; set; }



        /// <summary>
        /// 购买次数
        /// </summary>  
        [DisplayName("购买次数")]
        public int? AddupPurchaseCount { get; set; }



        /// <summary>
        /// 卖出次数
        /// </summary>  
        [DisplayName("卖出次数")]
        public int? AddupSelloutCount { get; set; }



        /// <summary>
        /// 支付宝
        /// </summary>  
        [DisplayName("支付宝")]
        [MaxLength(50, ErrorMessage = "支付宝最大长度为50")]
        public string AliPay { get; set; }



        /// <summary>
        /// 微信号
        /// </summary>  
        [DisplayName("微信号")]
        [MaxLength(50, ErrorMessage = "微信号最大长度为50")]
        public string WeiXin { get; set; }



        /// <summary>
        /// 
        /// </summary>  
        [DisplayName("Token")]
        [MaxLength(50, ErrorMessage = "最大长度为50")]
        public string Token { get; set; }



        /// <summary>
        /// 
        /// </summary>  
        [DisplayName("TokenExpirationTime")]
        public DateTime? TokenExpirationTime { get; set; }



        /// <summary>
        /// 
        /// </summary>  
        [DisplayName("LastLoginIP")]
        [MaxLength(50, ErrorMessage = "最大长度为50")]
        public string LastLoginIP { get; set; }



        /// <summary>
        /// 
        /// </summary>  
        [DisplayName("LastLoginAddress")]
        [MaxLength(50, ErrorMessage = "最大长度为50")]
        public string LastLoginAddress { get; set; }



        /// <summary>
        /// 
        /// </summary>  
        [DisplayName("Sign")]
        [MaxLength(50, ErrorMessage = "最大长度为50")]
        public string Sign { get; set; }




        /// <summary>
        /// 构造函数
        /// </summary>

        public User()
        {
            ID = Guid.NewGuid().ToString();
        }
        public void CreateSign()
        {
            //安全秘钥
            Sign = (UserName + StudentNumber + Password + Password2 + UserWallet + AddupTakeWallet + AddupIncomeWallet + AddupPurchaseCount + AddupSelloutCount+ Mobile).ToMD5();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface IUserService :IServiceBase<User> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(User entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class UserService :  ServiceBase<User>,IUserService
    {


        public UserService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(User entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
