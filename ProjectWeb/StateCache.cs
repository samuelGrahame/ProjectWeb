using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ProjectWeb.Document;

namespace ProjectWeb
{
    public class StateCache
    {
        public LinkedList<Operation> Operations = new LinkedList<Operation>();

        public static Dictionary<string, StateCache> Cache = new Dictionary<string, StateCache>();
        public StringBuilder builder = new StringBuilder();

        public void Append(char c)
        {
            builder.Append(c);            
        }

        public void AddedNewScript()
        {
            if(builder.Length > 0)
            {
                Operations.AddLast(new ContentOperation() { Content = builder.ToString() });
                builder.Length = 0;
            }            
        }

        public async Task<StringBuilder> Run(StateManager stateManager)
        {
            AddedNewScript();

            ScriptState state = null;
            foreach (var operation in Operations)
            {
                if(operation is ScriptOperation scriptOperation)
                {
                    if(state == null)
                    {
                        state = await scriptOperation.Script.RunAsync(stateManager.Globals);
                    }
                    else
                    {
                        state = await scriptOperation.Script.RunFromAsync(state);
                    }
                }else if (operation is ContentOperation contentOperation)
                {
                    stateManager.Builder.Append(contentOperation.Content);
                }
            }

            return stateManager.Builder;
        }
    }

    public abstract class Operation
    {
        
    }

    public class ScriptOperation : Operation
    {
        public Script Script;
    }

    public class ContentOperation : Operation
    {
        public string Content;
    }
}
