using Newtonsoft.Json;
using System;
using System.Data.OleDb;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using ClosedXML.Excel;

namespace MapApplication.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string Submit(HttpPostedFileBase certImageFile)
        {
            WriteProgressCount("0","0");
            bool isValid = false;
            string data = string.Empty;
            try
            {
                DataTable dtOK = new DataTable("dtOK");
                DataTable dtTX = new DataTable("dtTX");
                string rootPath = HostingEnvironment.ApplicationPhysicalPath;
                string currentDrivePath = System.IO.Path.GetPathRoot(rootPath);
                string folderName = currentDrivePath + "Files";
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                var path = string.Format(rootPath + "Document\\");
                if (!string.IsNullOrEmpty(certImageFile.FileName))
                {
                    string _FileName = "OK_Data" + System.IO.Path.GetExtension(certImageFile.FileName);

                    if (System.IO.File.Exists(path + _FileName))
                    {
                        System.IO.File.Delete(path + _FileName);
                    }
                    certImageFile.SaveAs(path + _FileName);
                    if (System.IO.File.Exists(path + "OKReport.xlsx"))
                    {
                        System.IO.File.Delete(path + "OKReport.xlsx");
                    }
                    if (System.IO.File.Exists(path + "TXReport.xlsx"))
                    {
                        System.IO.File.Delete(path + "TXReport.xlsx");
                    }
                    string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"" + path + _FileName + "\";Extended Properties=\"Excel 8.0\"";
                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        connection.Open();
                        OleDbDataAdapter dBAdapter = new OleDbDataAdapter();
                        dBAdapter.SelectCommand = new OleDbCommand("SELECT * FROM [Sheet1$] WHERE STATE='OK'", connection);
                        dBAdapter.Fill(dtOK);
                        dBAdapter.SelectCommand = new OleDbCommand("SELECT * FROM [Sheet1$] WHERE STATE='TX'", connection);
                        dBAdapter.Fill(dtTX);
                    }
                    var totalRecordCount = (dtOK.Rows.Count + dtTX.Rows.Count).ToString();
                    var startTXCount = dtOK.Rows.Count > 0 ? dtOK.Rows.Count : 0;
                    WriteProgressCount("0", totalRecordCount);
                    if (dtOK != null && dtOK.Rows.Count > 0)
                    {
                        using (XLWorkbook wb = new XLWorkbook())
                        {
                            var workSheet = wb.Worksheets.Add(dtOK);
                            workSheet.Rows().AdjustToContents();
                            using (MemoryStream stream = new MemoryStream())
                            {
                                wb.SaveAs(stream);
                                stream.Seek(0, SeekOrigin.Begin);
                                using (var fs = new FileStream(path + "OKReport.xlsx", FileMode.OpenOrCreate))
                                {
                                    stream.CopyTo(fs);
                                }
                            }
                        }
                        string mainOkVariableStr = string.Empty;
                        string mainOkAppendStr = string.Empty;
                        string mainOkFuncStr = string.Empty;
                        string mainQINQgsFieldStr = string.Empty;
                        string mainQINSetAttrStr = string.Empty;
                        if (dtOK.Columns.Count > 5)
                        {
                            for (int i = 6; i <= dtOK.Columns.Count; i++)
                            {
                                var currentColName = dtOK.Columns[i - 1].ColumnName;
                                mainOkVariableStr += currentColName + "=[] \n";
                                mainOkAppendStr += currentColName + ".append(r[\"" + currentColName + "\"]) \n  ";
                                mainOkFuncStr += "," + currentColName;
                                mainQINQgsFieldStr += $",QgsField(\"{currentColName}\", QVariant.String)";
                                mainQINSetAttrStr += $",str({currentColName}[f_indx])";
                            }
                        }

                        string fileName = folderName + @"\MainOk.py";

                        string okReportPath = path + @"OKReport.xlsx";
                        string mainOkPy = System.IO.File.ReadAllText(string.Format(rootPath + "PythonFile\\MainOk.py")).Replace("##ExcelPath##", okReportPath.Replace("\\", "\\\\")).Replace("##mainOkVariableStr##", mainOkVariableStr).Replace("##mainOkAppendStr##", mainOkAppendStr).Replace("##mainOkFuncStr##", mainOkFuncStr);
                        using (FileStream fs = System.IO.File.Create(fileName))
                        {
                            byte[] author = new UTF8Encoding(true).GetBytes(mainOkPy);
                            fs.Write(author, 0, author.Length);
                        }

                        string mainOINOkPy = System.IO.File.ReadAllText(string.Format(rootPath + "PythonFile\\MainQINOk.py")).Replace("##ShpVLayerPath##", folderName.Replace("\\", "\\\\") + "\\\\OK_32025_Sections.shp").Replace("##shpGPath##", path.Replace("\\", "\\\\") + "testSplits_OK.shp").Replace("##myTxtProgressFile##", string.Format(rootPath + "PythonFile\\progressCount.txt").Replace("\\", "\\\\")).Replace("##totalPer##", totalRecordCount).Replace("##mainOkFuncStr##", mainOkFuncStr).Replace("##mainQINQgsFieldStr##", mainQINQgsFieldStr).Replace("##mainQINSetAttrStr##", mainQINSetAttrStr);
                        string qinFileName = folderName + @"\QIN.py";
                        using (FileStream fs = System.IO.File.Create(qinFileName))
                        {
                            byte[] author = new UTF8Encoding(true).GetBytes(mainOINOkPy);
                            fs.Write(author, 0, author.Length);
                        }

                        string rootpath = fileName;
                        QGISFile.CreateFileUsingQGIS(rootpath);
                    }
                    if (dtTX != null && dtTX.Rows.Count > 0)
                    {
                        using (XLWorkbook wb = new XLWorkbook())
                        {
                            var workSheet = wb.Worksheets.Add(dtTX);
                            workSheet.Rows().AdjustToContents();
                            using (MemoryStream stream = new MemoryStream())
                            {
                                wb.SaveAs(stream);
                                stream.Seek(0, SeekOrigin.Begin);
                                using (var fs = new FileStream(path + "TXReport.xlsx", FileMode.OpenOrCreate))
                                {
                                    stream.CopyTo(fs);
                                }
                            }
                        }
                        string mainTXVariableStr = string.Empty;
                        string mainTXAppendStr = string.Empty;
                        string mainTXFuncStr = string.Empty;
                        string mainQINTXQgsFieldStr = string.Empty;
                        string mainQINTXSetAttrStr = string.Empty;
                        if (dtOK.Columns.Count > 5)
                        {
                            for (int i = 6; i <= dtOK.Columns.Count; i++)
                            {
                                var currentColName = dtOK.Columns[i - 1].ColumnName;
                                mainTXVariableStr += currentColName + "=[] \n";
                                mainTXAppendStr += currentColName + ".append(r[\"" + currentColName + "\"]) \n  ";
                                mainTXFuncStr += "," + currentColName;
                                mainQINTXQgsFieldStr += $",QgsField(\"{currentColName}\", QVariant.String)";
                                mainQINTXSetAttrStr += $",str({currentColName}[f_indx])";
                            }
                        }
                        string fileName = folderName + @"\MainTX.py";

                        string txReportPath = path + @"\TXReport.xlsx";
                        string mainTxPy = System.IO.File.ReadAllText(string.Format(rootPath + "PythonFile\\MainOk.py")).Replace("##ExcelPath##", txReportPath.Replace("\\", "\\\\")).Replace("##mainOkVariableStr##", mainTXVariableStr).Replace("##mainOkAppendStr##", mainTXAppendStr).Replace("##mainOkFuncStr##", mainTXFuncStr);
                        using (FileStream fs = System.IO.File.Create(fileName))
                        {
                            byte[] author = new UTF8Encoding(true).GetBytes(mainTxPy);
                            fs.Write(author, 0, author.Length);
                        }

                        string mainOINTxPy = System.IO.File.ReadAllText(string.Format(rootPath + "PythonFile\\MainQINTx.py")).Replace("##ShpVLayerPath##", folderName.Replace("\\", "\\\\") + "\\\\TX_32025_SECT.shp").Replace("##shpGPath##", path.Replace("\\", "\\\\") + "testSplits_Tx.shp").Replace("##myTxtProgressFile##", string.Format(rootPath.Replace("\\", "\\\\") + "PythonFile\\progressCount.txt")).Replace("##totalPer##", totalRecordCount).Replace("##startTXCount##", startTXCount.ToString()).Replace("##mainOkFuncStr##", mainTXFuncStr).Replace("##mainQINQgsFieldStr##", mainQINTXQgsFieldStr).Replace("##mainQINSetAttrStr##", mainQINTXSetAttrStr);
                        string qinFileName = folderName + @"\QIN.py";
                        using (FileStream fs = System.IO.File.Create(qinFileName))
                        {
                            byte[] author = new UTF8Encoding(true).GetBytes(mainOINTxPy);
                            fs.Write(author, 0, author.Length);
                        }

                        string rootpath = fileName;
                        QGISFile.CreateFileUsingQGIS(rootpath);
                    }
                }
                isValid = true;
                data = "File uploaded successfully!!";
            }
            catch (Exception ex)
            {
                isValid = false;
                data = "File uploaded failed!!";
            }
            return JsonConvert.SerializeObject(new { IsValid = isValid, data = data });
        }
        public string ReadProgressCount()
        {
            string rootPath = HostingEnvironment.ApplicationPhysicalPath + "PythonFile\\progressCount.txt";
            string progress = System.IO.File.ReadAllText(rootPath);
            return JsonConvert.SerializeObject(new { data = progress });
        }

        public static void WriteProgressCount(string value = "0",string totaValue="0")
        {
            string rootPath = HostingEnvironment.ApplicationPhysicalPath + "PythonFile\\progressCount.txt";
            System.IO.File.WriteAllText(rootPath,value + "/" +  totaValue);
        }
    }
    public static class QGISFile
    {
        public static void CreateFileUsingQGIS(string path)
        {
            string arg = string.Format(path); // path to the Python code
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(@"C:\Program Files\QGIS 3.34.1\bin\python-qgis.bat", arg);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true; // Hide the command line window
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;
            Process processChild = Process.Start(process.StartInfo);
            while (!processChild.HasExited)
            {
                Thread.Sleep(5000);
            }
            var exitcode = processChild.ExitCode;
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}