// Review-only N1 implementation. Compiled/host-free checked; no native call without stage GO.
// Exact-process NativeOM binding; no ROT, coordinates, SendKeys, Quit or COM release.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Automation;
public static class C09NativeN1 {
    static JavaScriptSerializer json=new JavaScriptSerializer {MaxJsonLength=4*1024*1024};
    static Dictionary<string,object> plan;
    static Process excel;
    static string output;
    static Stopwatch clock=Stopwatch.StartNew();
    static IntPtr hwnd;
    static HashSet<string> seenWindows=new HashSet<string>();
    static List<FileStream> immutableInputs=new List<FileStream>();
    static string executablePath;
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint pid);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string cls,string title);
    delegate bool EnumProc(IntPtr h,IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent,EnumProc callback,IntPtr data);
    [DllImport("oleacc.dll")] static extern int AccessibleObjectFromWindow(IntPtr h,uint id,ref Guid iid,[MarshalAs(UnmanagedType.Interface)] out object value);
    [DllImport("advapi32.dll",SetLastError=true)] static extern bool OpenProcessToken(IntPtr p,uint access,out IntPtr token);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
    static void Need(bool ok,string error) {if(!ok)throw new InvalidOperationException(error);}
    static string S(string key){return Convert.ToString(plan[key]);}
    static string Hash(string path) {
        using(var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
        using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
    }
    static void LockInput(string path,string expected) {
        var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
        immutableInputs.Add(stream);
        using(var h=SHA256.Create())Need(BitConverter.ToString(h.ComputeHash(stream)).Replace("-","").ToLowerInvariant()==expected,"Immutable native input drift");
        stream.Position=0;
    }
    static void Write(string name,object value) {
        using(var f=new FileStream(Path.Combine(output,name),FileMode.CreateNew,FileAccess.Write,FileShare.Read)) {
            var bytes=new UTF8Encoding(false).GetBytes(json.Serialize(value));f.Write(bytes,0,bytes.Length);f.Flush(true);
        }
    }
    static string Owner(Process process) {
        IntPtr token;Need(OpenProcessToken(process.Handle,8,out token),"Cannot verify owned process token");
        try {using(var identity=new WindowsIdentity(token))return identity.User.Value;}finally{CloseHandle(token);}
    }
    static void Context() {
        Need(clock.Elapsed.TotalSeconds<15,"N1 deadline");excel.Refresh();
        Need(!excel.HasExited && excel.Id==Convert.ToInt32(plan["pid"]) &&
             excel.StartTime.ToUniversalTime().Ticks==Convert.ToInt64(plan["start_utc_ticks"]) &&
             excel.SessionId==Convert.ToInt32(plan["session_id"]),"PID/start/session drift");
        Need(Owner(excel)==S("owner_sid") && WindowsIdentity.GetCurrent().User.Value==S("owner_sid"),"Process owner mismatch");
        Need(excel.MainModule.FileName==executablePath && immutableInputs.Count==2,"Executable/held XLL context drift");
        if(hwnd!=IntPtr.Zero) {
            uint pid;GetWindowThreadProcessId(hwnd,out pid);Need(pid==excel.Id,"Window PID drift");
            Need(excel.MainWindowHandle==hwnd && excel.MainWindowTitle==S("window_title"),"Window identity/title drift");
        }
    }
    static T Read<T>(Func<T> getter) {Context();T result=getter();Context();return result;}
    static object Typed(object value) {
        if(value==null)return new {type="null",value=(object)null};
        var a=value as Array;
        if(a!=null){var cells=new List<object>();foreach(object item in a)cells.Add(Typed(item));return new {type=value.GetType().FullName,rank=a.Rank,lengths=Enumerable.Range(0,a.Rank).Select(a.GetLength).ToArray(),cells=cells};}
        var error=value as ErrorWrapper;
        if(error!=null)return new {type="ErrorWrapper",native_error=error.ErrorCode};
        if(value is string || value is bool || value is double || value is int || value is decimal)return new {type=value.GetType().FullName,value=value};
        throw new InvalidOperationException("Unapproved provider value type: "+value.GetType().FullName);
    }
    static object Workbook(dynamic app) {
        Need(Read(()=> (int)app.Workbooks.Count)==1,"Unexpected workbook count");
        dynamic book=Read<object>(()=>(object)app.ActiveWorkbook);
        Need(Read(()=> (string)book.FullName)==S("workbook_path"),"Wrong active workbook");
        var sheets=new List<object>();int n=Read(()=> (int)book.Worksheets.Count);Need(n>0 && n<=10,"Sheet bound");
        for(int i=1;i<=n;i++) {
            int index=i;dynamic sheet=Read<object>(()=>(object)book.Worksheets[index]);
            string name=Read(()=> (string)sheet.Name),code=Read(()=> (string)sheet.CodeName);
            dynamic used=Read<object>(()=>(object)sheet.UsedRange);
            int rows=Read(()=> (int)used.Rows.Count),cols=Read(()=> (int)used.Columns.Count);
            Need(rows<=20 && cols<=20,"Unexpected UsedRange exceeds disposable-case bounds");
            sheets.Add(new {name=name,code_name=code,address=Read(()=> (string)used.Address),rows=rows,columns=cols,
                values=Typed(Read<object>(()=>(object)used.Value2)),formulas=Typed(Read<object>(()=>(object)used.Formula)),
                number_format=Typed(Read<object>(()=>(object)used.NumberFormat))});
        }
        var names=new List<object>();int count=Read(()=> (int)book.Names.Count);Need(count<=100,"Name bound");
        for(int i=1;i<=count;i++){int index=i;dynamic item=Read<object>(()=>(object)book.Names.Item(index));names.Add(new {name=Read(()=> (string)item.Name),refers_to=Read(()=> (string)item.RefersTo)});}
        var addins=new List<string>();int adds=Read(()=> (int)app.AddIns.Count);Need(adds<=100,"Addin bound");
        for(int i=1;i<=adds;i++){int index=i;dynamic item=Read<object>(()=>(object)app.AddIns.Item(index));if(Read(()=> (bool)item.Installed))addins.Add(Read(()=> (string)item.FullName));}
        string[] allowed=((IEnumerable)plan["allowed_addins"]).Cast<object>().Select(Convert.ToString).OrderBy(x=>x,StringComparer.OrdinalIgnoreCase).ToArray();
        Need(addins.OrderBy(x=>x,StringComparer.OrdinalIgnoreCase).SequenceEqual(allowed,StringComparer.OrdinalIgnoreCase),"Unexpected addins");
        return new {full_name=S("workbook_path"),saved=Read(()=> (bool)book.Saved),sheets=sheets,names=names,
            addins=addins,file_sha256=Hash(S("workbook_path")),culture=System.Globalization.CultureInfo.CurrentCulture.Name};
    }
    static List<object> Controls() {
        Context();var result=new List<object>();
        seenWindows.Clear();
        var allowed=new HashSet<string>(((IEnumerable)plan["allowed_uia_keys"]).Cast<object>().Select(Convert.ToString));
        var windows=Read(()=>AutomationElement.RootElement.FindAll(TreeScope.Children,
            new PropertyCondition(AutomationElement.ProcessIdProperty,excel.Id)));
        Need(windows.Count>0 && windows.Count<=8,"Unexpected owned window count");
        for(int j=0;j<windows.Count;j++) {
            AutomationElement window=windows[j];object pattern=null;string windowState=null;
            string windowName=Read(()=>window.Current.Name);seenWindows.Add(windowName);
            if(Read(()=>window.TryGetCurrentPattern(WindowPattern.Pattern,out pattern))) {
                Need(!Read(()=>((WindowPattern)pattern).Current.IsModal),"Unexpected modal; never dismiss");
                windowState=Read(()=>((WindowPattern)pattern).Current.WindowVisualState.ToString());
            }
            var elements=Read(()=>window.FindAll(TreeScope.Subtree,Condition.TrueCondition));Need(elements.Count<=1500,"Unbounded provider tree");
            for(int i=0;i<elements.Count;i++) {
                var e=elements[i];Need(Read(()=>e.Current.ProcessId)==excel.Id,"Provider escaped owned process");
                string framework=Read(()=>e.Current.FrameworkId),id=Read(()=>e.Current.AutomationId),name=Read(()=>e.Current.Name),kind=Read(()=>e.Current.ControlType.ProgrammaticName);
                bool enabled=Read(()=>e.Current.IsEnabled),visible=!Read(()=>e.Current.IsOffscreen);
                string key=windowName+"\u001f"+framework+"\u001f"+id+"\u001f"+name+"\u001f"+kind;
                Need(allowed.Contains(key),"Unknown provider/AutomationId/context; halt without fallback");
                object valuePattern=null;object value=null,toggle=null;
                if(Read(()=>e.TryGetCurrentPattern(ValuePattern.Pattern,out valuePattern))) {
                    value=Read(()=>((ValuePattern)valuePattern).Current.Value);
                }
                if(Read(()=>e.TryGetCurrentPattern(TogglePattern.Pattern,out valuePattern))) {
                    toggle=Read(()=>((TogglePattern)valuePattern).Current.ToggleState.ToString());
                }
                result.Add(new {automation_id=id,name=name,enabled=enabled,visible=visible,
                    value=value,toggle=toggle,window=windowName,control_type=kind,framework=framework,window_state=windowState,
                    native_hwnd=Read(()=>e.Current.NativeWindowHandle),bounds=Read(()=>e.Current.BoundingRectangle.ToString())});
            }
        }
        return result;
    }
    [STAThread] public static int Main(string[] args) {
        if(args.Length==1 && args[0]=="--self-test") {
            // Actual native methods are deliberately never called by this self-test.
            Need(json.Serialize(Typed(new object[,]{{1.0,"x"},{null,new ErrorWrapper(2042)}})).Contains("native_error"),"Typed error serialization");
            Console.WriteLine("PASS: typed array/null/error serialization; zero COM/UIA/process calls");return 0;
        }
        try {
            Need(args.Length==3,"Pinned plan/output/hash required");output=Path.GetFullPath(args[1]);
            Need(Directory.Exists(output) && !Directory.EnumerateFileSystemEntries(output).Any(),"Fresh empty helper output required");
            Need(Hash(args[0])==args[2],"Plan hash drift");plan=json.Deserialize<Dictionary<string,object>>(File.ReadAllText(args[0]));
            Need(S("schema")=="04A02-C09-owned-N1-context-v1" && S("stage")=="N1" && S("case_id")=="04A02-2X2-NATIVE-01","Exact N1 only");
            Need(S("risk_decision")=="INDEPENDENTLY_ACCEPTED_N1_ONLY" && !string.IsNullOrWhiteSpace(S("risk_source")),"Independent risk acceptance absent");
            Need(Hash(Process.GetCurrentProcess().MainModule.FileName)==S("helper_sha256"),"Helper hash drift");
            excel=Process.GetProcessById(Convert.ToInt32(plan["pid"]));executablePath=excel.MainModule.FileName;
            LockInput(executablePath,S("excel_sha256"));LockInput(S("xll_path"),S("xll_sha256"));
            Context();hwnd=excel.MainWindowHandle;Need(hwnd!=IntPtr.Zero,"Missing owned Excel window");Context();
            Need(Hash(S("workbook_path"))==S("workbook_sha256"),"Workbook pre-hash drift");
            // Exact native object model binding via an EXCEL7 child of this known owned window.
            IntPtr excel7=IntPtr.Zero;
            EnumChildWindows(hwnd,(h,d)=>{if(FindWindowEx(h,IntPtr.Zero,"EXCEL7",null)!=IntPtr.Zero)excel7=FindWindowEx(h,IntPtr.Zero,"EXCEL7",null);return true;},IntPtr.Zero);
            if(excel7==IntPtr.Zero)excel7=FindWindowEx(hwnd,IntPtr.Zero,"EXCEL7",null);
            Need(excel7!=IntPtr.Zero,"Unsupported exact-instance native window");uint boundPid;GetWindowThreadProcessId(excel7,out boundPid);Need(boundPid==excel.Id,"NativeOM child PID mismatch");
            object native;Guid iid=new Guid("00020400-0000-0000-C000-000000000046");Context();
            Marshal.ThrowExceptionForHR(AccessibleObjectFromWindow(excel7,0xfffffff0,ref iid,out native));Context();
            dynamic nativeWindow=native;dynamic app=Read<object>(()=>(object)nativeWindow.Application);
            uint appPid;GetWindowThreadProcessId(new IntPtr(Read(()=> (int)app.Hwnd)),out appPid);Need(appPid==excel.Id,"NativeOM application escaped owned instance");
            var before=Workbook(app);Write("before.json",before);Write("initial-uia.json",Controls());
            var root=Read(()=>AutomationElement.FromHandle(hwnd));
            var ribbon=Read(()=>root.FindAll(TreeScope.Descendants,new AndCondition(
                new PropertyCondition(AutomationElement.AutomationIdProperty,S("ribbon_automation_id")),
                new PropertyCondition(AutomationElement.NameProperty,S("ribbon_name")))));
            Need(ribbon.Count==1,"Unknown/ambiguous reviewed ribbon ID; no fallback");
            object invoke=null;Need(Read(()=>ribbon[0].TryGetCurrentPattern(InvokePattern.Pattern,out invoke)),"Unsupported ribbon invoke provider");
            Context();((InvokePattern)invoke).Invoke();Context();
            var initialized=Controls();Write("initialized-uia.json",initialized);
            Need(seenWindows.Contains(S("form_window_name")),"Initialized form not observed; no native pass");
            var after=Workbook(app);Write("after.json",after);
            Need(json.Serialize(before)==json.Serialize(after),"Observable workbook change; retain and halt");
            Write("result.json",new {status="NATIVE_N1_CAPTURED_UNREVIEWED_NOT_GOLDEN",N2="NO",pid=excel.Id,
                limitations="No guarantee about transient/provider/external changes outside recorded observations. No save/restore/write/cleanup issued."});
            return 0;
        } catch(Exception e) {
            if(output!=null && Directory.Exists(output)) {
                try{Write("failure.json",new {status="FAILED_RETAINED_NO_RETRY",error=e.ToString()});}catch(IOException){}
            }
            Console.Error.WriteLine(e.Message);return 1;
        } finally {foreach(var stream in immutableInputs)stream.Dispose();}
    }
}
