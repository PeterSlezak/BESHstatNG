# Read-only collection (stdout only). Does not allocate roots, modify ACLs or approve a human review.
param([Parameter(Mandatory=$true)][string]$OutputRoot)
$ErrorActionPreference='Stop'
if (-not ('C09Access' -as [type])) { Add-Type -Path (Join-Path $PSScriptRoot 'windows_acl_v4.cs') }
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
$target=[IO.Path]::GetFullPath($OutputRoot)
if ($target.StartsWith('\\') -or $target -match '\\[.]\.?\\') { throw 'Local canonical absolute root required.' }
$chain=New-Object 'System.Collections.Generic.List[string]'
$cursor=$target
while ($cursor) { $chain.Insert(0,$cursor); $parent=[IO.Directory]::GetParent($cursor); $cursor=if($parent){$parent.FullName}else{$null} }
$rows=foreach($path in $chain) {
    try {
        $item=Get-Item -LiteralPath $path -Force -ErrorAction Stop
        if (-not $item.PSIsContainer) { throw 'Not a directory' }
        $acl=Get-Acl -LiteralPath $path -ErrorAction Stop
        $access=@($acl.Access | ForEach-Object {
            [pscustomobject]@{sid=$_.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value
                rights=([long]$_.FileSystemRights -band 0xffffffffL);type=$_.AccessControlType.ToString()
                inherited=$_.IsInherited;inheritance=$_.InheritanceFlags.ToString();propagation=$_.PropagationFlags.ToString()}
        })
        [pscustomobject]@{path=$path;owner_sid=$acl.GetOwner([Security.Principal.SecurityIdentifier]).Value
            sddl=$acl.Sddl;canonical=$acl.AreAccessRulesCanonical;protected=$acl.AreAccessRulesProtected
            reparse=[bool]($item.Attributes -band [IO.FileAttributes]::ReparsePoint)
            attributes=[long]$item.Attributes;access=$access
            current_token_effective=[C09Access]::Evaluate($acl.GetSecurityDescriptorBinaryForm())}
    } catch { [pscustomobject]@{path=$path;error=$_.Exception.Message} }
}
$repo=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$owner=(Get-Acl -LiteralPath $repo).GetOwner([Security.Principal.SecurityIdentifier]).Value
[pscustomobject]@{schema='04A02-C09-output-acl-observation-v4';output_root=$target
    execution_sid=$identity.User.Value;execution_name=$identity.Name;repository_owner_sid=$owner
    group_sids=@($identity.Groups|ForEach-Object {$_.Value});ancestors=@($rows)
    reviewer=$null;review_source=$null
    limitation='Current-token effective access is measured. Other identities are conservative raw-ACE candidates, not fabricated AccessCheck results. OS administrators/SYSTEM/TrustedInstaller are explicit trusted maintenance principals.'} | ConvertTo-Json -Depth 10 -Compress
