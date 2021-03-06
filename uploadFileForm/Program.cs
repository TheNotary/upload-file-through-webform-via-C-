﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;


namespace uploadFileForm
{
    class Program
    {
        static void Main(string[] args)
        {
            NameValueCollection nvc = new NameValueCollection();
            //nvc.Add("id", "TTR");
            nvc.Add("table_name", "Products.csv");
            nvc.Add("commit", "Submit");
            HttpUploadFile("http://192.168.0.11:3000/import/import_csv",
                 @"C:\Program Files (x86)\wpTransmit\SendBuffer\Products.csv", "dump[file]", "application/vnd.ms-excel", nvc);
        }




        // http://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data
        public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            //log.Debug(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] boundarybytesF = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "\r\n");  // the first time it itereates, you need to make sure it doesn't put too many new paragraphs down or it completely messes up poor webbrick.  
            

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
            wr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            var nvc2 = new NameValueCollection();
            nvc2.Add("Accepts-Language", "en-us,en;q=0.5");
            wr.Headers.Add(nvc2);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;


            Stream rs = wr.GetRequestStream();

            bool firstLoop = true;
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                if (firstLoop)
                {
                    rs.Write(boundarybytesF, 0, boundarybytesF.Length);
                    firstLoop = false;
                }
                else
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                }
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            //string header = string.Format(headerTemplate, paramName, file, contentType);
            string header = string.Format(headerTemplate, paramName, new FileInfo(file).Name, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                
                //log.Debug(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                //log.Error("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }




    }
}
