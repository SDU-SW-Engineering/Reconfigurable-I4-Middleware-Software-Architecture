using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchestrator
{
    class Database
    {
        private readonly Dictionary<string, List<Tuple<string, string, string, string[], string, string>>> recipyDatabase;
        private readonly Dictionary<string, List<Tuple<string, string, int, int>>> cancelRecipyDatabase;
        public Database()
        {
            //(topic, message, expected response, variables to be saved, first OPC variable to be set, second OPC variable to be set)
            recipyDatabase = new Dictionary<string, List<Tuple<string, string, string, string[], string, string>>>();
            cancelRecipyDatabase = new Dictionary<string, List<Tuple<string, string, int, int>>>();

            var recipy = new List<Tuple<string, string, string, string[], string, string>>(){
                //Effimat 0
                new Tuple<string, string, string, string[], string, string>("Storage",
                "{\r\n  \"@id\":\"d11972cc-247a-4107-b899-5e2e29ab4257\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\" \",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"getCarrier\",\r\n  \"parameters\":\r\n    {\r\n      \"productId\":\"1012\"\r\n    }\r\n  \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"d11972cc-247a-4107-b899-5e2e29ab4257\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Storage/Effimat\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"effimatReferenceId\":null,\r\n      \"carrierSlot\":null,\r\n      \"success\":true\r\n    }\r\n}",
                new string[]{"effimatReferenceId", "carrierSlot"},
                "GetCarrier", "CarrierAtOpening"),

                //ER 1
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"a388f1d3-a753-4c87-a519-a0089be2d23d\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"moveToStorage\",\r\n  \"parameters\":\r\n    {\r\n      \"carrierSlot\":$carrierSlot\r\n    }\r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"a388f1d3-a753-4c87-a519-a0089be2d23d\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "MoveToWarehouse", "AtWarehouse"),

                //ER 2
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"a388f1d3-a753-4c87-a519-a9989be2d23d\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"pickItem\",\r\n  \"parameters\":\r\n    {\r\n      \"carrierSlot\":$carrierSlot\r\n    }\r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"a388f1d3-a753-4c87-a519-a9989be2d23d\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "PickUpAtWarehouse", "PartPickedAtWarehouse"),

                //Effimat 3
                new Tuple<string, string, string, string[], string, string>("Storage",
                "{\r\n  \"@id\":\"9b514787-88a7-4856-a747-4f54ff1e471f\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Storage/Effimat\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"returnCarrier\",\r\n  \"parameters\":\r\n    {\r\n      \"effimatReferenceId\":$effimatReferenceId\r\n    }\r\n  \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"9b514787-88a7-4856-a747-4f54ff1e471f\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Storage/Effimat\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n     \"success\":true\r\n    }\r\n}",
                null,
                "PutCarrierBack", "CarrierInStock"),

                //ER 4    
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"10cc6142-31c2-483b-906a-6d08a69cec7c\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"moveToInlet\"   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"10cc6142-31c2-483b-906a-6d08a69cec7c\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "MoveToB&Rinlet", "AtB&RInlet"),

                //ACOPOStrak 5
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"5d4099b6-ce00-4c1a-8c15-386408a61c64\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"`\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"moveAssemblyPart\"   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"5d4099b6-ce00-4c1a-8c15-386408a61c64\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "MoveAssemblyPart", "WaitingLoadOn"),

                //ER 6           
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"10cc6142-31c2-483b-906a-70d6a69cec7c\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"loadShuttle\"   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"10cc6142-31c2-483b-906a-70d6a69cec7c\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "LoadShuttle", "ShuttleLoaded"),

                //ACOPOStrak 7
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"7374cc97-bcbb-4474-a90d-4cf771fb1b5b\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"loadOnTrack\"   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"7374cc97-bcbb-4474-a90d-4cf771fb1b5b\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "LoadOnTrack", "WaitingCell1"),

                //UR 8
                new Tuple<string, string, string, string[], string, string>("Assembly",
                "{\r\n  \"@id\":\"33bec44f-30d5-4716-af94-4c8f420bbba9\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Assembly/UR\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"start\",\r\n  \"parameters\":\r\n    {\r\n      \"cellNumber\":1 \r\n    }\r\n   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"33bec44f-30d5-4716-af94-4c8f420bbba9\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Assembly/UR\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n    \"cellId\":\"1\",   \r\n \"success\":true\r\n    }\r\n}",
                null,
                "StartCell1", "EndCell1"),

                //ACOPOStrak 9
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"10576139-d995-4d7f-b679-9b535332f02e\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"taskComplete\",\r\n  \"parameters\":\r\n    {\r\n      \"cellNumber\":1 \r\n    }\r\n   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"10576139-d995-4d7f-b679-9b535332f02e\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "TaskCompleteCell1", "WaitingCell2"),

                //UR 10
                new Tuple<string, string, string, string[], string, string>("Assembly",
                "{\r\n  \"@id\":\"3a6a4164-b441-4833-91b0-0094543f3975\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Assembly/UR\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"start\",\r\n  \"parameters\":\r\n    {\r\n      \"cellNumber\":2 \r\n    }\r\n   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"3a6a4164-b441-4833-91b0-0094543f3975\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Assembly/UR\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n    \"cellId\":\"2\",   \r\n \"success\":true\r\n    }\r\n}",
                null,
                "StartCell2", "EndCell2"),


                //ACOPOStrak 11
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"d3b583a1-56c9-43a1-99dd-ba7f108da37a\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"taskComplete\",\r\n  \"parameters\":\r\n    {\r\n      \"cellNumber\":2 \r\n    }\r\n   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"d3b583a1-56c9-43a1-99dd-ba7f108da37a\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "TaskCompleteCell2", "WaitingLoadOff"),

                //ER                
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"3fcc2041-24b7-470f-bc6e-3de05c0724aa\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"moveToOutlet\"   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"3fcc2041-24b7-470f-bc6e-3de05c0724aa\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "MoveToB&ROutlet", "AtB&ROutlet"), //sometimes Outlet is written with capital, and sometimes not


                //ER 12                 
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"2f0d7927-84ec-4334-b4ad-39b1d6577a2b\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"pickPartFromTrack\"   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"2f0d7927-84ec-4334-b4ad-39b1d6577a2b\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "UnloadShuttle", "ShuttleUnloaded"),

                //ACOPOStrak 13
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"2f0d7927-84ec-4334-b4ad-29b1d657ae2a\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"loadOffTrack\"   \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"2f0d7927-84ec-4334-b4ad-29b1d657ae2a\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "LoadOffTrack", null),

                //Effimat
                new Tuple<string, string, string, string[], string, string>("Storage",
                "{\r\n  \"@id\":\"4b8b335a-054b-42a5-a107-dfdd8466cfd5\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Storage/Effimat\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"getCarrier\",\r\n  \"parameters\":\r\n    {\r\n      \"productId\":\"1012\"\r\n    }\r\n  \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"4b8b335a-054b-42a5-a107-dfdd8466cfd5\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Storage/Effimat\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"effimatReferenceId\":null,\r\n      \"carrierSlot\":null,\r\n      \"success\":true\r\n    }\r\n}",
                new string[]{"effimatReferenceId", "carrierSlot"},
                "GetCarrier", "CarrierAtOpening"),

                //ER                
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"088556ee-4eec-47cf-b875-ace5f9c50582\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"moveToStorage\",\r\n  \"parameters\":\r\n    {\r\n      \"carrierSlot\":$carrierSlot\r\n    }\r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"088556ee-4eec-47cf-b875-ace5f9c50582\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "MoveToWarehouse", "AtWarehouse"),

                //ER                
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"a7e29580-7866-4a57-bd4c-cd73fdd5f589\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"putItem\",\r\n  \"parameters\":\r\n    {\r\n      \"carrierSlot\":$carrierSlot\r\n    }\r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"a7e29580-7866-4a57-bd4c-cd73fdd5f589\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "PlacePartAtWarehouse", "PartPlacedAtWarehouse"),

                //Effimat
                new Tuple<string, string, string, string[], string, string>("Storage",
                "{\r\n  \"@id\":\"8aa1ed11-0e5d-4bd2-94fc-60447672ca90\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Storage/Effimat\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"returnCarrier\",\r\n  \"parameters\":\r\n    {\r\n      \"effimatReferenceId\":$effimatReferenceId\r\n    }\r\n  \r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"8aa1ed11-0e5d-4bd2-94fc-60447672ca90\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Storage/Effimat\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n     \"success\":true\r\n    }\r\n}",
                null,
                "PutCarrierBack", "CarrierInStock"),

                //ER                
                new Tuple<string, string, string, string[], string, string>("Transport",
                "{\r\n  \"@id\":\"e87cba2a-d2b8-4d72-97ba-bf407fc98acb\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"moveToCharger\"\r\n}",
                "{\r\n  \"@id\":null,\r\n  \"@type\":\"response\",\r\n  \"operationId\":\"e87cba2a-d2b8-4d72-97ba-bf407fc98acb\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"response\":\r\n    {\r\n      \"success\":true\r\n    }\r\n}",
                null,
                "null", "null"),
            };
            recipyDatabase.Add("drone", recipy);

            var cancelationProcedure = new List<Tuple<string, string, int, int>>()
            {
                new Tuple<string, string, int, int>("Storage",
                "{\r\n  \"@id\":\"9b514787-88a7-4856-a747-4f54ff1e471f\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Storage/Effimat\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"returnCarrier\",\r\n  \"parameters\":\r\n    {\r\n      \"effimatReferenceId\":$effimatReferenceId\r\n    }\r\n  \r\n}",
                0, 3
                )/*,
                new Tuple<string, string, int, int>("Transport",
                "{\r\n  \"@id\":\"7374cc97-bcbb-4474-a90d-4cf771fb1b5b\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/ER\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"moveToCharger\"   \r\n}",
                1, 13
                ),
                new Tuple<string, string, int, int>("Transport",
                "{\r\n  \"@id\":\"7374cc97-bcbb-4474-a90d-4cf771fb1b5b\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"loadOnTrack\"   \r\n}",
                5, 7
                ),
                new Tuple<string, string, int, int>("Transport",
                "{\r\n  \"@id\":\"10576139-d995-4d7f-b679-9b535332f02e\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"taskComplete\",\r\n  \"parameters\":\r\n    {\r\n      \"cellNumber\":1 \r\n    }\r\n   \r\n}",
                5, 9
                ),
                new Tuple<string, string, int, int>("Transport",
                "{\r\n  \"@id\":\"d3b583a1-56c9-43a1-99dd-ba7f108da37a\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"taskComplete\",\r\n  \"parameters\":\r\n    {\r\n      \"cellNumber\":2 \r\n    }\r\n   \r\n}",
                5, 11
                ),
                new Tuple<string, string, int, int>("Transport",
                "{\r\n  \"@id\":\"2f0d7927-84ec-4334-b4ad-29b1d657ae2a\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Transport/Track\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"loadOffTrack\"   \r\n}",
                5, 13
                ),*/
                //new Tuple<string, string, int, int>("Assembly",
                //"{\r\n  \"@id\":\"3a6a4164-b441-4833-91b0-0094543f3975\",\r\n  \"@type\":\"operation\",\r\n  \"aasOriginId\":\"i4.sdu.dk/Middleware/Orchestrator\",\r\n  \"aasTargetId\":\"i4.sdu.dk/Assembly/UR\",\r\n  \"orderId\":\"123456789\",\r\n  \"operation\":\"reset\"   \r\n}",
                //8, 10
                //)
            };
            cancelRecipyDatabase.Add("drone", cancelationProcedure);
        }
        public List<Tuple<string, string, string, string[], string, string>> GetRecipy(string recipy)
        {
            return recipyDatabase[recipy];
        }

        public List<Tuple<string, string>> GetCancelProcedure(string recipy, int reachedProgess)
        {
            return cancelRecipyDatabase[recipy]
                .Where(x => x.Item3 <= reachedProgess && x.Item4 > reachedProgess)
                .Select(x => new Tuple<string, string>(x.Item1, x.Item2))
                .ToList();
        }
    }
}
