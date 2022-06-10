using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System.Reflection;

namespace CaliberTournamentsV2.GoogleApis
{
    internal class Spreadsheets
    {
        private const string _spreadsheetId = "1qdETrsfY2dks_HjtOZ3lkHcSFl5VFF7jAJWvVlUlyls";

        //#pragma warning disable CS0414 // its ok
        private int? _currentSheetId = 671616174; // Caliber
        //#pragma warning restore CS0414 // its ok

        internal static List<Tuple<string, string>> LoadCommands()
        {
            SheetsService service = GetService(SheetsService.Scope.SpreadsheetsReadonly, "Readonly");

            string range = "Tournament2022!A:B";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(_spreadsheetId, range);

            IList<IList<object>> listRow = request.Execute().Values;

            List<Tuple<string, string>> commands = new();

            foreach (List<object> row in listRow)
                commands.Add(Tuple.Create((string)row[0], (string)row[1]));

            return commands;
        }

        internal async Task SaveDataListPickBans(List<Models.Referee.StartPickBan> listPickBansMap)
        {
            SheetsService service = GetService(SheetsService.Scope.Spreadsheets);

            var requestBody = new BatchUpdateSpreadsheetRequest()
            {
                Requests = GetRequestsSaveData(GetSheetId(service), listPickBansMap)
            };

            SpreadsheetsResource.BatchUpdateRequest request = service.Spreadsheets.BatchUpdate(requestBody, _spreadsheetId);

            await request.ExecuteAsync();
        }

        private static List<Request> GetRequestsSaveData(int pageId, List<Models.Referee.StartPickBan> listPickBansMap)
        {
            List<Request> requests = new()
            {
                new Request()
                {
                    UpdateCells = new UpdateCellsRequest()
                    {
                        Start = new GridCoordinate()
                        {
                            SheetId = pageId,
                            RowIndex = 0,
                            ColumnIndex = 0
                        },
                        Rows = GetDataRows(listPickBansMap),
                        Fields = "userEnteredValue"
                    }
                }
            };

            return requests;
        }

        private static List<RowData> GetDataRows(List<Models.Referee.StartPickBan> listPickBansMap)
        {
            List<RowData> rows = new();

            List<CellData> logRow = new()
            {
                GetCellValue("all data in json"),
                GetCellValue(ConverterLogs.Convert(listPickBansMap)),
            };
            rows.Add(new RowData() { Values = logRow });


            List<CellData> cellValuesHeader = new();
            cellValuesHeader.Add(GetCellValue("Json"));

            cellValuesHeader.Add(GetCellValue("Судья"));
            cellValuesHeader.Add(GetCellValue("Режим голосования"));
            cellValuesHeader.Add(GetCellValue("Команда 1"));
            cellValuesHeader.Add(GetCellValue("Команда 2"));
            cellValuesHeader.Add(GetCellValue("Капитан 1"));
            cellValuesHeader.Add(GetCellValue("Капитан 2"));
            cellValuesHeader.Add(GetCellValue("Дата начала карты"));
            cellValuesHeader.Add(GetCellValue("Дата завершения карты"));
            cellValuesHeader.Add(GetCellValue("Детали"));

            rows.Add(new RowData() { Values = cellValuesHeader });

            foreach (Models.Referee.StartPickBan itemPickBan in listPickBansMap)
            {
                List<CellData> cellValues = new();

                cellValues.Add(GetCellValue(GetJsonStartPickBan(itemPickBan)));

                cellValues.Add(GetCellValue(itemPickBan.NameReferee));
                cellValues.Add(GetCellValue(itemPickBan.Mode.ToString()));
                cellValues.Add(GetCellValue(itemPickBan.Team1Name));
                cellValues.Add(GetCellValue(itemPickBan.Team2Name));
                cellValues.Add(GetCellValue(itemPickBan.Team1?.Capitan?.Name ?? string.Empty));
                cellValues.Add(GetCellValue(itemPickBan.Team2?.Capitan?.Name ?? string.Empty));
                cellValues.Add(GetCellValue(itemPickBan.PickBanMap.DateStart.ToString("HH:mm:ss")));
                cellValues.Add(GetCellValue(itemPickBan.PickBanMap.DateEnd.ToString("HH:mm:ss")));

                string detailedMap = string.Join("\n",
                    itemPickBan.PickBanMap.PickBanDetailed.Select(
                        el => $"{(el.PickBanType == Models.PickBanType.ban ? "Бан" : "Пик")} -- {el.PickBanName} -- {el.Team?.Name} {el.CheckedDateTime:HH:mm:ss}"
                    ));
                cellValues.Add(GetCellValue(detailedMap));

                IEnumerable<Models.PickBans.PickBanDetailed> pickOrNoneMap = itemPickBan.PickBanMap.PickBanDetailed.Where(
                    el => el.PickBanType == Models.PickBanType.pick || el.PickBanType == Models.PickBanType.none);

                foreach (Models.PickBans.PickBanDetailed itemMap in pickOrNoneMap)
                {
                    string pickBanOperators = string.Join("\n",
                        itemMap.Operators.PickBanDetailed.Select(
                            el => $"{(el.PickBanType == Models.PickBanType.ban ? "Бан" : "Пик")} -- {el.PickBanName} -- {el.Team?.Name} {el.CheckedDateTime:HH:mm:ss}"
                        ));

                    cellValues.Add(GetCellValue(pickBanOperators));


                    string operators = string.Join("\n",
                        itemMap.Operators.TeamOperators.Select
                            (el => $"\t{el.Key.Name} \n{string.Join("\n", el.Value.Select(elOper => $"\t\t{elOper.ClassOperator} -- {elOper.OperatorName}"))}"));

                    cellValues.Add(GetCellValue(
                        $"{itemMap.PickBanName} :: {itemMap.Operators.DateStart:HH:mm:ss}-{itemMap.Operators.DateEnd:HH:mm:ss} \n" +
                        $"{operators}"));
                }

                rows.Add(new RowData() { Values = cellValues });
            }

            return rows;
        }

        private static CellData GetCellValue(string value)
            => new() { UserEnteredValue = new ExtendedValue() { StringValue = value } };

        private int GetSheetId(SheetsService service)
        {
            if (_currentSheetId == null)
            {
                int id = AddSheet(service);
                _currentSheetId = id;

                return id;
            }
            else
                return (int)_currentSheetId;
        }

        private static int AddSheet(SheetsService service)
        {
            BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest = new()
            {
                Requests = new List<Request>
            {
                new Request
                {
                    AddSheet = new AddSheetRequest
                        {
                            Properties = new SheetProperties()
                            {
                                Title = "Команды " + DateTime.Now.ToString("yyyy.MM.dd HH:mm: ss")
                            }
                        }
                    }
                }
            };

            var batchUpdateRequest = service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _spreadsheetId);

            var ex = batchUpdateRequest.Execute(); 

            int ID = (int)ex.Replies[0].AddSheet.Properties.SheetId!;

            return ID;
        }

        private static SheetsService GetService(string scopes, string detailed = "")
        {
            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Authorize(scopes, detailed),
                ApplicationName = "Caliber Tournaments",
            });
        }

        private static UserCredential? Authorize(string scopes, string detailed)
        {
            string? appDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory?.FullName;

            if (appDir == null)
                return default;

            string credPathJson = Path.Combine(
                appDir,
                "credentials.json");

            using FileStream stream = new(credPathJson, FileMode.Open, FileAccess.Read);
            UserCredential credential;

            string tokenDir = Path.Combine(
                appDir,
                "credentials" + detailed);

            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                       new string[1] { scopes },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(tokenDir, true)).Result;

            return credential;
        }

        private static string GetJsonStartPickBan(Models.Referee.StartPickBan data)
            => JsonConvert.SerializeObject(data);
    }
}
