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
        public string NpcName { get; set; }
        public string NpcDescription { get; set; }
        public string SystemPrompt { get; set; }
        public string Memory1 { get; set; }
        public string Memory2 { get; set; }
        public string Memory3 { get; set; }
        
        public Prompt (string currentLocation, string dateAndTime, string npcName, string npcDescription, string systemPrompt, string memory1, string memory2, string memory3)
        {
            CurrentLocation = currentLocation;
            DateAndTime = dateAndTime;
            NpcName = npcName;
            NpcDescription = npcDescription;
            SystemPrompt = systemPrompt;
            Memory1 = memory1;
            Memory2 = memory2;
            Memory3 = memory3;
        }
    }
    
    public static class ResponseParser
    {
        public static Response ParseResponse(string response)
        {
            var responseMatch = Regex.Match(response, @"response:(.*?)(,|$)");
            var actionTypeMatch = Regex.Match(response, @"type:(.*?)(,|$)");
            var targetMatch = Regex.Match(response, @"target:(.*?)(,|$)");

            // Trim leading and trailing spaces and remove non-alphabet characters
            var cleanedResponseText = Regex.Replace(responseMatch.Groups[1].Value.Trim(), @"[^a-zA-Z\s]", "");
            var cleanedActionType = Regex.Replace(actionTypeMatch.Groups[1].Value.Trim(), @"[^a-zA-Z\s]", "");
            var cleanedTarget = Regex.Replace(targetMatch.Groups[1].Value.Trim(), @"[^a-zA-Z\s]", "");

            return new Response
            {
                ResponseText = cleanedResponseText,
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
                   ", npcName : " + prompt.NpcName + ", npcDescription : " + prompt.NpcDescription + 
                   ", systemPrompt : " + prompt.SystemPrompt + ", memory1 : " + prompt.Memory1 + 
                   ", memory2 : " + prompt.Memory2 + ", memory3 : " + prompt.Memory3;
        }
    }
}