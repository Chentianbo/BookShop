﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

//***************************************
// 作者: ∮明天去要饭
// QICQ: 305725744
// .Net群: 6370988
// http://blog.csdn.net/kgdiwss
//***************************************

namespace JN.Services.Tool
{
    /// <summary>
    /// OperateXmlByDataSet 的摘要说明。
    /// </summary>
    public class XmlDataSet
    {
        public XmlDataSet()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        #region GetDataSetByXml
        /// <summary>
        /// 读取xml直接返回DataSet
        /// </summary>
        /// <param name="strXmlPath">xml文件相对路径</param>
        /// <returns></returns>
        public static DataSet GetDataSetByXml(string strXmlPath)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(GetXmlFullPath(strXmlPath));
                if (ds.Tables.Count > 0)
                {
                    return ds;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region GetDataViewByXml
        /// <summary>
        /// 读取Xml返回一个经排序或筛选后的DataView
        /// </summary>
        /// <param name="strXmlPath"></param>
        /// <param name="strWhere">筛选条件，如："name = 'kgdiwss'"</param>
        /// <param name="strSort">排序条件，如："Id desc"</param>
        /// <returns></returns>
        public static DataView GetDataViewByXml(string strXmlPath, string strWhere, string strSort)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(GetXmlFullPath(strXmlPath));
                DataView dv = new DataView(ds.Tables[0]);
                if (strSort != null)
                {
                    dv.Sort = strSort;
                }
                if (strWhere != null)
                {
                    dv.RowFilter = strWhere;
                }
                return dv;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region WriteXmlByDataSet
        /// <summary>
        /// 向Xml文件插入一行数据
        /// </summary>
        /// <param name="strXmlPath">xml文件相对路径</param>
        /// <param name="Columns">要插入行的列名数组，如：string[] Columns = {"name","IsMarried"};</param>
        /// <param name="ColumnValue">要插入行每列的值数组，如：string[] ColumnValue={"明天去要饭","false"};</param>
        /// <returns>成功返回true,否则返回false</returns>
        public static bool WriteXmlByDataSet(string strXmlPath, string[] Columns, string[] ColumnValue)
        {
            try
            {
                //根据传入的XML路径得到.XSD的路径，两个文件放在同一个目录下
                string strXsdPath = strXmlPath.Substring(0, strXmlPath.IndexOf(".")) + ".xsd";

                DataSet ds = new DataSet();
                //读xml架构，关系到列的数据类型
                ds.ReadXmlSchema(GetXmlFullPath(strXsdPath));
                ds.ReadXml(GetXmlFullPath(strXmlPath));
                DataTable dt = ds.Tables[0];
                //在原来的表格基础上创建新行
                DataRow newRow = dt.NewRow();

                //循环给一行中的各个列赋值
                for (int i = 0; i < Columns.Length; i++)
                {
                    newRow[Columns[i]] = ColumnValue[i];
                }
                dt.Rows.Add(newRow);
                dt.AcceptChanges();
                ds.AcceptChanges();

                ds.WriteXml(GetXmlFullPath(strXmlPath));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region UpdateXmlRow
        /// <summary>
        /// 更行符合条件的一条Xml记录
        /// </summary>
        /// <param name="strXmlPath">XML文件路径</param>
        /// <param name="Columns">列名数组</param>
        /// <param name="ColumnValue">列值数组</param>
        /// <param name="strWhereColumnName">条件列名</param>
        /// <param name="strWhereColumnValue">条件列值</param>
        /// <returns></returns>
        public static bool UpdateXmlRow(string strXmlPath, string[] Columns, string[] ColumnValue, string strWhereColumnName, string strWhereColumnValue)
        {
            try
            {
                string strXsdPath = strXmlPath.Substring(0, strXmlPath.IndexOf(".")) + ".xsd";

                DataSet ds = new DataSet();
                //读xml架构，关系到列的数据类型
                ds.ReadXmlSchema(GetXmlFullPath(strXsdPath));
                ds.ReadXml(GetXmlFullPath(strXmlPath));

                //先判断行数
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        //如果当前记录为符合Where条件的记录
                        if (ds.Tables[0].Rows[i][strWhereColumnName].ToString().Trim().Equals(strWhereColumnValue))
                        {
                            //循环给找到行的各列赋新值
                            for (int j = 0; j < Columns.Length; j++)
                            {
                                ds.Tables[0].Rows[i][Columns[j]] = ColumnValue[j];
                            }
                            //更新DataSet
                            ds.AcceptChanges();
                            //重新写入XML文件
                            ds.WriteXml(GetXmlFullPath(strXmlPath));
                            return true;
                        }
                    }

                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region DeleteXmlRowByIndex
        /// <summary>
        /// 通过删除DataSet中iDeleteRow这一行，然后重写Xml以实现删除指定行
        /// </summary>
        /// <param name="strXmlPath"></param>
        /// <param name="iDeleteRow">要删除的行在DataSet中的Index值</param>
        public static bool DeleteXmlRowByIndex(string strXmlPath, int iDeleteRow)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(GetXmlFullPath(strXmlPath));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //删除符号条件的行
                    ds.Tables[0].Rows[iDeleteRow].Delete();
                }
                ds.WriteXml(GetXmlFullPath(strXmlPath));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region DeleteXmlRows
        /// <summary>
        /// 删除strColumn列中值为ColumnValue的行
        /// </summary>
        /// <param name="strXmlPath">xml相对路径</param>
        /// <param name="strColumn">列名</param>
        /// <param name="ColumnValue">strColumn列中值为ColumnValue的行均会被删除</param>
        /// <returns></returns>
        public static bool DeleteXmlRows(string strXmlPath, string strColumn, string[] ColumnValue)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(GetXmlFullPath(strXmlPath));

                //先判断行数
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //判断行多还是删除的值多，多的for循环放在里面
                    if (ColumnValue.Length > ds.Tables[0].Rows.Count)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            for (int j = 0; j < ColumnValue.Length; j++)
                            {
                                if (ds.Tables[0].Rows[i][strColumn].ToString().Trim().Equals(ColumnValue[j]))
                                {
                                    ds.Tables[0].Rows[i].Delete();
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < ColumnValue.Length; j++)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                if (ds.Tables[0].Rows[i][strColumn].ToString().Trim().Equals(ColumnValue[j]))
                                {
                                    ds.Tables[0].Rows[i].Delete();
                                }
                            }
                        }
                    }
                    ds.WriteXml(GetXmlFullPath(strXmlPath));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region DeleteXmlAllRows
        /// <summary>
        /// 删除所有行
        /// </summary>
        /// <param name="strXmlPath">XML路径</param>
        /// <returns></returns>
        public static bool DeleteXmlAllRows(string strXmlPath)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(GetXmlFullPath(strXmlPath));
                //如果记录条数大于0
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //移除所有记录
                    ds.Tables[0].Rows.Clear();
                }
                //重新写入，这时XML文件中就只剩根节点了
                ds.WriteXml(GetXmlFullPath(strXmlPath));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region GetXmlFullPath
        /// <summary>
        /// 返回完整路径
        /// </summary>
        /// <param name="strPath">Xml的路径</param>
        /// <returns></returns>
        public static string GetXmlFullPath(string strPath)
        {
            if (strPath.IndexOf(":") > 0)
            {
                return strPath;
            }
            else
            {
                return System.Web.HttpContext.Current.Server.MapPath(strPath);
            }
        }
        #endregion
    }
}
