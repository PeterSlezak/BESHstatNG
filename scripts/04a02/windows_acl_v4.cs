// Read-only Windows security evaluation for the actual process token. No impersonation of another user.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
public static class C09Access {
    [StructLayout(LayoutKind.Sequential)] struct Mapping { public uint Read, Write, Execute, All; }
    [DllImport("advapi32.dll", SetLastError=true)] static extern bool DuplicateToken(IntPtr token, int level, out IntPtr copy);
    [DllImport("advapi32.dll", SetLastError=true)] static extern bool AccessCheck(byte[] descriptor, IntPtr token, uint desired,
        ref Mapping mapping, IntPtr privileges, ref uint size, out uint granted, out bool status);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
    public static Dictionary<string,object> Evaluate(byte[] descriptor) {
        IntPtr token;
        if (!DuplicateToken(WindowsIdentity.GetCurrent().Token, 2, out token)) throw new Win32Exception();
        IntPtr buffer=IntPtr.Zero;
        try {
            uint size=4096, granted;
            buffer=Marshal.AllocHGlobal((int)size);
            var mapping=new Mapping {Read=0x120089,Write=0x120116,Execute=0x1200a0,All=0x1f01ff};
            bool status;
            if (!AccessCheck(descriptor,token,0x02000000,ref mapping,buffer,ref size,out granted,out status))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return new Dictionary<string,object> {
                {"maximum_allowed",granted},{"access_status",status},
                {"create_file",(granted&2)!=0},{"create_directory",(granted&4)!=0},
                {"delete_child",(granted&0x40)!=0},{"delete_self",(granted&0x10000)!=0},
                {"write_dacl",(granted&0x40000)!=0},{"write_owner",(granted&0x80000)!=0},
                {"read_control",(granted&0x20000)!=0},
                {"scope","AccessCheck for this actual token only; rename/replacement also depends on parent DELETE_CHILD or object DELETE, sharing locks and attributes."}
            };
        } finally { if(buffer!=IntPtr.Zero) Marshal.FreeHGlobal(buffer);CloseHandle(token); }
    }
}
