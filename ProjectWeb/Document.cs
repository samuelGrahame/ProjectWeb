using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWeb
{
    public class Document
    {
        public const string ScriptStart = "<?cs";
        public const string ScriptEnd = "?>";

        public const string ScriptInclude = "include";
        public const string ScriptIncludeOnce = "include_once";

        public class IncludedFiles
        {
            public List<string> Files = new List<string>();
        }

        public static async Task<StringBuilder> ParseAsync(string data, StateManager stateManager, IncludedFiles includedFiles, bool isInclude = false)
        {            
            if (stateManager.NeedBase)
            {                
                stateManager.NeedBase = false;
                // TODO - make this as base...
                await stateManager.Push("void Echo(string input) { builder.Append(input); } void Echo(byte[] buffer) { context.Response.ContentLength64 = buffer.Length; Context.Response.OutputStream.Write(buffer, 0, buffer.Length); } ");                
            }

            var builder = stateManager.Globals.builder;

            var codeBuilder = new StringBuilder();
            int length = data.Length;

            int startLength = ScriptStart.Length;
            int endLength = ScriptEnd.Length;
            int includeLength = ScriptInclude.Length;
            int includeOnceLength = ScriptIncludeOnce.Length;

            bool inServerCode = false;

            string content;

            for (int i = 0; i < length; i++)
            {
                if(inServerCode)
                {
                    if (i + endLength <= length)
                    {
                        if (data[i] == '?' && data[i + 1] == '>')
                        {
                            i += 1;
                            if (codeBuilder.Length > 0)
                            {
                                content = codeBuilder.ToString();
                                if(!string.IsNullOrWhiteSpace(content))
                                {           
                                    if(!isInclude)
                                    {                                        
                                        await stateManager.Push(codeBuilder.ToString());
                                    }
                                    
                                }
                                if(!isInclude)
                                    codeBuilder.Length = 0;
                            }
                            
                            inServerCode = false;
                            continue;                            
                        }
                    }

                    if(i + includeLength <= length &&
                        data[i] == 'i' && data[i + 1] == 'n' && data[i + 2] == 'c' && data[i + 3] == 'l' && data[i + 4] == 'u' && data[i + 5] == 'd' & data[i + 6] == 'e')
                    {
                        bool includeOnce = false;
                        if(i + includeOnceLength <= length)
                        {
                            if(data[i + 7] == '_' && data[i + 8] == 'o' && data[i + 9] == 'n' && data[i + 10] == 'c' && data[i + 11] == 'e')
                            {
                                includeOnce = true;
                            }
                        }

                        int x;
                        for(x = i; x < length && data[x] != '"'; x++)
                        { }

                        if(x < length && data[x] == '"')
                        {
                            var path = new StringBuilder();
                            for (x = x + 1; x < length && data[x] != '"'; x++)
                            {
                                path.Append(data[x]);
                            }
                            if (x < length && data[x] == '"')
                            {
                                int increment = 2; 
                                if (x + 1 < length && data[x + 1] == ';' || (x + 2 < length && data[x + 2] == ';' && (increment = 3) == 3))
                                {
                                    i = x + increment; // just incase they have ) at the end for php support.

                                    var file = path.ToString().ToLower(); // RIP LINUX
                                    if (!includedFiles.Files.Contains(file))
                                    {
                                        includedFiles.Files.Add(file);
                                    }
                                    else
                                    {
                                        if(includeOnce)
                                        {
                                            continue;
                                        }
                                    }                                    
                                    
                                    codeBuilder.Append(await ParseAsync(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/wwwroot/" + file), stateManager, includedFiles, true));


                                    continue;
                                }                                
                            }
                        }
                    }

                    codeBuilder.Append(data[i]);
                }
                else
                {
                    if (i + startLength <= length)
                    {
                        if (data[i] == '<' && data[i + 1] == '?' && data[i + 2] == 'c' && data[i + 3] == 's')
                        {
                            i += 3;
                            inServerCode = true;
                            continue;
                        }
                    }
                    stateManager.StateCache.Append(data[i]);
                    builder.Append(data[i]);
                }
            }

            return isInclude ? codeBuilder : builder;
        }      
    }
}
