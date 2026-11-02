# Read-only C08 output/ancestor evidence. Does not infer independent review.
param([Parameter(Mandatory=$true)][string]$OutputRoot)
$ErrorActionPreference = 'Stop'
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$target = [IO.Path]::GetFullPath($OutputRoot)
$chain = New-Object System.Collections.Generic.List[string]
$cursor = $target
while ($cursor) {
    $chain.Insert(0, $cursor)
    $parent = [IO.Directory]::GetParent($cursor)
    $cursor = if ($null -eq $parent) { $null } else { $parent.FullName }
}
$rows = foreach ($path in $chain) {
    try {
        $item = Get-Item -LiteralPath $path -Force -ErrorAction Stop
        $acl = Get-Acl -LiteralPath $path -ErrorAction Stop
        $access = @($acl.Access | ForEach-Object {
            [pscustomobject]@{ sid=$_.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value
                rights=[long]$_.FileSystemRights; type=$_.AccessControlType.ToString()
                inherited=$_.IsInherited; inheritance=$_.InheritanceFlags.ToString(); propagation=$_.PropagationFlags.ToString() }
        })
        [pscustomobject]@{path=$path; owner_sid=$acl.GetOwner([Security.Principal.SecurityIdentifier]).Value
            sddl=$acl.Sddl; protected=$acl.AreAccessRulesProtected
            reparse=[bool]($item.Attributes -band [IO.FileAttributes]::ReparsePoint); access=$access
            effective_modify_review='NOT_INDEPENDENTLY_REVIEWED'; unapproved_modify_sids=$null}
    } catch {
        [pscustomobject]@{path=$path;error=$_.Exception.Message;effective_modify_review='UNVERIFIABLE'}
    }
}
[pscustomobject]@{schema='04A02-C08-output-acl-observation-v1';output_root=$target
    execution_sid=$identity.User.Value; execution_name=$identity.Name
    group_sids=@($identity.Groups | ForEach-Object {$_.Value});ancestors=@($rows)
    independent_review_source=$null; reviewer=$null
    limitation='Raw ACE/token evidence only. Effective Modify, inheritance, DELETE_CHILD/rename and owner-context review are unresolved; no ACL changed.'} | ConvertTo-Json -Depth 8 -Compress
