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

$t = "";
Get-ChildItem .\Staging -Recurse -Exclude Info -File | Where-Object { $_.Directory.Name -ne "Dist" } |%{ $_.Directory.Name + "/" + $_.Name } |% { 
    (Get-StringHash ($_ + ".hash") "SHA256") + (Get-FileHash -Algorithm SHA256 "Staging/$_" |% Hash).ToLower()
} |% {
    $t = "$t$_";
}

$gt = "";
foreach ($c in $t.ToCharArray()) {
    $st = $c;
    0..(Get-Random -Maximum 4) |% {
        [char[]](Get-Random -Minimum 65 -Maximum 90)
    } |% {
        $st = "$st$_";
    }
    $gt = "$gt$st"
}

Set-Content -Path Staging/Info $gt -NoNewline

scp -r .\Staging\* vps:/var/www/html/KiraiMod/

Set-Location $ol
