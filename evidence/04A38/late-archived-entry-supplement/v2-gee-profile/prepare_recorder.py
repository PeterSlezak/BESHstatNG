"""Derive only recorder/typed decoding code from the immutable 432 recorder."""
from inspect_entry import ROOT,HERE,pin,save

def main():
    original=ROOT/'evidence/04A38/late-archived-entry-supplement/v1/LateCapture38.cs'
    text=original.read_text(encoding='utf-8')
    edits=[]
    def replace(old,new):
        nonlocal text
        assert text.count(old)==1,old
        text=text.replace(old,new,1);edits.append(dict(original=old,replacement=new))
    replace('public static class LateCapture38 {','public static class GeeCapture38 {\n static string ActiveHandle;\n static readonly List<object> Sessions=new List<object>();')
    text=text.replace('LateCapture38.Error(e)','GeeCapture38.Error(e)')
    replace('case "Nothing":return null;','case "Nothing":return null;\n   case "HandleReference":if((string)d["session"]!="fresh-case"||ActiveHandle==null)throw new Exception("MISSING_FITTED_HANDLE");return ActiveHandle;')
    replace('if(value==null)return "N";','if(value==null)return "N";\n  if(value is string && ActiveHandle!=null && (string)value==ActiveHandle)return "HANDLE:GEE:SESSION1";')
    replace('if(result==null)return "RETURN_NULL";','if(result==null)return "RETURN_NULL";if(result is string && ActiveHandle!=null && (string)result==ActiveHandle)return "HANDLE_CREATED";if(result is string)return "RETURN_TEXT";')
    replace('Branch=Library.GetType("BESHStatNG.LateBranch38");','Branch=Library.GetType("BESHStatNG.GeeBranch38");')
    replace('var operations=new List<object>();var witnesses=new List<object>();','''var operations=new List<object>();var witnesses=new List<object>();
    ActiveHandle=null;
    var gee=Library.GetType("BESHStatNG.WorksheetFunctions.GEEUDFs",true);
    var cache=gee.GetField("_geeCache",BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);
    cache.GetType().GetMethod("Clear").Invoke(cache,null);
    int modelCreations=0;''')
    replace('var actual=target.Invoke(null,supplied);kind=Kind(actual);result=Snap(actual);','''var actual=target.Invoke(null,supplied);
      if((string)op["target"]=="BESH.REGR.GEE_FIT") {
       ActiveHandle=actual as string;
       if(ActiveHandle==null||!System.Text.RegularExpressions.Regex.IsMatch(ActiveHandle,@"^GEE:[0-9a-f]{32}$"))throw new Exception("FIT_DID_NOT_SUCCEED:"+Snap(actual));
       modelCreations++;
       var h=cache.GetType().GetProperty("Item").GetValue(cache,new object[]{ActiveHandle});
       Func<string,object> property=name=>h.GetType().GetProperty(name).GetValue(h,null);
       var names=(string[])property("VarNames");
       var parameters=(string[])gee.GetMethod("BuildParameterNames",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{names,((double[,])property("FittedDesign")).GetLength(1)});
       if(!parameters.Contains("X1"))throw new Exception("APPROVED_DESIGN_COLUMN_X1_MISSING");
       Sessions.Add(new{case_id=(string)c["case_id"],raw_handle=ActiveHandle,symbol="HANDLE:GEE:SESSION1",parameter_names=parameters,
        fitted_design=Snap(property("FittedDesign")),fitted_time=Snap(property("FittedTimeValues")),has_time=Snap(property("HasTime")),alpha=Snap(property("Alpha")),creation_order=modelCreations});
      }
      kind=Kind(actual);result=Snap(actual);''')
    replace('writer.WriteLine((string)c["case_id"]','if(modelCreations!=1)throw new Exception("EXACTLY_ONE_SUCCESSFUL_FIT_REQUIRED");\n    writer.WriteLine((string)c["case_id"]')
    replace('if(count!=432)throw new Exception("CASE_COUNT_MISMATCH");','if(count!=12)throw new Exception("CASE_COUNT_MISMATCH");\n  File.WriteAllText(output+".sessions.json",json.Serialize(Sessions),new UTF8Encoding(false));')
    replace('Console.WriteLine("Captured432 "+role','Console.WriteLine("Captured12 "+role')
    with (HERE/'GeeCapture38.cs').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
    save('recorder-derivation.json',dict(source=pin(original),derived=pin(HERE/'GeeCapture38.cs'),edits=edits,
         handle_encoding='Raw random GUID handle retained in sessions sidecar; output uses a declared session-relative symbol bound to exactly one successful actual GEE_FIT. No statistical value normalization.',
         replaced_statistical_logic=False))
    print('Recorder prepared; no model or query executed')

if __name__=='__main__':main()
