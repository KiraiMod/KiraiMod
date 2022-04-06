#http://jongurgul.com/blog/get-stringhash-get-filehash/
Function Get-StringHash([String] $String,$HashName = "MD5")
{
  $StringBuilder = New-Object System.Text.StringBuilder
  [System.Security.Cryptography.HashAlgorithm]::Create($HashName).ComputeHash([System.Text.Encoding]::UTF8.GetBytes($String))|%{
    [Void]$StringBuilder.Append($_.ToString("x2"))
  }
  $StringBuilder.ToString()
}

$ol = Get-Location |% Path
Set-Location Dist
$t = ""

Write-Host "Generating info..."
Get-ChildItem |% Name |% { 
    $nh = (Get-StringHash ($_ + ".hash") "SHA256")
    $fh = (Get-FileHash -Algorithm SHA256 $_ |% Hash).ToLower()
    Write-Host ("`t" + $_.ToString().PadRight(22) + "$nh $fh")
    $nh + $fh
} |% {
    $t = "$t$_";
}

Write-Host "Adding entropy..."
$ca = $t.ToCharArray()
$t = ""
foreach ($c in $ca) {
    $st = $c;
    0..(Get-Random -Maximum 4) |% { 
        [char[]](Get-Random -Minimum 65 -Maximum 90)
    } |% { $st = "$st$_"; }
    $t = "$t$st"
}

Write-Host "Creating name table..."
$t = "$t;" + ([System.String]::Join(";", (Get-ChildItem |% Name)))

Set-Content -Path Info $t -NoNewline

scp -r ../Dist/* vm0:/var/www/html/KiraiMod/

Set-Location $ol
