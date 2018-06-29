using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Data = Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace DbSizeDeterminer
{
    //class for work with google sheets api
    public class GoogleService
    {
        public static string client_Id = "client_id.json";
        public static readonly string[] scopes = {SheetsService.Scope.Spreadsheets};
        private static readonly string appName = "Google Sheets API dbSizeDeterminer";
        public Data.Spreadsheet responce;
        public SheetsService service;
        
        public void StartService(List<Server> servers)
        {
            UserCredential credential = GetSheetCredentials();
            SheetsService service = GetService(credential);
            Data.Spreadsheet responce = CreateSpreadsheet(service, "Table servers", servers);
            
            this.responce = responce;
            this.service = service;
            
            SetDataHeaderInTable(responce, service);
            SetDataInTable(responce,service, servers);
        }
        // get user credentials
        public UserCredential GetSheetCredentials(){
            using (var stream =
                new FileStream(client_Id, FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/user_credentials.json");

                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
        }
        // get service google api
        public SheetsService GetService(UserCredential userCredential)
        {
            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredential,
                ApplicationName = appName 
            });
        }
        // get table and list in google sheets
        public Data.Spreadsheet CreateSpreadsheet(SheetsService service, string tableName ,List<Server> servers)
        {
            Data.Spreadsheet requestBody = new Data.Spreadsheet();
            
            List<Data.Sheet> list = new List<Data.Sheet>();
            
            for (int i = 0; i < servers.Count; i++)
            {
                list.Add(new Data.Sheet
                {
                    Properties = new Data.SheetProperties
                    {
                        Title = servers[0].name
                    }
                });
            }
            
            requestBody.Sheets = list;
            requestBody.Properties = new Data.SpreadsheetProperties
            {
                Title = tableName
            };
            
            SpreadsheetsResource.CreateRequest request = service.Spreadsheets.Create(requestBody);

            Data.Spreadsheet response = request.Execute();

            return response;
        }
        // set header table in list
        public void SetDataHeaderInTable(Data.Spreadsheet response, SheetsService service)
        {
            string spreadSheetId = response.SpreadsheetId;
            List<int?> sheetsId = new List<int?>();
            string[] dataHeader = {"Server", "DB", "Size, in GB", "Update date" };

            foreach (var item in response.Sheets)
            {
                sheetsId.Add(item.Properties.SheetId);
            }
            
            List<Data.Request> requests = new List<Data.Request>();
            List<Data.CellData> values = new List<Data.CellData>();
            
            foreach (string data in dataHeader)
            {
                values.Add(new Data.CellData()
                {
                    UserEnteredValue = new Data.ExtendedValue()
                    {
                        StringValue = data
                    }
                });
            }
            requests.Add(
                new Data.Request
                {
                    UpdateCells = new Data.UpdateCellsRequest()
                    {
                        Start = new Data.GridCoordinate()
                        {
                            SheetId = sheetsId[0],
                            RowIndex = 0,
                            ColumnIndex = 0
                        },
                        Rows = new List<Data.RowData> {new Data.RowData {Values = values}},
                        Fields = "userEnteredValue"
                    }
                });
            Data.BatchUpdateSpreadsheetRequest bUpd = new Data.BatchUpdateSpreadsheetRequest
            {
                Requests = requests
            };

            service.Spreadsheets.BatchUpdate(bUpd, spreadSheetId).Execute();

        }
        // set servers data in list
        public void SetDataInTable(Data.Spreadsheet response, SheetsService service, List<Server> servers)
        {
            string spreadSheetId = response.SpreadsheetId;
            
            List<int?> sheetsId = new List<int?>();

            string currentDate = DateTime.Now.ToString();

            foreach (var item in response.Sheets)
            {
                sheetsId.Add(item.Properties.SheetId);
            }
            
            for (int i = 0; i < servers.Count; i++)
            {
                Server server = servers[i];
                int? sheetId = sheetsId[i];

                string[,] data = new string[server.dbData.Count,4];
                
                

                foreach (string key in server.dbData.Keys)
                {
                    data.SetValue(server.name, 0,0);
                    data.SetValue(key, 0,1);
                    data.SetValue(server.dbData[key], 0,2);
                    data.SetValue(currentDate,0,3);

                    server.dbData.Remove(key);
                    break;
                }

                int count = 1;
                foreach (string key in server.dbData.Keys)
                {
                    data.SetValue(" ", count,0);
                    data.SetValue(key, count,1);
                    data.SetValue(server.dbData[key], count,2);
                    data.SetValue(" ", count,3);

                    ++count;
                }
                List<Data.Request> requests = new List<Data.Request>();
                

                for (int j = 0; j < data.GetLength(0); j++)
                {
                    List<Data.CellData> values = new List<Data.CellData>();

                    for (int k = 0; k < data.GetLength(1); k++)
                    {
                        values.Add(new Data.CellData()
                        {
                            UserEnteredValue = new Data.ExtendedValue()
                            {
                                StringValue = data[j,k]
                            }
                        });
                        
                    }
                    
                    requests.Add(
                        new Data.Request
                        {
                            UpdateCells = new Data.UpdateCellsRequest()
                            {
                                Start = new Data.GridCoordinate()
                                {
                                    SheetId = sheetId,
                                    RowIndex = j + 1,
                                    ColumnIndex = 0
                                },
                                Rows = new List<Data.RowData> {new Data.RowData {Values = values}},
                                Fields = "userEnteredValue"
                            }
                        });
                    Data.BatchUpdateSpreadsheetRequest bUpd = new Data.BatchUpdateSpreadsheetRequest
                    {
                        Requests = requests
                    };

                    service.Spreadsheets.BatchUpdate(bUpd, spreadSheetId).Execute();
                                     
                }
                string[] dataBasement = {server.name, "Volume",server.size, currentDate };
                
                List<Data.CellData> valuesB = new List<Data.CellData>();
                foreach (string dataB in dataBasement)
                {
                    valuesB.Add(new Data.CellData()
                    {
                        UserEnteredValue = new Data.ExtendedValue()
                        {
                            StringValue = dataB
                        }
                    });
                }
                requests.Add(
                    new Data.Request
                    {
                        UpdateCells = new Data.UpdateCellsRequest()
                        {
                            Start = new Data.GridCoordinate()
                            {
                                SheetId = sheetId,
                                RowIndex = server.dbData.Count + 1,
                                ColumnIndex = 0
                            },
                            Rows = new List<Data.RowData> {new Data.RowData {Values = valuesB}},
                            Fields = "userEnteredValue"
                        }
                    });
                Data.BatchUpdateSpreadsheetRequest bUpsRequest = new Data.BatchUpdateSpreadsheetRequest
                {
                    Requests = requests
                };

                service.Spreadsheets.BatchUpdate(bUpsRequest, spreadSheetId).Execute();
            } 
        }      
    }
}