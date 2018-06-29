using System;
using System.Collections.Generic;
using System.Threading;

namespace DbSizeDeterminer
{
    class Program
    {
        static void Main(string[] args)
        {
            StartApp();
        }

        
        public static void StartApp()
        {
            DAO dao = new DAO();
            List<Server> servers = dao.GetConfigData();
            dao.GetDbData(servers);
            GoogleService googleService = new GoogleService();
            googleService.StartService(servers);
            List<Object> services = new List<object>();
            services.Add(dao);
            services.Add(googleService);
            Thread thread = new Thread(UpdateData);
            thread.Start(services);
            
            string exit = "exit";
            // wait users input
            while(Console.ReadLine() != exit){}

            try
            {
                thread.Abort();
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("The application ended");
            }
        }
        // update data in google lists
        public static void UpdateData(object list)
        {
            while (Thread.CurrentThread.IsAlive)
            {
                Thread.Sleep(7000);
                
                List <Object> services = (List<Object>)list;
                DAO dao = (DAO) services[0];
                
                GoogleService gService = (GoogleService) services[1];
                List<Server> servers = dao.GetConfigData();
                
                dao.GetDbData(servers);
                gService.SetDataInTable(gService.responce, gService.service, servers);
                Console.WriteLine("Update");
            }  
        }
    }
}