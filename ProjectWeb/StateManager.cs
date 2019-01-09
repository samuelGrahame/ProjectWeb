using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using MySql.Data.MySqlClient;
using ProjectWeb.Global;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using static ProjectWeb.Document;

namespace ProjectWeb
{
    public class StateManager
    {
        public Params Globals;
        public bool NeedBase = true;
        private ScriptState<object> state;
        public StateCache StateCache = new StateCache();

        public class MissingResolver : Microsoft.CodeAnalysis.MetadataReferenceResolver
        {
            public override bool Equals(object other)
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public override bool ResolveMissingAssemblies => false;

            public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
            {
                throw new NotImplementedException();
            }
        }

        public async Task Push(string source)
        {
            StateCache.AddedNewScript();

            if (state == null)
            {                
                var options = ScriptOptions.Default.WithReferences(
                        typeof(ProjectWeb.Mysql.Mysql).Assembly,
                        typeof(MySqlConnection).Assembly,
                        typeof(ProjectWeb.Other.Extensions).Assembly).WithImports(
                        "ProjectWeb.Mysql", "MySql.Data.MySqlClient", "ProjectWeb.Other.Extensions").
                        WithEmitDebugInformation(false).
                        WithMetadataResolver(new MissingResolver());

                state = await CSharpScript.RunAsync(source, globals: Globals, options: options);
                
                StateCache.Operations.AddLast(new ScriptOperation() { Script = state.Script });
            }
            else
            {
                state = await state.ContinueWithAsync(source);
                
                StateCache.Operations.AddLast(new ScriptOperation() { Script = state.Script });
            }
        }
    }
}
