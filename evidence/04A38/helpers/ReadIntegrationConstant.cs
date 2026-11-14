using System;
using System.Reflection;
public static class ReadIntegrationConstant {
 public static int Main(string[] args){
  try{var assembly=Assembly.ReflectionOnlyLoadFrom(args[0]);Console.WriteLine(assembly.GetType("ExcelDna.Integration.XlCall").GetField("xlCoerce").GetRawConstantValue());return 0;}
  catch(Exception ex){Console.Error.WriteLine(ex.GetType().FullName+": "+ex.Message);return 1;}
 }
}
