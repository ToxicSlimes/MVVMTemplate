using Core.Network.FusionAdapter.Runner;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
class Logger : Core.Abstractions.Services.ILogger { public void Error(string f, params object[] a) {} public void Info(string f, params object[] a) {} }
class Mediator : Core.Mediator.Abstractions.IMediator { public UniTask Publish(object n) => new(Task.CompletedTask); }
class Program {
 static int failures;
 static FusionLobbyController New() { GameObject.All.Clear(); NetworkRunner.All.Clear(); NetworkRunner.OnAddCallbacks = null; return new(new Mediator(), new Logger()); }
 static void Check(bool ok, string msg) { if (!ok) throw new Exception(msg); }
 static async Task Throws<T>(Task task) where T:Exception { try { await task; } catch(T) { return; } throw new Exception("exception not propagated"); }
 static async Task Run(string name, Func<Task> test) { try { await test(); Console.WriteLine("PASS "+name); } catch(Exception e) { failures++; Console.WriteLine("FAIL "+name+": "+e.Message); } }
 static async Task Main() {
 await Run("pre-join initialization failure cleans object and releases guard", async () => { var c=New(); var original=new InvalidOperationException(); NetworkRunner.OnAddCallbacks=()=>throw original; try { await c.JoinAsync(); throw new Exception("exception not propagated"); } catch(InvalidOperationException e) { Check(ReferenceEquals(original,e),"changed exception"); } Check(GameObject.All[0].Destroyed,"pre-join object retained"); NetworkRunner.OnAddCallbacks=null; NetworkRunner.Join=(r,t)=>{ r.LobbyInfo.IsValid=true; return Task.FromResult(new StartGameResult { Ok=true }); }; Check((await c.JoinAsync()).IsOk,"retry failed"); });
 await Run("success and competing join preserve runner", async () => { var c=New(); var pending=new TaskCompletionSource<StartGameResult>(); NetworkRunner.Join=(r,t)=>pending.Task; var first=c.JoinAsync().AsTask(); await Throws<InvalidOperationException>(c.JoinAsync().AsTask()); Check(!GameObject.All[0].Destroyed && NetworkRunner.All[0].Calls==1,"parallel join changed runner"); NetworkRunner.All[0].LobbyInfo.IsValid=true; pending.SetResult(new StartGameResult { Ok=true }); Check((await first).IsOk && (await c.JoinAsync()).IsOk && NetworkRunner.All.Count==1 && !GameObject.All[0].Destroyed,"successful runner lost"); });
 await Run("join fault does not destroy possibly active runner", async () => { var c=New(); NetworkRunner.Join=(r,t)=>Task.FromException<StartGameResult>(new InvalidOperationException()); await Throws<InvalidOperationException>(c.JoinAsync().AsTask()); Check(!GameObject.All[0].Destroyed,"possibly active runner destroyed"); });
 await Run("join cancellation does not destroy possibly active runner", async () => { var c=New(); var pending=new TaskCompletionSource<StartGameResult>(); NetworkRunner.Join=(r,t)=>pending.Task; var join=c.JoinAsync().AsTask(); pending.SetCanceled(); await Throws<OperationCanceledException>(join); Check(!GameObject.All[0].Destroyed,"possibly active runner destroyed"); });
 Console.WriteLine($"Failures: {failures}"); Environment.ExitCode=failures==0?0:1;
 }
}