using System;
using System.Text.RegularExpressions;

namespace Test_Enviroment.Scripts.Llm
{
    public class ResponseAction
    {
        public string Type { get; set; }
        
        public string Target { get; set; }
    }

    public class Response
    {
        public string ResponseText { get; set; }
        public ResponseAction ResponseAction { get; set; }
    }
    
    public class Prompt
    {
        public string CurrentLocation { get; set; }
        public string DateAndTime { get; set; }
        public string NpcDescription { get; set; }
        public string SystemPrompt { get; set; }
        public string Memory1 { get; set; }
        public string Memory2 { get; set; }
        public string Memory3 { get; set; }
        
        public Prompt (string currentLocation, string dateAndTime, string npcDescription, string systemPrompt, string memory1, string memory2)
        {
            CurrentLocation = currentLocation;
            DateAndTime = dateAndTime;
            NpcDescription = npcDescription;
            SystemPrompt = systemPrompt;
            Memory1 = memory1;
            Memory2 = memory2;
        }
    }
    
    public static class ResponseParser
    {
        
        public static Response ParseResponse(string response)
        {
            var parts = response.Split(new[] { "action: " }, StringSplitOptions.None);
            var responsePart = parts[0].Replace("response: ", "").Trim().Replace("\"", "");
            var actionPart = parts.Length > 1 ? parts[1] : "";

            var actionTypeMatch = Regex.Match(actionPart, @"type: (.*?),");
            var targetMatch = Regex.Match(actionPart, @"target: (.*?)}");

            var cleanedActionType = actionTypeMatch.Groups[1].Value.Trim();
            var cleanedTarget = targetMatch.Groups[1].Value.Trim();

            return new Response
            {
                ResponseText = responsePart.ToLower(),
                ResponseAction = new ResponseAction
                {
                    Type = cleanedActionType.ToLower(),
                    Target = cleanedTarget.ToLower()
                }
            };
        }
        public static string ConstructPrompt(Prompt prompt)
        {
            return "location : " + prompt.CurrentLocation + ", dateAndTime : " + prompt.DateAndTime + 
                   ", Who am i : " + prompt.NpcDescription + ",\n" +
                    prompt.SystemPrompt + ", memory1 : " + prompt.Memory1 + 
                   ", memory2 : " + prompt.Memory2 + ", memory3 : " + prompt.Memory3;
        }
    }
}