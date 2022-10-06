using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using HtmlAgilityPack;
using mshtml;

namespace IR_project
{
    class Program
    {

        static int counter=0;
        static void database_save(string URL, string Text)
        {


            string connetionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\C#\IR_project\IR_project\Links.mdf;Integrated Security=True";

            try
            {
                SqlConnection sqlconn = new SqlConnection(connetionString);
                sqlconn.Open();

                SqlCommand command = new SqlCommand("INSERT INTO dbo.DOCS (URL,Text) VALUES (@URL,@Text)"
                        , sqlconn);



                command.Parameters.AddWithValue("@URL", URL);
                command.Parameters.AddWithValue("@Text", Text);
                command.ExecuteNonQuery();
                Console.WriteLine("done");


                sqlconn.Close();
            }
            catch
            {
                Console.WriteLine("fail");
            }
        }

        public static Queue<string> start()
        {
            SqlCommand command;
            Queue<string> db_content = new Queue<string>();
            string connetionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\C#\IR_project\IR_project\Links.mdf;Integrated Security=True";
            SqlConnection conn = new SqlConnection(connetionString);
            conn.Open();
            command = new SqlCommand("select url from dbo.DOCS ", conn);
            command.CommandType = System.Data.CommandType.Text;
            SqlDataReader rd = command.ExecuteReader();
            while (rd.Read())
            {
                db_content.Enqueue(rd[0].ToString());
            }

            rd.Close();
            conn.Close();
            return db_content;
        }

        public static  Queue<string> URL_links = new Queue<string>();
        public static Queue<string> visited_URL_links = new Queue<string>();
        public static Queue<string> saved_SQL_links = start();
        public static void func(object seed){

            
            String URL =(string) seed;

            
            URL_links.Enqueue(URL);
           
            while (URL_links.Count != 0)
            {
                // Create a new 'WebRequest' object to the mentioned URL.
                string currentURL = URL_links.Dequeue();
                visited_URL_links.Enqueue(currentURL);

                if (saved_SQL_links.Contains(currentURL)&&counter>30)
                   continue;

                WebRequest myWebRequest;
                WebResponse myWebResponse;
                try
                {
                    myWebRequest = WebRequest.Create(currentURL);
                    myWebResponse = myWebRequest.GetResponse();
                }
                catch (Exception e)
                {
                    continue;
                }

                Stream streamResponse = myWebResponse.GetResponseStream();
                StreamReader sReader = new StreamReader(streamResponse);
                string rString = sReader.ReadToEnd().ToString();

                sReader.Close();
                myWebResponse.Close();

                if (counter == 3000)
                    break;

                Console.WriteLine(currentURL);


                var pageDoc1 = new HtmlDocument();
                pageDoc1.LoadHtml(rString);
                string Body1 = pageDoc1.DocumentNode.InnerText;

                Body1 = Regex.Replace(Body1, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                Body1 = Regex.Replace(Body1, @"[^\u0000-\u007F]", "");
                Body1 = Regex.Replace(Body1, @"[^\u0020-\u007E]", "");


                if (Regex.IsMatch(Body1, @"^[a-zA-Z0-9]+$"))
                    continue;




                IHTMLDocument2 myDoc = new HTMLDocumentClass();
                myDoc.write(rString);
                IHTMLElementCollection elements = myDoc.links;
                foreach (IHTMLElement el in elements)
                {



                    string link = (string)el.getAttribute("href", 0);

                    try
                    {
                        WebRequest request = WebRequest.Create(link);
                        WebResponse response = request.GetResponse();
                        string type = response.ContentType;

                        if (!(type.Contains("UTF-8") || type.Contains("utf-8")))
                            continue;

                        
                        if (visited_URL_links.Contains(link))
                            continue;

                        if (!link.Contains(".bbc."))
                            continue;


                        visited_URL_links.Enqueue(link);
                        URL_links.Enqueue(link);


                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }


                if (Body1 != "" && !saved_SQL_links.Contains(currentURL))
                {


                    saved_SQL_links.Enqueue(currentURL);
                    database_save(currentURL, Body1);
                    counter++;
                    Console.WriteLine(counter);
                }
                else
                    continue;



            }

}

        static Queue q;
      
        static int NumOfThread;
        
        static void Main(string[] args)
        {

            q = new Queue();

            counter = 0;
            NumOfThread = 0;
           

            
            q.Enqueue("https://www.bbc.com/news");
            q.Enqueue("https://www.bbc.com/sport/rugby-union");
            q.Enqueue("https://www.bbc.com/sport/rugby-union/welsh");
            q.Enqueue("https://www.bbc.com/worklife");
            q.Enqueue("https://www.bbc.com");
            q.Enqueue("https://www.bbc.com/culture");
            q.Enqueue("https://www.bbc.com/news/science_and_environment");
            q.Enqueue("https://www.bbc.com/news/coronavirus");
            q.Enqueue("https://www.bbc.com/worklife");
            q.Enqueue("https://www.bbc.com/reel");
            while (true)
            {
                try
                {
                    if (NumOfThread < 10)
                    {
                        NumOfThread++;
                        Thread th = new Thread(new ParameterizedThreadStart(func));
                        th.Start(q.Dequeue());
                    }

                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);
                }
            }




        }




    }
}




